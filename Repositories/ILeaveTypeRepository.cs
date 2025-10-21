using LeaveManagement.Models;

namespace LeaveManagement.Repositories
{
    public interface ILeaveTypeRepository
    {
        Task<int> CreateLeaveTypeAsync(string leaveType, int maxLeavesPerYear);
        Task<IEnumerable<LeaveType>> GetAllLeaveTypesAsync();
        Task<LeaveType> GetLeaveTypeByIdAsync(int leaveTypeId);
        Task<int> UpdateLeaveTypeAsync(int leaveTypeId, string? leaveType = null, int? maxLeavesPerYear = null);
        Task<int> DeleteLeaveTypeAsync(int leaveTypeId);
    }
}
