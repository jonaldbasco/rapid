using rapid.core.app.Models;

namespace rapid.core.app.Interface
{
    public interface IStaffService
    {
        Task<List<StaffMember>> GetStaffAsync();
    }
}
