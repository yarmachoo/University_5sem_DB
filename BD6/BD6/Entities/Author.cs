using System.ComponentModel.DataAnnotations;

namespace BD6.Entities
{
    public class Author
    {
        public int AuthorId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string City { get; set; }

        public string Country { get; set; }
    }
}
