using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BD6.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
        public int? RoleId { get; set; }

        public RoleOfUser Role { get; set; }

        public int? UserProfileId { get; set; }

        public UserProfile UserProfile { get; set; }
    }
}
