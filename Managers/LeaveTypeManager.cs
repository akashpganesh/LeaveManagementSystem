using LeaveManagement.Models;
using LeaveManagement.Repositories;

namespace LeaveManagement.Managers
{
    public class LeaveTypeManager : ILeaveTypeManager
    {
        private readonly ILeaveTypeRepository _repo;
        private readonly ILogger<LeaveTypeManager> _logger;

        public LeaveTypeManager(ILeaveTypeRepository repo, ILogger<LeaveTypeManager> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<bool> CreateLeaveTypeAsync(string leaveType, int maxLeavesPerYear)
        {
            try
            {
                var rows = await _repo.CreateLeaveTypeAsync(leaveType, maxLeavesPerYear);
                return rows > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating leave type");
                throw;
            }
        }

        public async Task<IEnumerable<LeaveType>> GetAllLeaveTypesAsync()
        {
            try
            {
                return await _repo.GetAllLeaveTypesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all leave types");
                throw;
            }
        }

        public async Task<LeaveType> GetLeaveTypeByIdAsync(int leaveTypeId)
        {
            try
            {
                return await _repo.GetLeaveTypeByIdAsync(leaveTypeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leave type with ID {LeaveTypeId}", leaveTypeId);
                throw;
            }
        }

        public async Task<bool> UpdateLeaveTypeAsync(int leaveTypeId, string? leaveType = null, int? maxLeavesPerYear = null)
        {
            try
            {
                var rowsAffected = await _repo.UpdateLeaveTypeAsync(leaveTypeId, leaveType, maxLeavesPerYear);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating leave type with ID {LeaveTypeId}", leaveTypeId);
                throw;
            }
        }

        public async Task<bool> DeleteLeaveTypeAsync(int leaveTypeId)
        {
            try
            {
                var rowsAffected = await _repo.DeleteLeaveTypeAsync(leaveTypeId);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting leave type with ID {LeaveTypeId}", leaveTypeId);
                throw;
            }
        }
    }
}
