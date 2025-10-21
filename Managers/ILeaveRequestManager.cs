using LeaveManagement.Models;

namespace LeaveManagement.Managers
{
    public interface ILeaveRequestManager
    {
        Task<int> RequestLeave(int employeeId, CreateLeaveRequestDto dto);
        Task<(IEnumerable<LeaveRequestDto> LeaveRequests, LeaveStatusCountDto Counts)> GetAllLeaveRequestsAsync(int? managerId = null, int? months = null);
        Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeIdAsync(int employeeId);
        Task<LeaveRequestDto> GetLeaveRequestByIdAsync(int leaveId);
        Task<bool> UpdateLeaveStatusAsync(int leaveId, string status);
        Task<DashboardDto> GetDashboardAsync(int employeeId);
        Task<bool> CancelLeaveRequestAsync(int leaveId, int employeeId);
    }
}
