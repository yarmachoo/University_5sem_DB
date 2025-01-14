using System.ComponentModel.DataAnnotations;

namespace BD6.Entities
{
    public class FullAddressOfPublish
    {
        public int FullAddressOfPublishId { get; set; }

        public string Street { get; set; }

        public int Home { get; set; }

        public int? Corpus { get; set; }
        public string City { get; set; }

        public string Country { get; set; }

        public int? ApartmentNumber { get; set; }
    }
}
