namespace rapid.core.app.Models
{
    public class DashboardViewModel
    {
        public List<PatientAdmission> Patients { get; set; } = new();
        public List<StaffMember> Staffs { get; set; } = new();
        public List<StaffClass> Staff { get; set; } = new();
    }
}