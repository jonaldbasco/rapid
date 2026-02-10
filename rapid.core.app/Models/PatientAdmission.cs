namespace rapid.core.app.Models
{
    public class PatientAdmission
    {
        public string Id { get; set; } = default!;
        public string? PatientName { get; set; }
        public int AcuityLevel { get; set; }
        public string AcuityLabel { get; set; } = default!;      // e.g., "ESI-2"
        public string AcuityClassName { get; set; } = default!;  // e.g., "bg-surge-high text-black"
        public string? Complaint { get; set; }
        public DateTime? DateTimeAdded { get; set; }

        // Timeline
        public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? AssignedAtUtc { get; set; }
        public DateTime? DischargedAtUtc { get; set; }
        public enum PatientStatus
        {
            Waiting = 0,
            InRoom = 1,
            Discharged = 2
        }

        // Flow status
        public PatientStatus Status { get; set; } = PatientStatus.Waiting;


    }
}
