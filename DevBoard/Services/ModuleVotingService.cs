using DevBoard.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DevBoard.Services
{
    /// <summary>
    /// Category score  = (upvotes - downvotes) * SeverityMultiplier
    /// Module score    = sum of all category scores (rounded to int)
    /// A higher score means MORE community pain signal on that module.
    /// </summary>
    public class ModuleVotingService
    {
        private readonly DevBoardContext _ctx;

        public ModuleVotingService(DevBoardContext ctx)
        {
            _ctx = ctx;
        }

        // ── Category-level vote logic ─────────────────────────────────────

        public int GetCategoryUpvotes(int categoryId)
            => _ctx.CategoryVotes.Count(v => v.CategoryId == categoryId && v.Value == 1);

        public int GetCategoryDownvotes(int categoryId)
            => _ctx.CategoryVotes.Count(v => v.CategoryId == categoryId && v.Value == -1);

        public int GetCategoryNetVotes(int categoryId)
            => _ctx.CategoryVotes.Where(v => v.CategoryId == categoryId).Sum(v => (int?)v.Value) ?? 0;

        /// <summary>Weighted signal: net votes × severity multiplier</summary>
        public decimal GetCategoryScore(int categoryId, decimal severityMultiplier)
            => GetCategoryNetVotes(categoryId) * severityMultiplier;

        public int GetUserCategoryVote(int categoryId, string userId)
        {
            var v = _ctx.CategoryVotes.FirstOrDefault(x => x.CategoryId == categoryId && x.UserId == userId);
            return v?.Value ?? 0;
        }

        /// <summary>
        /// Toggle/switch category vote. Same value again = remove (un-vote).
        /// Returns the new net votes and the new score for the category.
        /// </summary>
        public (int netVotes, decimal score) Vote(int categoryId, string userId, int value, decimal severityMultiplier)
        {
            if (value != 1 && value != -1) throw new ArgumentException("Vote value must be +1 or -1");

            var existing = _ctx.CategoryVotes.FirstOrDefault(v => v.CategoryId == categoryId && v.UserId == userId);

            if (existing != null && existing.Value == value)
                _ctx.CategoryVotes.Remove(existing);         // un-vote
            else if (existing != null)
                existing.Value = value;                      // flip
            else
                _ctx.CategoryVotes.Add(new CategoryVote { CategoryId = categoryId, UserId = userId, Value = value, CreatedAt = DateTime.UtcNow });

            _ctx.SaveChanges();

            int net = GetCategoryNetVotes(categoryId);
            return (net, net * severityMultiplier);
        }

        // ── Module-level aggregation ──────────────────────────────────────

        /// <summary>Sum of all category weighted scores for this module.</summary>
        public decimal GetModuleScore(int moduleId)
        {
            var cats = _ctx.Categories.Where(c => c.ModuleId == moduleId).ToList();
            decimal total = 0;
            foreach (var c in cats)
                total += GetCategoryScore(c.Id, c.SeverityMultiplier);
            return total;
        }

        // ── Recent activity feed ──────────────────────────────────────────

        public class VoteFeedItem
        {
            public string UserId { get; set; }
            public int Value { get; set; }
            public string CategoryName { get; set; }
            public string ModuleName { get; set; }
            public DateTime CreatedAt { get; set; }

            public string UserDisplay => UserId.Split('@')[0];
            public string Action => Value == 1 ? "upvoted" : "downvoted";
            public string TimeAgo
            {
                get
                {
                    var diff = DateTime.UtcNow - CreatedAt;
                    if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
                    if (diff.TotalHours < 24)   return $"{(int)diff.TotalHours}h ago";
                    return $"{(int)diff.TotalDays}d ago";
                }
            }
        }

        public List<VoteFeedItem> GetRecentVotes(int projectId, int count = 10)
        {
            // SQLite/EF6 can't do APPLY joins, so resolve category IDs first
            var catMap = _ctx.Categories
                .Where(c => c.Module.ProjectId == projectId)
                .Select(c => new { c.Id, c.Name, ModuleName = c.Module.Name })
                .ToList()
                .ToDictionary(c => c.Id, c => new { c.Name, c.ModuleName });

            if (!catMap.Any()) return new List<VoteFeedItem>();

            var catIds = catMap.Keys.ToList();

            var votes = _ctx.CategoryVotes
                .Where(v => catIds.Contains(v.CategoryId))
                .OrderByDescending(v => v.CreatedAt)
                .Take(count)
                .ToList();

            return votes.Select(v =>
            {
                var cat = catMap.ContainsKey(v.CategoryId) ? catMap[v.CategoryId] : null;
                return new VoteFeedItem
                {
                    UserId       = v.UserId,
                    Value        = v.Value,
                    CategoryName = cat?.Name      ?? "Unknown",
                    ModuleName   = cat?.ModuleName ?? "Unknown",
                    CreatedAt    = v.CreatedAt
                };
            }).ToList();
        }
    }
}
