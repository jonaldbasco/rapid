namespace rapid.core.app.Models
{
    public class NurseDashboardViewModel
    {
        public StaffMember Nurse { get; set; } = default!;
        public List<NurseRequestItem> Requests { get; set; } = new();
    }

    public class NurseRequestItem
    {
        public SurgeRequest Request { get; set; } = default!;
        public NurseDecision Decision { get; set; } = NurseDecision.Pending;

        // convenience for UI
        public string TimeAgoLabel { get; set; } = "";
    }
}