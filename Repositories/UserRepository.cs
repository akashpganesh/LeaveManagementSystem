using Dapper;
using LeaveManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace LeaveManagement.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
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
                if (ex.Message.Contains("Email already exists", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Email already exists", ex);

                throw new Exception($"Database error while registering user: {ex.Message}", ex);
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
                throw new Exception($"Database error while logging in: {ex.Message}", ex);
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
                throw new Exception($"Database error while fetching users: {ex.Message}", ex);
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
                throw new Exception($"Database error while fetching user with ID {userId}: {ex.Message}", ex);
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
                if (ex.Message.Contains("Email already exists", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Email already exists", ex);

                throw new Exception($"Database error while updating user: {ex.Message}", ex);
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
                if (ex.Message.Contains("Old password is incorrect", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Old password is incorrect", ex);

                throw new Exception($"Database error while changing password: {ex.Message}", ex);
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
                if (ex.Message.Contains("User not found", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("User not found", ex);

                throw new Exception($"Database error while deleting user: {ex.Message}", ex);
            }
        }

        public async Task<bool> AssignManagerAsync(int userId, int managerId)
        {
            using (IDbConnection db = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UserId", userId);
                parameters.Add("@ManagerId", managerId);

                try
                {
                    await db.ExecuteAsync("sp_AssignManager", parameters, commandType: CommandType.StoredProcedure);
                    return true;
                }
                catch (SqlException ex)
                {
                    // Handle SQL errors (like user not found)
                    Console.WriteLine($"SQL Error: {ex.Message}");
                    return false;
                }
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
                throw new Exception($"Database error while promoting user: {ex.Message}", ex);
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
                throw new Exception($"Database error while fetching managers: {ex.Message}", ex);
            }
        }
    }
}
