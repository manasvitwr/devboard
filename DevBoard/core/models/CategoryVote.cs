using System;
using System.ComponentModel.DataAnnotations;

namespace DevBoard.Core.Models
{
    public class CategoryVote
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }

        [Required]
        [StringLength(256)]
        public string UserId { get; set; }

        public int Value { get; set; } // +1 or -1

        public DateTime CreatedAt { get; set; }

        public virtual Category Category { get; set; }
    }
}
