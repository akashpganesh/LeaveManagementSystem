using LeaveManagement.Models;
using LeaveManagement.Repositories;

namespace LeaveManagement.Managers
{
    public class LeaveRequestManager : ILeaveRequestManager
    {
        private readonly ILeaveRequestRepository _repo;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<LeaveRequestManager> _logger;

        public LeaveRequestManager(ILeaveRequestRepository repo, IUserRepository userRepo, ILogger<LeaveRequestManager> logger)
        {
            _repo = repo;
            _userRepo = userRepo;
            _logger = logger;
        }

        public async Task<int> RequestLeave(int employeeId, CreateLeaveRequestDto dto)
        {
            try
            {
                if (dto.StartDate > dto.EndDate)
                    throw new ArgumentException("StartDate cannot be after EndDate.");

                if (dto.LeaveTypeId <= 0)
                    throw new ArgumentException("LeaveType must be provided.");

                var leave = new LeaveRequest
                {
                    EmployeeId = employeeId,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
                    LeaveTypeId = dto.LeaveTypeId,
                    Remarks = dto.Remarks
                };

                return await _repo.CreateLeaveRequest(leave);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while requesting leave for EmployeeId {EmployeeId}", employeeId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while requesting leave for EmployeeId {EmployeeId}", employeeId);
                throw;
            }
        }

        public async Task<(IEnumerable<LeaveRequestDto> LeaveRequests, LeaveStatusCountDto Counts)> GetAllLeaveRequestsAsync(int? managerId = null, int? months = null)
        {
            try
            {
                var leaveRequestsTask = _repo.GetAllLeaveRequestsAsync(managerId, months);
                var leaveCountsTask = _repo.GetLeaveCountsAsync(managerId, months);

                await Task.WhenAll(leaveRequestsTask, leaveCountsTask);

                return (leaveRequestsTask.Result, leaveCountsTask.Result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching combined leave requests and counts for ManagerId {ManagerId}", managerId);
                throw new Exception("An error occurred while fetching leave requests.", ex);
            }
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
        {
            try
            {
                return await _repo.GetLeaveRequestsByEmployeeIdAsync(employeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching leave requests for EmployeeId {EmployeeId}", employeeId);
                throw new Exception("An error occurred while fetching leave requests for the employee.", ex);
            }
        }

        public async Task<LeaveRequestDto> GetLeaveRequestByIdAsync(int leaveId)
        {
            try
            {
                var leave = await _repo.GetLeaveRequestByIdAsync(leaveId);
                if (leave == null)
                {
                    _logger.LogWarning("Leave request not found with LeaveId {LeaveId}", leaveId);
                    return null;
                }
                return leave;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching leave request with LeaveId {LeaveId}", leaveId);
                throw new Exception("An error occurred while fetching leave request.", ex);
            }
        }

        public async Task<bool> UpdateLeaveStatusAsync(int leaveId, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new ArgumentException("Status must be provided.");

                var rowsAffected = await _repo.UpdateLeaveStatusAsync(leaveId, status);
                return rowsAffected > 0;
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating leave status for LeaveId {LeaveId}", leaveId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating leave status for LeaveId {LeaveId}", leaveId);
                throw new Exception("An error occurred while updating leave status.", ex);
            }
        }

        public async Task<DashboardDto> GetDashboardAsync(int employeeId)
        {
            try
            {
                var employeeTask = _userRepo.GetUserById(employeeId);
                var leaveStatsTask = _repo.GetLeaveStatsByEmployeeIdAsync(employeeId);

                await Task.WhenAll(employeeTask, leaveStatsTask);

                var employee = employeeTask.Result;
                var stats = leaveStatsTask.Result;

                if (employee == null)
                {
                    _logger.LogWarning("Employee not found with EmployeeId {EmployeeId}", employeeId);
                    return null;
                }

                return new DashboardDto
                {
                    EmployeeId = employee.UserId,
                    EmployeeName = employee.FullName,
                    Department = employee.Department,
                    ManagerName = employee.ManagerName,
                    TotalApproved = stats?.ApprovedCount ?? 0,
                    TotalPending = stats?.PendingCount ?? 0,
                    TotalRejected = stats?.RejectedCount ?? 0,
                    TotalCancelled = stats?.CancelledCount ?? 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching dashboard for EmployeeId {EmployeeId}", employeeId);
                throw new Exception("An error occurred while fetching the dashboard.", ex);
            }
        }
        public async Task<bool> CancelLeaveRequestAsync(int leaveId, int employeeId)
        {
            try
            {
                return await _repo.CancelLeaveRequestAsync(leaveId, employeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling leave request {LeaveId} for Employee {EmployeeId}", leaveId, employeeId);
                throw;
            }
        }

    }
}