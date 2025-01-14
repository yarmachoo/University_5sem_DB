using System.ComponentModel.DataAnnotations.Schema;

namespace BD6.Entities
{
    public class OrderBook
    {
        public int OrderBookId { get; set; }

        public int BookId { get; set; }

        public Book Book { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; }
    }
}
