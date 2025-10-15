using LeaveManagement.Models;

namespace LeaveManagement.Managers
{
    public interface IUserManager
    {
        Task RegisterUser(RegisterRequest user);
        Task<LoginResponse> LoginUser(string email, string password);
        Task<IEnumerable<UserDto>> GetAllUsers(int? managerId = null);
        Task<UserDto> GetUserById(int userId);
        Task UpdateUserProfile(int userId, UpdateUserRequest request);
        Task ChangePassword(int userId, ChangePasswordRequest request);
        Task DeleteUser(int userId);
        Task<bool> AssignManagerAsync(int userId, int managerId);
        Task<bool> PromoteToManagerAsync(int userId);
        Task<IEnumerable<UserDto>> GetManagersByDepartmentAsync(string department);
    }
}
