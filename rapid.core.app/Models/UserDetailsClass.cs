using System.ComponentModel.DataAnnotations;

namespace rapid.core.app.Models
{
    public class UserDetailsClass
    {
        [Key]
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Position { get; set; }
        public string Unit { get; set; }
    }
}
