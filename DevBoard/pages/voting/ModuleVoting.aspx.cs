using DevBoard.Core.Models;
using DevBoard.Core.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

namespace DevBoard.Pages
{
    public partial class ModuleVoting : System.Web.UI.Page
    {
        // ── Role → vote weight per votingsys.md ──────────────────────────
        // Admin=10, QA=3, Dev=1, Stakeholder=1
        private static readonly Dictionary<string, int> VoteWeights = new Dictionary<string, int>
        {
            { "Admin",       10 },
            { "QA",           3 },
            { "Dev",          1 },
            { "Stakeholder",  1 }
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated) { Response.Redirect("~/Login.aspx"); return; }
            if (!IsPostBack) { LoadProjects(); LoadData(); }
        }

        protected void ProjectDropDown_SelectedIndexChanged(object sender, EventArgs e)
            => LoadData();

        private void LoadProjects()
        {
            using (var ctx = new DevBoardContext())
            {
                var projects = ctx.Projects.OrderBy(p => p.Name).ToList();
                ProjectDropDown.DataSource     = projects;
                ProjectDropDown.DataTextField  = "Name";
                ProjectDropDown.DataValueField = "Id";
                ProjectDropDown.DataBind();
            }
        }

        private void LoadData()
        {
            if (string.IsNullOrEmpty(ProjectDropDown.SelectedValue)) return;
            int    projectId = int.Parse(ProjectDropDown.SelectedValue);
            string userId    = User.Identity.Name;

            using (var ctx = new DevBoardContext())
            {
                var svc = new ModuleVotingService(ctx);

                // ── Step 1: flat queries — SQLite/EF6 can't do nested APPLY joins ──
                var modules = ctx.Modules
                    .Where(m => m.ProjectId == projectId)
                    .Include(m => m.Categories)          // one level deep only
                    .OrderByDescending(m => m.IsCritical)
                    .ThenBy(m => m.Name)
                    .ToList();

                if (!modules.Any())
                {
                    ModulesRepeater.Visible = false;
                    EmptyPanel.Visible      = true;
                }
                else
                {
                    ModulesRepeater.Visible = true;
                    EmptyPanel.Visible      = false;

                    var catIds = modules.SelectMany(m => m.Categories.Select(c => c.Id)).ToList();
                    var moduleIds = modules.Select(m => m.Id).ToList();

                    // Load votes for these categories
                    var allVotes = ctx.CategoryVotes
                        .Where(v => catIds.Contains(v.CategoryId))
                        .ToList();

                    // Load ALL tickets for these modules (covers tickets with or without CategoryId)
                    var allTickets = ctx.Tickets
                        .Where(t => t.ModuleId.HasValue && moduleIds.Contains(t.ModuleId.Value))
                        .ToList();

                    // Load ticket upvote counts for all tickets in these modules
                    // (Gravity Well: high ticket boost volume weighs down category health)
                    var ticketIds = allTickets.Select(t => t.Id).ToList();
                    var allTicketVotes = ctx.TicketVotes
                        .Where(tv => ticketIds.Contains(tv.TicketId) && tv.Value == 1)
                        .ToList();

                    // Build weight lookup (call Roles API once per unique user)
                    var userWeightCache = new Dictionary<string, int>();
                    int GetWeight(string uid)
                    {
                        if (userWeightCache.TryGetValue(uid, out int w)) return w;
                        int weight = 1;
                        var roles = System.Web.Security.Roles.GetRolesForUser(uid);
                        foreach (var role in roles)
                            if (VoteWeights.TryGetValue(role, out int rw) && rw > weight)
                                weight = rw;
                        userWeightCache[uid] = weight;
                        return weight;
                    }

                    var moduleVMs = new List<ModuleViewModel>();

                    foreach (var m in modules)
                    {
                        var catVMs = new List<CategoryViewModel>();

                        foreach (var c in m.Categories.OrderByDescending(x => x.SeverityMultiplier))
                        {
                            var catVotes   = allVotes.Where(v => v.CategoryId == c.Id).ToList();
                            // Tickets explicitly in this category, OR (no category set but in this module)
                            var catTickets = allTickets
                                .Where(t => t.CategoryId == c.Id ||
                                            (!t.CategoryId.HasValue && t.ModuleId == m.Id))
                                .ToList();

                            // Invert vote direction: Downvote (-1) means "Unstable" (increases stress)
                            // Upvote (+1) means "Stable" (decreases stress)
                            int weightedVotes = catVotes.Sum(v => v.Value * -1 * GetWeight(v.UserId));
                            int openTickets   = catTickets.Count(t => t.Status != Status.Done);

                            // ── Gravity Well formula (per votingsys.md §2) ────────────────────
                            // Sc = (CategoryVotes × Wu) + (ΣTicketBoosts × 0.2)
                            // 5 boosts on a ticket ≈ 1 category-level flag.
                            // Keeps ticket workload pressure visible on module health
                            // without conflating two distinct signals.
                            var openTicketIds  = catTickets.Where(t => t.Status != Status.Done).Select(t => t.Id).ToHashSet();
                            int ticketBoosts   = allTicketVotes.Count(tv => openTicketIds.Contains(tv.TicketId));
                            decimal ticketPenalty = ticketBoosts * 0.2m;

                            // Sc = weighted category votes + ticket boost penalty, clamped ≥ 0
                            decimal scRaw = (decimal)weightedVotes + ticketPenalty;
                            decimal sc    = Math.Max(0m, scRaw);

                            int userVote = catVotes.FirstOrDefault(v => v.UserId == userId)?.Value ?? 0;

                            // Bar width: Sc in [0, ∞), treat Sc=1.5 as 100% bar
                            double barWidth = sc > 0m ? Math.Min((double)(sc / 1.5m) * 100.0, 100.0) : 0.0;

                            catVMs.Add(new CategoryViewModel
                            {
                                Id                 = c.Id,
                                Name               = c.Name,
                                SeverityMultiplier = c.SeverityMultiplier,
                                WeightedVotes      = weightedVotes,
                                OpenTickets        = openTickets,
                                TicketBoostPenalty = ticketPenalty,
                                StressScore        = sc,
                                IsHighRisk         = sc > 0.5m,
                                BarWidth           = Math.Round(barWidth, 1),
                                BarColor           = StressBarColor(sc),
                                StressClass        = StressClass(sc),
                                UserVote           = userVote
                            });
                        }

                        // Hm = 100 - avg(Sc) × 100, clamped to [0, 100]
                        // Sc = (WeightedCategoryVotes + TicketBoostPenalty), so 1 unit of Sc = 100% stress.
                        // avg(Sc) across categories drives the module health bar.
                        double healthPct = 100.0;
                        if (catVMs.Any())
                            healthPct = Math.Min(100.0, Math.Max(0.0,
                                100.0 - (double)catVMs.Average(c => c.StressScore) * 100.0));

                        int healthPctInt = (int)Math.Round(healthPct);
                        string tab       = (healthPctInt >= 95) ? "low" : "top";

                        moduleVMs.Add(new ModuleViewModel
                        {
                            Id             = m.Id,
                            Name           = m.Name,
                            IsCritical     = m.IsCritical,
                            HealthPct      = healthPctInt,
                            HealthClass    = HealthClass(healthPctInt),
                            HealthBarColor = HealthBarColor(healthPctInt),
                            IconClass      = m.IsCritical ? "critical" : "normal",
                            TagHtml        = BuildTagHtml(m.IsCritical, healthPctInt),
                            Tab            = tab,
                            Categories     = catVMs
                        });
                    }

                    // Sort by health ascending (most unstable at the top), then critical modules first
                    moduleVMs = moduleVMs.OrderBy(m => m.HealthPct).ThenByDescending(m => m.IsCritical).ToList();

                    ModulesRepeater.DataSource = moduleVMs;
                    ModulesRepeater.DataBind();
                }

                // Recent-vote feed
                var feed = svc.GetRecentVotes(projectId, 12);
                FeedRepeater.DataSource = feed.Any() ? (object)feed : null;
                FeedRepeater.DataBind();
                EmptyFeedPanel.Visible = !feed.Any();
            }
        }

