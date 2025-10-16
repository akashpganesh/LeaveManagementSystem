using LeaveManagement.Managers;
using LeaveManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LeaveManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveRequestController : ControllerBase
    {
        private readonly ILeaveRequestManager _leaveManager;

        public LeaveRequestController(ILeaveRequestManager leaveManager)
        {
            _leaveManager = leaveManager;
        }

        [HttpPost("request")]
        [Authorize]
        public async Task<IActionResult> RequestLeave([FromBody] CreateLeaveRequestDto dto)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var employeeId = int.Parse(User.FindFirst("UserId")?.Value);

                    if (dto.StartDate > dto.EndDate)
                        return BadRequest(new { Message = "StartDate cannot be after EndDate.", CorrelationId = correlationId });

                    var leaveId = await _leaveManager.RequestLeave(employeeId, dto);

                    return Ok(new
                    {
                        Message = "Leave request submitted successfully.",
                        LeaveId = leaveId,
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while submitting leave request.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetLeaveRequests([FromQuery] int? months = null)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    IEnumerable<LeaveRequestDto> leaveRequests;
                    LeaveStatusCountDto counts;

                    if (User.IsInRole("Admin"))
                    {
                        var result = await _leaveManager.GetAllLeaveRequestsAsync(null, months);
                        leaveRequests = result.LeaveRequests;
                        counts = result.Counts;

                        var grouped = leaveRequests
                            .GroupBy(lr => lr.Department)
                            .ToDictionary(g => g.Key, g => g.ToList());

                        return Ok(new
                        {
                            Message = "Leave requests retrieved successfully (grouped by department).",
                            Data = grouped,
                            Counts = counts,
                            CorrelationId = correlationId
                        });
                    }
                    else if (User.IsInRole("Manager"))
                    {
                        var managerId = int.Parse(User.FindFirst("UserId")?.Value);
                        var result = await _leaveManager.GetAllLeaveRequestsAsync(managerId, months);
                        leaveRequests = result.LeaveRequests;
                        counts = result.Counts;

                        return Ok(new
                        {
                            Message = "Leave requests retrieved successfully.",
                            Data = leaveRequests,
                            Counts = counts,
                            CorrelationId = correlationId
                        });
                    }
                    else
                    {
                        return StatusCode(403, new
                        {
                            Message = "You are not authorized to access this resource.",
                            CorrelationId = correlationId
                        });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while fetching leave requests.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet("MyLeaveRequests")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMyLeaveRequests()
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    // Get the current employee ID from token
                    var employeeIdClaim = User.FindFirst("UserId")?.Value;
                    if (string.IsNullOrEmpty(employeeIdClaim))
                    {
                        return Unauthorized(new
                        {
                            Message = "Employee ID not found in token.",
                            CorrelationId = correlationId
                        });
                    }

                    int employeeId = int.Parse(employeeIdClaim);

                    var leaveRequests = await _leaveManager.GetLeaveRequestsByEmployeeIdAsync(employeeId);

                    return Ok(new
                    {
                        Message = "Your leave requests retrieved successfully.",
                        Data = leaveRequests,
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while fetching your leave requests.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet("{leaveId}")]
        [Authorize]
        public async Task<IActionResult> GetLeaveRequestById(int leaveId)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var leaveRequest = await _leaveManager.GetLeaveRequestByIdAsync(leaveId);

                    if (leaveRequest == null)
                        return NotFound(new
                        {
                            Message = "Leave request not found.",
                            CorrelationId = correlationId
                        });

                    var currentUserId = int.Parse(User.FindFirst("UserId")?.Value);
                    var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

                    // Employee can only view their own request
                    if (currentUserRole == "Employee" && leaveRequest.EmployeeId != currentUserId)
                    {
                        return StatusCode(403, new
                        {
                            Message = "You are not authorized to access this leave request.",
                            CorrelationId = correlationId
                        });
                    }

                    // Manager can only view requests of their employees
                    if (currentUserRole == "Manager" && leaveRequest.EmployeeId != currentUserId)
                    {
                        if (leaveRequest.ManagerId != currentUserId)
                        {
                            return StatusCode(403, new
                            {
                                Message = "You are not authorized to access this leave request.",
                                CorrelationId = correlationId
                            });
                        }
                    }

                    return Ok(new
                    {
                        Message = "Leave request retrieved successfully.",
                        Data = leaveRequest,
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while fetching the leave request.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpPost("{leaveId}/UpdateStatus")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> UpdateLeaveStatus(int leaveId, [FromBody] UpdateLeaveStatusRequest request)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    // Validate status
                    if (request.Status != "Approved" && request.Status != "Rejected")
                        return BadRequest(new
                        {
                            Message = "Invalid status. Only 'Approved' or 'Rejected' are allowed.",
                            CorrelationId = correlationId
                        });

                    // Manager can only approve/reject requests of their employees
                    if (User.IsInRole("Manager"))
                    {
                        var leaveRequest = await _leaveManager.GetLeaveRequestByIdAsync(leaveId);
                        var currentManagerId = int.Parse(User.FindFirst("UserId")?.Value);
                        if (leaveRequest.ManagerId != currentManagerId)
                        {
                            return StatusCode(403, new
                            {
                                Message = "You are not authorized to update this leave request.",
                                CorrelationId = correlationId
                            });
                        }
                    }

                    var result = await _leaveManager.UpdateLeaveStatusAsync(leaveId, request.Status);
                    if (!result)
                        return NotFound(new
                        {
                            Message = "Leave request not found.",
                            CorrelationId = correlationId
                        });

                    return Ok(new
                    {
                        Message = $"Leave request {request.Status.ToLower()} successfully.",
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while updating leave status.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet("GetDashboard")]
        [Authorize]
        public async Task<IActionResult> GetDashboard()
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    // Extract UserId from JWT token
                    var userIdClaim = User.FindFirst("UserId")?.Value;
                    if (string.IsNullOrEmpty(userIdClaim))
                    {
                        return Unauthorized(new
                        {
                            Message = "User ID not found in token.",
                            CorrelationId = correlationId
                        });
                    }

                    int employeeId = int.Parse(userIdClaim);

                    // Fetch dashboard details
                    var dashboard = await _leaveManager.GetDashboardAsync(employeeId);

                    if (dashboard == null)
                    {
                        return NotFound(new
                        {
                            Message = "Dashboard data not found.",
                            CorrelationId = correlationId
                        });
                    }

                    return Ok(new
                    {
                        Message = "Dashboard data retrieved successfully.",
                        Data = dashboard,
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while fetching dashboard data.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }
    }
}
