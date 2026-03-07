using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DevBoard.Models
{
    public class Module
    {
        public Module()
        {
            Tickets = new List<Ticket>();
            Categories = new List<Category>();
        }

        public int Id { get; set; }

        public int ProjectId { get; set; }

        [StringLength(255)]
        public string ExtId { get; set; }

        [StringLength(1024)]
        public string Description { get; set; }

        public bool IsCritical { get; set; }


        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Path { get; set; }

        public virtual Project Project { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
    }
}
