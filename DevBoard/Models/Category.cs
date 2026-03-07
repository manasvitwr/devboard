using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DevBoard.Models
{
    public class Category
    {
        public Category()
        {
            Tickets = new List<Ticket>();
        }

        public int Id { get; set; }

        public int ModuleId { get; set; }

        [StringLength(255)]
        public string ExtId { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        public decimal SeverityMultiplier { get; set; }
        
        public decimal BaseScore { get; set; }
        
        public decimal StressScore { get; set; }

        public virtual Module Module { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
