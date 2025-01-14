using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BD6.Entities
{
    public class Feedback
    {
        public int FeedbackId { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public int BookId { get; set; }

        public Book Book { get; set; }

        public decimal? Assesment { get; set; }
    }
}
