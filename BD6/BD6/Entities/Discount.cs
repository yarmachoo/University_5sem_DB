using System.ComponentModel.DataAnnotations;

namespace BD6.Entities
{
    public class Discount
    {
        public int DiscountId { get; set; }

        public string Code { get; set; }

        public decimal? Percents { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool? IsActive { get; set; }
    }
}
