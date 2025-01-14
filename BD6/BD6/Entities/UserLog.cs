using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BD6.Entities
{
    public class UserLog
    {
        public int UserLogId { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public int ActionId { get; set; }

        public Action Action { get; set; }

        public DateTime DateTime { get; set; }
    }
}
