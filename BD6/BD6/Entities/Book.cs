using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BD6.Entities
{
    public class Book
    {
        public int BookId { get; set; }
        public string Name { get; set; }
        public int? PublishingId { get; set; }
        public Publishing Publishing { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal? Assesment { get; set; }

        // Новые свойства
        public List<string> Authors { get; set; }
        public List<string> Categories { get; set; }
    }

}
