using System.ComponentModel.DataAnnotations;

namespace DevBoard.Models
{
    public class TicketVote
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        [Required]
        [StringLength(256)]
        public string UserId { get; set; }

        public int Value { get; set; } // +1 or -1

        public virtual Ticket Ticket { get; set; }
    }
}
