namespace rapid.core.app.Models
{
    public class NegotiationClass
    {
        public int Id { get; set; }
        public required string StaffId { get; set; } = "";
        public required string NurseName { get; set; } = "";
        public required string Unit { get; set; } = "";

        public string Status { get; set; } = "Active";
        // Active | Accepted | Declined | Escalated

        public int CurrentRound { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
