using Dapper;
using LeaveManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LeaveManagement.Repositories
{
    public class LeaveTypeRepository : ILeaveTypeRepository
    {
        private readonly string _connectionString;

        public LeaveTypeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> CreateLeaveTypeAsync(string leaveType, int maxLeavesPerYear)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@LeaveType", leaveType);
            parameters.Add("@MaxLeavesPerYear", maxLeavesPerYear);

            return await connection.ExecuteAsync("sp_CreateLeaveType", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<IEnumerable<LeaveType>> GetAllLeaveTypesAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var leaveTypes = await connection.QueryAsync<LeaveType>(
                "sp_GetAllLeaveTypes",
                commandType: CommandType.StoredProcedure
            );
            return leaveTypes;
        }

        public async Task<LeaveType> GetLeaveTypeByIdAsync(int leaveTypeId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@LeaveTypeId", leaveTypeId);

            return await connection.QueryFirstOrDefaultAsync<LeaveType>(
                "sp_GetLeaveTypeById",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> UpdateLeaveTypeAsync(int leaveTypeId, string? leaveType = null, int? maxLeavesPerYear = null)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@LeaveTypeId", leaveTypeId);
            parameters.Add("@LeaveType", leaveType);
            parameters.Add("@MaxLeavesPerYear", maxLeavesPerYear);

            return await connection.ExecuteAsync(
                "sp_UpdateLeaveType",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> DeleteLeaveTypeAsync(int leaveTypeId)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@LeaveTypeId", leaveTypeId);

            return await connection.ExecuteAsync(
                "sp_DeleteLeaveType",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
