using System.ComponentModel.DataAnnotations;

namespace rapid.core.app.Models
{
    public class PatientLocation
    {
        [Key]
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string CurrUnit { get; set; }
        public required string PrevUnit { get; set; }
    }
}
