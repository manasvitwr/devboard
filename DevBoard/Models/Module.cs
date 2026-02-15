using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DevBoard.Models
{
    public class Module
    {
        public Module()
        {
            Tickets = new List<Ticket>();
        }

        public int Id { get; set; }

        public int ProjectId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(500)]
        public string Path { get; set; }

        public virtual Project Project { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
