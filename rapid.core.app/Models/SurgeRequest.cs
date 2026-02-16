namespace rapid.core.app.Models
{
    public class SurgeRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Unit { get; set; } = "";              // e.g., "Trauma Bay"
        public string Location { get; set; } = "";          // e.g., "Metro General Hospital - Trauma Bay"
        public string Description { get; set; } = "";       // message shown on cards
        public bool IsCritical { get; set; }                // adds red border
        public double PayMultiplier { get; set; } = 1.0;    // e.g., 1.5
        public int DurationHours { get; set; } = 4;         // e.g., 4 hours
        public string SpecialtyTag { get; set; } = "";      // match nurse specialty (Trauma/ICU/etc.)
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

    public enum NurseDecision
    {
        Pending,        // no response yet
        Accepted,       // accepted on nurse side
        Declined        // declined on nurse side
    }
}