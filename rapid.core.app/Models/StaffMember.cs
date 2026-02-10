namespace rapid.core.app.Models
{
    public class StaffMember
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Role { get; set; } = default!;        // doctor / nurse / specialist
        public string Specialty { get; set; } = default!;
        public List<string> Certifications { get; set; } = new();
        public string Status { get; set; } = "off_duty";   // off_duty / available / on_duty
        public double ResponseRate { get; set; }           // 0.88 means 88%
        public int DistanceMinutes { get; set; }           // travel time
    }
}