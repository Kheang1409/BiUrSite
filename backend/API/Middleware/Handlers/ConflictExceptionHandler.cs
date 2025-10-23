using Backend.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Middleware.Handlers;
public class ConflictExceptionHandler  : ExceptionHandlerBase
{
    public override ProblemDetails? Handle(Exception exception, HttpContext context, bool isDevelopment)
    {
        if (exception is ConflictException conflictEx)
        {
            // Return 409 Conflict
            context.Response.StatusCode = StatusCodes.Status409Conflict;

            return new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = isDevelopment ? conflictEx.Message : null,
                Instance = $"{context.Request.Method} {context.Request.Path}"
            };
        }

        return base.Handle(exception, context, isDevelopment);
    }
}