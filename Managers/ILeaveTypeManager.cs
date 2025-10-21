using LeaveManagement.Models;

namespace LeaveManagement.Managers
{
    public interface ILeaveTypeManager
    {
        Task<bool> CreateLeaveTypeAsync(string leaveType, int maxLeavesPerYear);
        Task<IEnumerable<LeaveType>> GetAllLeaveTypesAsync();
        Task<LeaveType> GetLeaveTypeByIdAsync(int leaveTypeId);
        Task<bool> UpdateLeaveTypeAsync(int leaveTypeId, string? leaveType = null, int? maxLeavesPerYear = null);
        Task<bool> DeleteLeaveTypeAsync(int leaveTypeId);
    }
}
