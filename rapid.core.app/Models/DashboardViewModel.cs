namespace rapid.core.app.Models
{
    public class DashboardViewModel
    {
        public List<PatientAdmission> Patients { get; set; } = new();
        public List<StaffMember> Staff { get; set; } = new();
    }
}