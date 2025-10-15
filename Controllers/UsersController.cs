using LeaveManagement.Managers;
using LeaveManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using System.Security.Claims;

namespace LeaveManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManager _userManager;

        public UsersController(IUserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    await _userManager.RegisterUser(request);
                    return Ok(new { Message = "User registered successfully", CorrelationId = correlationId });
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Email already exists"))
                {
                    return Conflict(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while registering the user.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var loginResponse = await _userManager.LoginUser(request.Email, request.Password);
                    return Ok(new
                    {
                        Message = "Login successful",
                        Data = loginResponse,
                        CorrelationId = correlationId
                    });
                }
                catch (UnauthorizedAccessException)
                {
                    return Unauthorized(new { Message = "Invalid email or password", CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while logging in.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllUsers()
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    IEnumerable<UserDto> users;

                    if (User.IsInRole("Admin"))
                    {
                        users = await _userManager.GetAllUsers();
                    }
                    else if (User.IsInRole("Manager"))
                    {
                        var managerIdClaim = User.FindFirst("UserId")?.Value;
                        if (managerIdClaim == null)
                            return Unauthorized(new
                            {
                                Message = "Manager ID not found in token.",
                                CorrelationId = correlationId
                            });

                        int managerId = int.Parse(managerIdClaim);
                        users = await _userManager.GetAllUsers(managerId);
                    }
                    else
                    {
                        return StatusCode(403, new
                        {
                            Message = "You do not have permission to access this resource.",
                            CorrelationId = correlationId
                        });
                    }

                    // Group employees by Role
                    var groupedUsers = users
                        .GroupBy(u => u.Role)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    return Ok(new
                    {
                        Message = "Users retrieved successfully",
                        Data = groupedUsers,
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while fetching users.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUserById(int id)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var currentUserId = int.Parse(User.FindFirst("UserId")?.Value);
                    var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                    if (currentUserRole == "Employee" && currentUserId != id)
                        return StatusCode(403, new
                        {
                            Message = "You are not authorized to access this profile.",
                            CorrelationId = correlationId
                        });

                    var user = await _userManager.GetUserById(id);

                    if (currentUserRole == "Manager" && currentUserId != id && user.ManagerId != currentUserId)
                        return StatusCode(403, new
                        {
                            Message = "You are not authorized to access this profile.",
                            CorrelationId = correlationId
                        });

                    if (user == null)
                        return NotFound(new { Message = "User not found", CorrelationId = correlationId });

                    return Ok(new { Message = "User fetched successfully", Data = user, CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while fetching the user.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(int id, [FromBody] UpdateUserRequest request)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var currentUserId = int.Parse(User.FindFirst("UserId")?.Value);
                    var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                    if (currentUserRole != "Admin" && currentUserId != id)
                        return StatusCode(403, new
                        {
                            Message = "You are not authorized to update this profile.",
                            CorrelationId = correlationId
                        });

                    await _userManager.UpdateUserProfile(id, request);
                    return Ok(new { Message = "User profile updated successfully", CorrelationId = correlationId });
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Email already exists"))
                {
                    return Conflict(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while updating the user profile.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpPatch("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var currentUserId = int.Parse(User.FindFirst("UserId")?.Value);
                    await _userManager.ChangePassword(currentUserId, request);
                    return Ok(new { Message = "Password changed successfully", CorrelationId = correlationId });
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Old password is incorrect"))
                {
                    return Unauthorized(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while changing the password.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    await _userManager.DeleteUser(id);
                    return Ok(new { Message = "User deleted successfully", CorrelationId = correlationId });
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("User not found"))
                {
                    return NotFound(new { Message = ex.Message, CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while deleting the user.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpPost("assign-manager")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignManager([FromBody] AssignManagerRequest request)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                if (request.UserId == request.ManagerId)
                    return BadRequest(new { 
                        Message = "A user cannot be their own manager.", 
                        CorrelationId = correlationId
                    });

                var result = await _userManager.AssignManagerAsync(request.UserId, request.ManagerId);

                if (!result)
                    return BadRequest(new { 
                        Message = "Failed to assign manager. Check user and manager IDs.",
                        CorrelationId = correlationId
                    });

                return Ok(new {
                    message = "Manager assigned successfully.",
                    CorrelationId = correlationId
                });
            }
        }

        [HttpPost("promote-to-manager")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PromoteToManager([FromBody] PromoteToManagerRequest request)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var success = await _userManager.PromoteToManagerAsync(request.UserId);

                    if (!success)
                    {
                        return BadRequest(new
                        {
                            Message = "Failed to promote user. User may not exist.",
                            CorrelationId = correlationId
                        });
                    }

                    return Ok(new
                    {
                        Message = "User promoted to Manager successfully.",
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while promoting user to Manager.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet("managers-by-department/{department}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetManagersByDepartment(string department)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var managers = await _userManager.GetManagersByDepartmentAsync(department);

                    if (!managers.Any())
                    {
                        return NotFound(new
                        {
                            Message = $"No managers found for department: {department}",
                            CorrelationId = correlationId
                        });
                    }

                    return Ok(new
                    {
                        Message = "Managers retrieved successfully.",
                        Data = managers,
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while fetching managers.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }
    }
}
