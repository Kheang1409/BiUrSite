
using Backend.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Middleware.Handlers;
public class ForbiddenHandler  : ExceptionHandlerBase
{
    public override ProblemDetails? Handle(Exception exception, HttpContext context, bool isDevelopment)
    {
        if (exception is ForbiddenException forbidden)
        {
            // Return 403 Forbidden
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            return new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Forbidden",
                Detail = isDevelopment ? forbidden.Message : null,
                Instance = $"{context.Request.Method} {context.Request.Path}"
            };
        }

        return base.Handle(exception, context, isDevelopment);
    }
}