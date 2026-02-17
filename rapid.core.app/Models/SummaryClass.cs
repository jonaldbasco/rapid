namespace rapid.core.app.Models
{
    public class SummaryClass
    {
        public int Id { get; set; }
        public int NegotiationId { get; set; }
        public string StaffId { get; set; } = "";
        public string Summary { get; set; } = "";
    }
}
