namespace Backend.Middllewares{

    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger, IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                
                var errorResponse = new
                {
                    message = "An unexpected error occurred. Please try again later.",
                    details = ex.Message,
                    stackTrace = _env.IsDevelopment() ? ex.StackTrace : null  
                };

                await context.Response.WriteAsJsonAsync(errorResponse); 
            }
        }
    }
}
