using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BD6.Entities
{
    public class Publishing
    {
        public int PublishingId { get; set; }
        public string Name { get; set; }

        public DateTime? DateOfPublish { get; set; }

        public int? PlaceOfPublishId { get; set; }

        public FullAddressOfPublish PlaceOfPublish { get; set; }
    }
}
