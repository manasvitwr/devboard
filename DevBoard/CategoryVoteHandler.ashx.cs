using DevBoard.Models;
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
                        weightedNet += v.Value * w;
                    }

                    int openTickets = category.Tickets.Count(t => t.Status != Status.Done);

                    // Sc = max(0, (WeightedVotes + OpenTickets * SeverityMultiplier) / 100)
                    decimal sc = Math.Max(0m,
                        ((decimal)weightedNet + (openTickets * category.SeverityMultiplier)) / 100m);

                    int upvotes   = category.Votes.Count(v => v.Value == 1);
                    int downvotes = category.Votes.Count(v => v.Value == -1);
                    int userVote  = category.Votes.FirstOrDefault(v => v.UserId == userId)?.Value ?? 0;

                    var result = new
                    {
                        netVotes    = weightedNet,
                        stressScore = (double)Math.Round(sc, 4),
                        upvotes,
                        downvotes,
                        userVote,
                        isHighRisk  = sc > 0.5m
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
