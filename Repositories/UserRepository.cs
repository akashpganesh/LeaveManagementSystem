using Dapper;
using LeaveManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LeaveManagement.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IConfiguration config, ILogger<UserRepository> logger)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public async Task RegisterUser(RegisterRequest user)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@FullName", user.FullName);
                parameters.Add("@Email", user.Email);
                parameters.Add("@Phone", user.Phone);
                parameters.Add("@Password", user.Password);
                parameters.Add("@Role", user.Role);
                parameters.Add("@Department", user.Department);

                await db.ExecuteAsync("sp_RegisterUser", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error registering user: {Email}", user.Email);

                if (ex.Message.Contains("Email already exists", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Email already exists", ex);

                throw new Exception("Database error while registering user", ex);
            }
        }

        public async Task<User?> LoginUser(string email, string password)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@Email", email);
                parameters.Add("@Password", password);

                var user = await db.QueryFirstOrDefaultAsync<User>(
                    "sp_LoginUser",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return user;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error logging in user: {Email}", email);
                throw new Exception("Database error while logging in", ex);
            }
        }

        public async Task<IEnumerable<User>> GetAllUsers(int? managerId = null)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@ManagerId", managerId, DbType.Int32);

                return await db.QueryAsync<User>(
                    "sp_GetAllUsers",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error fetching all users for managerId: {ManagerId}", managerId);
                throw new Exception("Database error while fetching users", ex);
            }
        }

        public async Task<User?> GetUserById(int userId)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                return await db.QueryFirstOrDefaultAsync<User>(
                    "sp_GetUserById",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error fetching user by Id: {UserId}", userId);
                throw new Exception($"Database error while fetching user with ID {userId}", ex);
            }
        }

        public async Task UpdateUserProfile(int userId, string? fullName, string? email, string? phone)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@FullName", fullName);
                parameters.Add("@Email", email);
                parameters.Add("@Phone", phone);

                await db.ExecuteAsync("sp_UpdateUserProfile", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error updating user profile for UserId: {UserId}", userId);

                if (ex.Message.Contains("Email already exists", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Email already exists", ex);

                throw new Exception("Database error while updating user", ex);
            }
        }

        public async Task ChangePassword(int userId, string oldPassword, string newPassword)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@OldPassword", oldPassword);
                parameters.Add("@NewPassword", newPassword);

                await db.ExecuteAsync("sp_ChangePassword", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error changing password for UserId: {UserId}", userId);

                if (ex.Message.Contains("Old password is incorrect", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Old password is incorrect", ex);

                throw new Exception("Database error while changing password", ex);
            }
        }

        public async Task DeleteUser(int userId)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);

                await db.ExecuteAsync("sp_DeleteUser", parameters, commandType: CommandType.StoredProcedure);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error deleting user with UserId: {UserId}", userId);

                if (ex.Message.Contains("User not found", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("User not found", ex);

                throw new Exception("Database error while deleting user", ex);
            }
        }

        public async Task<bool> AssignManagerAsync(int userId, int managerId)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@ManagerId", managerId);

                await db.ExecuteAsync("sp_AssignManager", parameters, commandType: CommandType.StoredProcedure);
                return true;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error assigning manager {ManagerId} to user {UserId}", managerId, userId);
                return false;
            }
        }

        public async Task<bool> PromoteToManagerAsync(int userId)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var rowsAffected = await db.ExecuteAsync(
                    "sp_PromoteToManager",
                    new { UserId = userId },
                    commandType: CommandType.StoredProcedure
                );

                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error promoting user {UserId} to manager", userId);
                throw new Exception("Database error while promoting user", ex);
            }
        }

        public async Task<IEnumerable<User>> GetManagersByDepartmentAsync(string department)
        {
            try
            {
                using IDbConnection db = new SqlConnection(_connectionString);

                var result = await db.QueryAsync<User>(
                    "sp_GetManagersByDepartment",
                    new { Department = department },
                    commandType: CommandType.StoredProcedure
                );

                return result;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error fetching managers for department: {Department}", department);
                throw new Exception("Database error while fetching managers", ex);
            }
        }
    }
}