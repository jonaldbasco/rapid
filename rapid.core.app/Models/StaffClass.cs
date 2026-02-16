using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace rapid.core.app.Models
{
    public class StaffClass
    {
        [Key]
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
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}