using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DevBoard.Models
{
    public class Project
    {
        public Project()
        {
            ConfigPath = "devboard.modules.json";
            Modules = new List<Module>();
            Tickets = new List<Ticket>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [StringLength(500)]
        public string RepoUrl { get; set; }

        [StringLength(200)]
        public string ConfigPath { get; set; }

        public virtual ICollection<Module> Modules { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}
