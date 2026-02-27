using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Middleware.Handlers;
public abstract class ExceptionHandlerBase : IExceptionHandler
{
    private IExceptionHandler? _next;

    public IExceptionHandler SetNext(IExceptionHandler handler)
    {
        _next = handler;
        return handler;
    }

    public virtual ProblemDetails? Handle(Exception exception, HttpContext context, bool isDevelopment)
    {
        return _next?.Handle(exception, context, isDevelopment);
    }
}
