using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Middleware.Handlers;
public class DefaultExceptionHandler : ExceptionHandlerBase
{
    public override ProblemDetails? Handle(Exception exception, HttpContext context, bool isDevelopment)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = isDevelopment ? exception.Message : null,
            Instance = $"{context.Request.Method} {context.Request.Path}"
        };
    }
}