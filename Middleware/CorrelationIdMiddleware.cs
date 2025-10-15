using Serilog.Context;

namespace LeaveManagement.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers.ContainsKey("X-Correlation-ID")
                ? context.Request.Headers["X-Correlation-ID"].ToString()
                : Guid.NewGuid().ToString();

            // Save to context for access in other layers
            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            // Push to Serilog context
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