        protected void ModulesRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item &&
                e.Item.ItemType != ListItemType.AlternatingItem) return;

            var vm    = e.Item.DataItem as ModuleViewModel;
            var cats  = e.Item.FindControl("CategoriesRepeater") as Repeater;
            var noLbl = e.Item.FindControl("NoCatsLabel")         as Label;

            if (vm?.Categories?.Any() == true)
            {
                cats.DataSource = vm.Categories;
                cats.DataBind();
                if (noLbl != null) noLbl.Visible = false;
            }
            else
            {
                cats.DataSource = null;
                cats.DataBind();
                if (noLbl != null) { noLbl.Text = "No categories defined for this module."; noLbl.Visible = true; }
            }
        }

        // ── Static helper methods (called from code-behind, results stored in ViewModel) ──

        private static string HealthClass(int pct)
        {
            if (pct >= 90) return "health-green";
            if (pct >= 70) return "health-yellow";
            return "health-red";
        }

        private static string HealthBarColor(int pct)
        {
            if (pct >= 90) return "#16a34a";
            if (pct >= 70) return "#f59e0b";
            return "#dc2626";
        }

        private static string BuildTagHtml(bool isCritical, int healthPct)
        {
            var tags = new List<string>();
            if (isCritical)   tags.Add("<span class='tag-critical'>Critical Module</span>");
            if (healthPct < 70 && !isCritical) tags.Add("<span class='tag-underperform'>Underperforming</span>");
            else if (healthPct < 90 && healthPct >= 70) tags.Add("<span class='tag-moderate'>Monitor</span>");
            return string.Join(" ", tags);
        }

        private static string StressClass(decimal sc)
        {
            if (sc > 0.5m)  return "stress-critical";
            if (sc > 0.25m) return "stress-high";
            if (sc > 0m)    return "stress-low";
            return "stress-ok";
        }

        private static string StressBarColor(decimal sc)
        {
            if (sc > 0.5m)  return "#dc2626";
            if (sc > 0.25m) return "#f59e0b";
            if (sc > 0m)    return "#16a34a";
            return "#9ca3af";
        }
    }

    // ── View Models ──────────────────────────────────────────────────────────

    public class ModuleViewModel
    {
        public int     Id             { get; set; }
        public string  Name           { get; set; }
        public bool    IsCritical     { get; set; }
        public int     HealthPct      { get; set; }
        public string  HealthClass    { get; set; }
        public string  HealthBarColor { get; set; }
        public string  IconClass      { get; set; }
        public string  TagHtml        { get; set; }
        public string  Tab            { get; set; }   // "top" or "low"
        public IList<CategoryViewModel> Categories { get; set; }
    }

    public class CategoryViewModel
    {
        public int     Id                 { get; set; }
        public string  Name               { get; set; }
        public decimal SeverityMultiplier { get; set; }
        public int     WeightedVotes      { get; set; }
        public int     OpenTickets        { get; set; }
        /// <summary>ΣTicketBoosts × 0.2 — the Gravity Well contribution from ticket upvotes.</summary>
        public decimal TicketBoostPenalty { get; set; }
        public decimal StressScore        { get; set; }  // Sc = WeightedVotes + TicketBoostPenalty
        public bool    IsHighRisk         { get; set; }  // Sc > 0.5
        public double  BarWidth           { get; set; }
        public string  BarColor           { get; set; }
        public string  StressClass        { get; set; }
        public int     UserVote           { get; set; }
    }
}
