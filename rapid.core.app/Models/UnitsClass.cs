using System.ComponentModel.DataAnnotations;

namespace rapid.core.app.Models
{
    public class UnitsClass
    {
        [Key]
        public required string UnitCode { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public int CurrentStaff { get; set; }
        public int RequiredStaff { get; set; }
        public int InPatients { get; set; }
        public int Incoming {  get; set; }
        public string Status { get; set; }

    }
}
