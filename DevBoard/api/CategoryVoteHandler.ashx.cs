using DevBoard.Core.Models;
using System;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace DevBoard
{
    /// <summary>
    /// POST /CategoryVoteHandler.ashx
    ///   categoryId – int
    ///   value      – 1 or -1
    /// Returns JSON: { netVotes, stressScore, upvotes, downvotes, userVote }
    /// </summary>
    public class CategoryVoteHandler : IHttpHandler
    {
        private static readonly Dictionary<string, int> VoteWeights = new Dictionary<string, int>
        {
            { "Admin", 10 }, { "QA", 3 }, { "Dev", 1 }, { "Stakeholder", 1 }
        };

        public bool IsReusable => false;

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            if (!context.Request.IsAuthenticated)
            {
                context.Response.StatusCode = 401;
                context.Response.Write("{\"error\":\"Unauthorized\"}");
                return;
            }

            try
            {
                int    categoryId = int.Parse(context.Request.Form["categoryId"]);
                int    value      = int.Parse(context.Request.Form["value"]);
                string userId     = context.User.Identity.Name;

                // Determine voter weight
                int userWeight = 1;
                var roles = System.Web.Security.Roles.GetRolesForUser(userId);
                foreach (var role in roles)
                    if (VoteWeights.TryGetValue(role, out int rw) && rw > userWeight)
                        userWeight = rw;

                using (var ctx = new DevBoardContext())
                {
                    var category = ctx.Categories
                        .Include("Votes")
                        .Include("Tickets")
                        .FirstOrDefault(c => c.Id == categoryId);

                    if (category == null)
                    {
                        context.Response.StatusCode = 404;
                        context.Response.Write("{\"error\":\"Category not found\"}");
                        return;
                    }

                    // Apply vote (toggle/flip/add)
                    var existing = ctx.CategoryVotes.FirstOrDefault(v => v.CategoryId == categoryId && v.UserId == userId);
                    if (existing != null && existing.Value == value)
                        ctx.CategoryVotes.Remove(existing);
                    else if (existing != null)
                        existing.Value = value;
                    else
                        ctx.CategoryVotes.Add(new CategoryVote { CategoryId = categoryId, UserId = userId, Value = value, CreatedAt = DateTime.UtcNow });

                    ctx.SaveChanges();

                    // Reload votes after save
                    ctx.Entry(category).Collection("Votes").Load();

                    // Compute weighted net votes (role-weighted)
                    int weightedNet = 0;
                    foreach (var v in category.Votes)
                    {
                        int w = 1;
                        var vRoles = System.Web.Security.Roles.GetRolesForUser(v.UserId);
                        foreach (var r in vRoles)
                            if (VoteWeights.TryGetValue(r, out int rw) && rw > w) w = rw;
                            // Invert vote direction: Downvote (-1) means "Unstable" (increases stress)
                            weightedNet += v.Value * -1 * w;
                    }

                    int openTickets = category.Tickets.Count(t => t.Status != Status.Done);

                    // ── Gravity Well formula (per votingsys.md §2) ────────────────────────────
                    // Sc = (CategoryVotes × Wu) + (ΣTicketBoosts × 0.2)
                    // Load upvotes on open tickets in this category
                    var openTicketIds = category.Tickets
                        .Where(t => t.Status != Status.Done)
                        .Select(t => t.Id)
                        .ToList();
                    var ticketBoosts = openTicketIds.Any()
                        ? ctx.TicketVotes.Count(tv => openTicketIds.Contains(tv.TicketId) && tv.Value == 1)
                        : 0;
                    decimal ticketPenalty = ticketBoosts * 0.2m;

                    // Sc = weighted category votes + ticket boost penalty, clamped ≥ 0
                    decimal sc = Math.Max(0m, (decimal)weightedNet + ticketPenalty);

                    int upvotes   = category.Votes.Count(v => v.Value == 1);
                    int downvotes = category.Votes.Count(v => v.Value == -1);
                    int userVote  = category.Votes.FirstOrDefault(v => v.UserId == userId)?.Value ?? 0;

                    var result = new
                    {
                        netVotes          = weightedNet,
                        stressScore       = (double)Math.Round(sc, 4),
                        ticketBoostPenalty = (double)Math.Round(ticketPenalty, 4),
                        upvotes,
                        downvotes,
                        userVote,
                        isHighRisk        = sc > 0.5m
                    };

                    context.Response.Write(new JavaScriptSerializer().Serialize(result));
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                context.Response.Write("{\"error\":\"" + ex.Message.Replace("\"", "'") + "\"}");
            }
        }
    }
}
