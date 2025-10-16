using Dapper;
using LeaveManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LeaveManagement.Repositories
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<LeaveRequestRepository> _logger;

        public LeaveRequestRepository(IConfiguration configuration, ILogger<LeaveRequestRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task<int> CreateLeaveRequest(LeaveRequest leave)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeId", leave.EmployeeId);
                parameters.Add("@StartDate", leave.StartDate);
                parameters.Add("@EndDate", leave.EndDate);
                parameters.Add("@LeaveType", leave.LeaveType);
                parameters.Add("@Remarks", leave.Remarks);

                return await db.ExecuteScalarAsync<int>(
                    "sp_CreateLeaveRequest",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while creating leave request for EmployeeId: {EmployeeId}", leave.EmployeeId);
                throw new Exception($"Database error while creating leave request: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetAllLeaveRequestsAsync(int? managerId = null, int? months = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@ManagerId", managerId);
                parameters.Add("@Months", months);

                return await connection.QueryAsync<LeaveRequestDto>(
                    "sp_GetAllLeaveRequests",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while fetching all leave requests");
                throw new Exception($"Database error while fetching leave requests: {ex.Message}", ex);
            }
        }

        public async Task<LeaveStatusCountDto> GetLeaveCountsAsync(int? managerId = null, int? months = null)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@ManagerId", managerId);
                parameters.Add("@Months", months);

                var counts = await connection.QueryFirstOrDefaultAsync<LeaveStatusCountDto>(
                    "sp_GetLeaveCounts",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return counts ?? new LeaveStatusCountDto();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while fetching leave counts");
                throw new Exception($"Database error while fetching leave counts: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByManagerIdAsync(int managerId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@ManagerId", managerId);

                return await connection.QueryAsync<LeaveRequestDto>(
                    "sp_GetLeaveRequestsByManagerId",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while fetching leave requests for ManagerId: {ManagerId}", managerId);
                throw new Exception($"Database error while fetching leave requests: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<LeaveRequestDto>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@EmployeeId", employeeId);

                return await connection.QueryAsync<LeaveRequestDto>(
                    "sp_GetLeaveRequestsByEmployeeId",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while fetching leave requests for EmployeeId: {EmployeeId}", employeeId);
                throw new Exception($"Database error while fetching leave requests: {ex.Message}", ex);
            }
        }

        public async Task<LeaveRequestDto> GetLeaveRequestByIdAsync(int leaveId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@LeaveId", leaveId);

                return await connection.QueryFirstOrDefaultAsync<LeaveRequestDto>(
                    "sp_GetLeaveRequestById",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while fetching leave request with LeaveId: {LeaveId}", leaveId);
                throw new Exception($"Database error while fetching leave request: {ex.Message}", ex);
            }
        }

        public async Task<int> UpdateLeaveStatusAsync(int leaveId, string status)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@LeaveId", leaveId);
                parameters.Add("@Status", status);

                return await connection.ExecuteAsync(
                    "sp_UpdateLeaveStatus",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while updating leave status for LeaveId: {LeaveId}", leaveId);
                throw new Exception($"Database error while updating leave status: {ex.Message}", ex);
            }
        }

        public async Task<LeaveStatusCountDto> GetLeaveStatsByEmployeeIdAsync(int employeeId)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                return await connection.QueryFirstOrDefaultAsync<LeaveStatusCountDto>(
                    "sp_GetLeaveStatsByEmployeeId",
                    new { EmployeeId = employeeId },
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error while fetching leave stats for EmployeeId: {EmployeeId}", employeeId);
                throw new Exception($"Database error while fetching leave stats: {ex.Message}", ex);
            }
        }
    }
}