using System.ComponentModel.DataAnnotations.Schema;

namespace rapid.core.app.Models
{
    public class StaffMember
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public required string Role { get; set; }
        public required string Unit { get; set; }
        public required string Certification { get; set; }
        public required string Status { get; set; }
        public required string Decision { get; set; }
        public double ResponseRate { get; set; }
        public int DistanceMinutes { get; set; }
        public required string isAvailable { get; set; }
        //public string UpdatedAt { get; set; } = DateTime.UtcNow.ToString("o"); // "2026-02-16T12:34:56Z"

        // Store as UTC ticks or ISO string
        public long UpdatedAtTicks { get; set; } = DateTime.UtcNow.Ticks;
        public string Summary { get; set; } = "";
    }
}