using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DevBoard.Models
{
    public class Ticket
    {
        public Ticket()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Votes = new List<TicketVote>();
        }

        public int Id { get; set; }

        public int ProjectId { get; set; }

        public int? ModuleId { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        public TicketType Type { get; set; }

        public Status Status { get; set; }

        public Priority Priority { get; set; }

        [Required]
        [StringLength(256)]
        public string CreatedById { get; set; }

        [StringLength(256)]
        public string AssignedToId { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public bool MissingTests { get; set; }

        public bool Flaky { get; set; }

        public bool ManualHeavy { get; set; }

        public TestEffort EstimatedTestEffort { get; set; }

        [StringLength(1000)]
        public string AffectedPaths { get; set; }

        public virtual Project Project { get; set; }
        public virtual Module Module { get; set; }
        public virtual ICollection<TicketVote> Votes { get; set; }
    }
}
