namespace rapid.core.app.Models
{
    public class NegotiationMessage
    {
        public int Id { get; set; }

        public int NegotiationId { get; set; }
        public string Sender { get; set; } = ""; // AI | Nurse | System
        public string Message { get; set; } = "";

        public int Round { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
