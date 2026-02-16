namespace rapid.core.app.Models
{
    public class NurseRespondViewModel
    {
        public StaffMember Nurse { get; set; } = default!;
        public SurgeRequest Request { get; set; } = default!;
        public NurseDecision CurrentDecision { get; set; } = NurseDecision.Pending;
    }
}