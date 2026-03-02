using LeaveManagement.Models;

namespace LeaveManagement.Repositories
{
    public interface ILeaveRequestRepository
    {
        Task<int> CreateLeaveRequest(LeaveRequest leave);
        Task<IEnumerable<LeaveRequestDto>> GetAllLeaveRequestsAsync(int? managerId = null, int? months = null);
        Task<LeaveStatusCountDto> GetLeaveCountsAsync(int? managerId = null, int? months = null);
        Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeIdAsync(int employeeId);
        Task<LeaveRequestDto> GetLeaveRequestByIdAsync(int leaveId);
        Task<int> UpdateLeaveStatusAsync(int leaveId, string status);
        Task<LeaveStatusCountDto> GetLeaveStatsByEmployeeIdAsync(int employeeId);
        Task<bool> CancelLeaveRequestAsync(int leaveId, int employeeId);

    }
}
