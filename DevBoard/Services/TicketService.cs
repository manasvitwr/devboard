using DevBoard.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DevBoard.Services
{
    public class TicketService
    {
        private readonly DevBoardContext _context;

        public TicketService(DevBoardContext context)
        {
            _context = context;
        }

        public List<Ticket> GetAllTickets()
        {
            return _context.Tickets
                .Include(t => t.Project)
                .Include(t => t.Module)
                .Include(t => t.Votes)
                .ToList();
        }

        public List<Ticket> GetTicketsByProject(int projectId)
        {
            return _context.Tickets
                .Include(t => t.Project)
                .Include(t => t.Module)
                .Include(t => t.Votes)
                .Where(t => t.ProjectId == projectId)
                .ToList();
        }

        public Ticket GetTicketById(int id)
        {
            return _context.Tickets
                .Include(t => t.Project)
                .Include(t => t.Module)
                .Include(t => t.Votes)
                .FirstOrDefault(t => t.Id == id);
        }

        public void CreateTicket(Ticket ticket)
        {
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.UpdatedAt = DateTime.UtcNow;
            _context.Tickets.Add(ticket);
            _context.SaveChanges();
        }

        public void UpdateTicket(Ticket ticket)
        {
            ticket.UpdatedAt = DateTime.UtcNow;
            _context.Entry(ticket).State = EntityState.Modified;
            _context.SaveChanges();
        }

        public void DeleteTicket(int id)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                _context.SaveChanges();
            }
        }

        public void UpdateTicketStatus(int id, Status status)
        {
            var ticket = _context.Tickets.Find(id);
            if (ticket != null)
            {
                ticket.Status = status;
                ticket.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
            }
        }

        public void Vote(int ticketId, string userId, int value)
        {
            // Ensure value is +1 or -1
            if (value != 1 && value != -1)
                throw new ArgumentException("Vote value must be +1 or -1");

            var existingVote = _context.TicketVotes
                .FirstOrDefault(v => v.TicketId == ticketId && v.UserId == userId);

            if (existingVote != null && existingVote.Value == value)
            {
                // Clicking the same vote again — toggle it off (un-vote)
                _context.TicketVotes.Remove(existingVote);
            }
            else if (existingVote != null)
            {
                // Switching direction — update in place
                existingVote.Value = value;
            }
            else
            {
                // Fresh vote
                _context.TicketVotes.Add(new TicketVote
                {
                    TicketId = ticketId,
                    UserId = userId,
                    Value = value
                });
            }

            _context.SaveChanges();
        }

        public int GetTicketScore(int ticketId)
        {
            return _context.TicketVotes
                .Where(v => v.TicketId == ticketId)
                .Sum(v => (int?)v.Value) ?? 0;
        }

        public int GetUserVote(int ticketId, string userId)
        {
            var vote = _context.TicketVotes
                .FirstOrDefault(v => v.TicketId == ticketId && v.UserId == userId);
            return vote?.Value ?? 0;
        }

        public Dictionary<int, int> GetModulePainScores(int projectId)
        {
            var modules = _context.Modules
                .Where(m => m.ProjectId == projectId)
                .Include(m => m.Tickets)
                .Include(m => m.Tickets.Select(t => t.Votes))
                .ToList();

            var painScores = new Dictionary<int, int>();

            foreach (var module in modules)
            {
                var openQADebtCount = module.Tickets.Count(t =>
                    (t.Type == TicketType.QADebt || t.Type == TicketType.Bug) &&
                    t.Status != Status.Done);

                var flakyCount = module.Tickets.Count(t => t.Flaky);

                var upvotesOnQADebt = module.Tickets
                    .Where(t => t.Type == TicketType.QADebt || t.Type == TicketType.Bug)
                    .SelectMany(t => t.Votes)
                    .Where(v => v.Value > 0)
                    .Sum(v => v.Value);

                // Pain Score = (OpenQADebt * 2) + UpvotesOnQADebt + (Flaky * 3)
                var painScore = (openQADebtCount * 2) + upvotesOnQADebt + (flakyCount * 3);
                painScores[module.Id] = painScore;
            }

            return painScores;
        }
    }
}
