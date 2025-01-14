using System.ComponentModel.DataAnnotations.Schema;

namespace BD6.Entities
{
    public class CategoryBook
    {
        public int CategoryBookId { get; set; }

        public int BookId { get; set; }

        public Book Book { get; set; }

        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
