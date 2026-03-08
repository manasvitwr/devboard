using DevBoard.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Security;

namespace DevBoard.Core.Services
{
    public class AnalyticsDashboardDto
    {
        public int ProjectHealthPct { get; set; }
        public string ProjectHealthStatus { get; set; }
        public string ProjectHealthCssClass { get; set; }

        public List<StressMapDto> TopUnstableModules { get; set; }

        public List<string> CategoryLabels { get; set; }
        public List<int> TodoTickets { get; set; }
        public List<int> InProgressTickets { get; set; }
        public List<int> DoneTickets { get; set; }

        public int TotalUpvotes { get; set; }
        public int TotalDownvotes { get; set; }
    }

    public class StressMapDto
    {
        public string ModuleName { get; set; }
        public int HealthPct { get; set; }
        public string HealthBgClass { get; set; }
        public List<FailingCategoryDto> FailingCategories { get; set; }
    }

    public class FailingCategoryDto
    {
        public string CategoryName { get; set; }
        public int Downvotes { get; set; }
        public decimal StressScore { get; set; }
        public bool IsCriticalRisk { get; set; }
    }

    public class AnalyticsService
    {
        private readonly DevBoardContext _context;

        private static readonly Dictionary<string, int> VoteWeights = new Dictionary<string, int>
        {
            { "Admin",       10 },
            { "QA",           3 },
            { "Dev",          1 },
            { "Stakeholder",  1 }
        };

        public AnalyticsService(DevBoardContext context)
        {
            _context = context;
        }

        public AnalyticsDashboardDto GetDashboardData(int projectId)
        {
            var modules = _context.Modules
                .Where(m => m.ProjectId == projectId)
                .Include(m => m.Categories)
                .ToList();

            var moduleIds = modules.Select(m => m.Id).ToList();
            var catIds = modules.SelectMany(m => m.Categories.Select(c => c.Id)).ToList();

            // Load all tickets
            var allTickets = _context.Tickets
                .Where(t => t.ProjectId == projectId)
                .ToList();

            // Load category votes
            var allVotes = _context.CategoryVotes
                .Where(v => catIds.Contains(v.CategoryId))
                .ToList();

            // Load ticket votes
            var ticketIds = allTickets.Select(t => t.Id).ToList();
            var allTicketVotes = _context.TicketVotes
                .Where(tv => ticketIds.Contains(tv.TicketId) && tv.Value == 1)
                .ToList();

            // Build weight lookup
            var userWeightCache = new Dictionary<string, int>();
            int GetWeight(string uid)
            {
                if (userWeightCache.TryGetValue(uid, out int w)) return w;
                int weight = 1;
                var roles = Roles.GetRolesForUser(uid);
                foreach (var role in roles)
                    if (VoteWeights.TryGetValue(role, out int rw) && rw > weight)
                        weight = rw;
                userWeightCache[uid] = weight;
                return weight;
            }

            var stressMapData = new List<StressMapDto>();

            double sumHealth = 0;
            int moduleCount = modules.Count;

            foreach (var m in modules)
            {
                decimal totalSc = 0;
                var failingCats = new List<FailingCategoryDto>();

                foreach (var c in m.Categories)
                {
                    var catVotes = allVotes.Where(v => v.CategoryId == c.Id).ToList();
                    var catTickets = allTickets.Where(t => t.CategoryId == c.Id || (!t.CategoryId.HasValue && t.ModuleId == m.Id)).ToList();

                    // Weighted votes (Downvote (-1) equals +1 instability signal)
                    int weightedVotes = catVotes.Sum(v => v.Value * -1 * GetWeight(v.UserId));
                    
                    var openTicketIds = catTickets.Where(t => t.Status != Status.Done).Select(t => t.Id).ToHashSet();
                    int ticketBoosts = allTicketVotes.Count(tv => openTicketIds.Contains(tv.TicketId));
                    decimal ticketPenalty = ticketBoosts * 0.2m;

                    decimal scRaw = ((decimal)weightedVotes + ticketPenalty) * c.SeverityMultiplier;
                    decimal sc = Math.Max(0m, scRaw);
                    totalSc += sc;

                    int downvotesCount = catVotes.Count(v => v.Value == -1);

                    if (downvotesCount > 0 || sc > 0)
                    {
                        failingCats.Add(new FailingCategoryDto
                        {
                            CategoryName = c.Name,
                            Downvotes = downvotesCount,
                            StressScore = sc,
                            IsCriticalRisk = sc > 0.5m
                        });
                    }
                }

                // Nuclear Health Formula: Hm = 100 * (0.95)^(\sum Sc)
                double moduleHealth = 100.0 * Math.Pow(0.95, (double)totalSc);
                sumHealth += moduleHealth;

                int healthPct = (int)Math.Round(moduleHealth);

                stressMapData.Add(new StressMapDto
                {
                    ModuleName = m.Name,
                    HealthPct = healthPct,
                    HealthBgClass = HealthBgClass(healthPct),
                    FailingCategories = failingCats.OrderByDescending(fc => fc.Downvotes).Take(3).ToList()
                });
            }

            double aggregateHealth = moduleCount > 0 ? (sumHealth / moduleCount) : 100.0;
            int aggregateHealthPct = (int)Math.Round(aggregateHealth);

            var dto = new AnalyticsDashboardDto();

            if (allTickets.Count == 0 && allVotes.Count == 0)
            {
                dto.ProjectHealthPct = 100;
                dto.ProjectHealthStatus = "Neutral / Stable (No Data)";
                dto.ProjectHealthCssClass = "health-gauge-value health-green";
            }
            else
            {
                dto.ProjectHealthPct = aggregateHealthPct;
                dto.ProjectHealthCssClass = "health-gauge-value " + HealthTextClass(aggregateHealthPct);

                if (aggregateHealthPct >= 90) dto.ProjectHealthStatus = "Stable";
                else if (aggregateHealthPct >= 70) dto.ProjectHealthStatus = "Monitor / Warning";
                else dto.ProjectHealthStatus = "Critical / Unstable";
            }

            dto.TopUnstableModules = stressMapData
                .Where(s => s.HealthPct < 100 || s.FailingCategories.Any())
                .OrderBy(s => s.HealthPct)
                .Take(3)
                .ToList();

            var ticketsByCat = allTickets
                .Where(t => t.CategoryId.HasValue)
                .GroupBy(t => t.Category.Name)
                .OrderBy(g => g.Key)
                .ToList();

            dto.CategoryLabels = ticketsByCat.Select(g => g.Key).ToList();
            dto.TodoTickets = new List<int>();
            dto.InProgressTickets = new List<int>();
            dto.DoneTickets = new List<int>();

            foreach (var g in ticketsByCat)
            {
                dto.TodoTickets.Add(g.Count(t => t.Status == Status.Todo));
                dto.InProgressTickets.Add(g.Count(t => t.Status == Status.InProgress));
                dto.DoneTickets.Add(g.Count(t => t.Status == Status.Done));
            }

            dto.TotalUpvotes = allVotes.Count(v => v.Value == 1);
            dto.TotalDownvotes = allVotes.Count(v => v.Value == -1);

            return dto;
        }

        private string HealthBgClass(int pct)
        {
            if (pct >= 90) return "bg-health-green";
            if (pct >= 70) return "bg-health-yellow";
            return "bg-health-red";
        }

        private string HealthTextClass(int pct)
        {
            if (pct >= 90) return "health-green";
            if (pct >= 70) return "health-yellow";
            return "health-red";
        }
    }
}
