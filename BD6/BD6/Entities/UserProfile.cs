using System.ComponentModel.DataAnnotations;

namespace BD6.Entities
{
    public class UserProfile
    {
        public int UserProfileId { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime DateOfRegistration { get; set; }
    }
}
