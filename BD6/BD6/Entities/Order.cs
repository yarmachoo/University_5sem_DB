using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BD6.Entities
{
    public class Order
    {
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public int FullAddressOfOrderId { get; set; }

        public FullAddressOfOrder FullAddressOfOrder { get; set; }
        public int? DiscountId { get; set; }

        public Discount Discount { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal Cost { get; set; }
        public List<Book> Books { get; set; }
    }
}
