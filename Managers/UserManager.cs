using LeaveManagement.Models;
using LeaveManagement.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LeaveManagement.Managers
{
    public class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;
        private readonly ILogger<UserManager> _logger;

        public UserManager(IUserRepository userRepo, IConfiguration config, ILogger<UserManager> logger)
        {
            _userRepo = userRepo;
            _config = config;
            _logger = logger;
        }

        public async Task RegisterUser(RegisterRequest user)
        {
            try
            {
                await _userRepo.RegisterUser(user);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Email already exists"))
            {
                throw; // Allow controller to return 400
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while registering user {Email}", user.Email);
                throw new Exception($"Error occurred while registering the user: {ex.Message}", ex);
            }
        }

        public async Task<LoginResponse> LoginUser(string email, string password)
        {
            try
            {
                var user = await _userRepo.LoginUser(email, password);
                if (user == null)
                    throw new UnauthorizedAccessException("Invalid email or password");

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var token = new JwtSecurityToken(
                    issuer: _config["Jwt:Issuer"],
                    audience: _config["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:ExpiresInMinutes"])),
                    signingCredentials: creds
                );

                return new LoginResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    UserId = user.UserId,
                    Role = user.Role
                };
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Failed login attempt for {Email}", email);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", email);
                throw new Exception($"Error occurred while logging in: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUsers(int? managerId = null)
        {
            try
            {
                var users = await _userRepo.GetAllUsers(managerId);

                return users.Select(u => new UserDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Role = u.Role,
                    Department = u.Department,
                    ManagerId = u.ManagerId,
                    ManagerName = u.ManagerName,
                    DateJoined = u.DateJoined
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching users");
                throw new Exception($"Error occurred while fetching users: {ex.Message}", ex);
            }
        }

        public async Task<UserDto> GetUserById(int userId)
        {
            try
            {
                var user = await _userRepo.GetUserById(userId);
                if (user == null)
                    return null;

                return new UserDto
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Role = user.Role,
                    Department = user.Department,
                    ManagerId = user.ManagerId,
                    ManagerName = user.ManagerName,
                    DateJoined = user.DateJoined
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user with ID {UserId}", userId);
                throw new Exception($"Error occurred while fetching the user: {ex.Message}", ex);
            }
        }

        public async Task UpdateUserProfile(int userId, UpdateUserRequest request)
        {
            try
            {
                if (request.FullName == null && request.Email == null && request.Phone == null)
                    throw new ArgumentException("At least one field must be provided to update.");

                await _userRepo.UpdateUserProfile(userId, request.FullName, request.Email, request.Phone);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Email already exists"))
            {
                throw; // Allow controller to handle duplicate email case
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while updating user profile {UserId}", userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile {UserId}", userId);
                throw new Exception($"Error occurred while updating the user profile: {ex.Message}", ex);
            }
        }

        public async Task ChangePassword(int userId, ChangePasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.OldPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
                    throw new ArgumentException("Old password and new password must be provided.");

                await _userRepo.ChangePassword(userId, request.OldPassword, request.NewPassword);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Old password is incorrect"))
            {
                throw; // Let controller send proper response
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error while changing password for {UserId}", userId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                throw new Exception($"Error occurred while changing the password: {ex.Message}", ex);
            }
        }

        public async Task DeleteUser(int userId)
        {
            try
            {
                await _userRepo.DeleteUser(userId);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("User not found"))
            {
                throw; // Let controller return 404
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {UserId}", userId);
                throw new Exception($"Error occurred while deleting the user: {ex.Message}", ex);
            }
        }

        public async Task<bool> AssignManagerAsync(int userId, int managerId)
        {
            return await _userRepo.AssignManagerAsync(userId, managerId);
        }

        public async Task<bool> PromoteToManagerAsync(int userId)
        {
            try
            {
                return await _userRepo.PromoteToManagerAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error promoting user with ID {userId} to Manager");
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetManagersByDepartmentAsync(string department)
        {
            try
            {
                var users = await _userRepo.GetManagersByDepartmentAsync(department);

                return users.Select(u => new UserDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Department = u.Department
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while fetching managers for department: {department}");
                throw new Exception($"Error fetching managers for department {department}: {ex.Message}", ex);
            }
        }
    }
}
