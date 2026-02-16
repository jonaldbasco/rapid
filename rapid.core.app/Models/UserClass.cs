using System.ComponentModel.DataAnnotations;

namespace rapid.core.app.Models
{
    public class UserClass
    {
        [Key]
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string UserName { get; set; }
        public required string UserPass { get; set; }
    }
}
