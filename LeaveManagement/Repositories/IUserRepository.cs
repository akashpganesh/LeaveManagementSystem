using LeaveManagement.Models;

namespace LeaveManagement.Repositories
{
    public interface IUserRepository
    {
        Task RegisterUser(RegisterRequest user);
        Task<User> LoginUser(string email, string password);
        Task<IEnumerable<User>> GetAllUsers(int? managerId = null);
        Task<User> GetUserById(int userId);
        Task UpdateUserProfile(int userId, string? fullName, string? email, string? phone);
        Task ChangePassword(int userId, string oldPassword, string newPassword);
        Task DeleteUser(int userId);
        Task<bool> AssignManagerAsync(int userId, int managerId);
        Task<bool> PromoteToManagerAsync(int userId);
        Task<IEnumerable<User>> GetManagersByDepartmentAsync(string department);
    }
}
