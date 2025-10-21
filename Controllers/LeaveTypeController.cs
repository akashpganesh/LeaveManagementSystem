using LeaveManagement.Managers;
using LeaveManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveTypeController : ControllerBase
    {
        private readonly ILeaveTypeManager _leaveTypeManager;

        public LeaveTypeController(ILeaveTypeManager leaveTypeManager)
        {
            _leaveTypeManager = leaveTypeManager;
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateLeaveType([FromBody] LeaveType model)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            if (string.IsNullOrWhiteSpace(model.LeaveTypeName) || model.MaxLeavesPerYear <= 0)
            {
                return BadRequest(new
                {
                    Message = "Invalid leave type details.",
                    CorrelationId = correlationId
                });
            }

            try
            {
                var success = await _leaveTypeManager.CreateLeaveTypeAsync(model.LeaveTypeName, model.MaxLeavesPerYear);
                if (!success)
                    return BadRequest(new { Message = "Failed to create leave type.", CorrelationId = correlationId });

                return Ok(new
                {
                    Message = "Leave type created successfully.",
                    CorrelationId = correlationId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while creating leave type.",
                    Details = ex.Message,
                    CorrelationId = correlationId
                });
            }
        }

        [HttpGet("GetAll")]
        [Authorize]
        public async Task<IActionResult> GetAllLeaveTypes()
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var leaveTypes = await _leaveTypeManager.GetAllLeaveTypesAsync();

                    return Ok(new
                    {
                        Message = "Leave types retrieved successfully.",
                        Data = leaveTypes,
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while fetching leave types.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLeaveTypeById(int id)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var leaveType = await _leaveTypeManager.GetLeaveTypeByIdAsync(id);

                    if (leaveType == null)
                        return NotFound(new
                        {
                            Message = "Leave type not found.",
                            CorrelationId = correlationId
                        });

                    return Ok(new
                    {
                        Message = "Leave type retrieved successfully.",
                        Data = leaveType,
                        CorrelationId = correlationId
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while fetching leave type.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLeaveType(int id, [FromBody] LeaveType model)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

            try
            {
                var success = await _leaveTypeManager.UpdateLeaveTypeAsync(
                    id,
                    string.IsNullOrWhiteSpace(model.LeaveTypeName) ? null : model.LeaveTypeName,
                    model.MaxLeavesPerYear > 0 ? model.MaxLeavesPerYear : null
                );

                if (!success)
                    return NotFound(new { Message = "Leave type not found or no changes applied.", CorrelationId = correlationId });

                return Ok(new { Message = "Leave type updated successfully.", CorrelationId = correlationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while updating leave type.",
                    Details = ex.Message,
                    CorrelationId = correlationId
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLeaveType(int id)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                try
                {
                    var success = await _leaveTypeManager.DeleteLeaveTypeAsync(id);

                    if (!success)
                        return NotFound(new { Message = "Leave type not found.", CorrelationId = correlationId });

                    return Ok(new { Message = "Leave type deleted successfully.", CorrelationId = correlationId });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new
                    {
                        Message = "An error occurred while deleting leave type.",
                        Details = ex.Message,
                        CorrelationId = correlationId
                    });
                }
            }
        }
    }
}