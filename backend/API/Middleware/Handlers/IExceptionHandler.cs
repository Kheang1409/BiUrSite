using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Middleware.Handlers;
public interface IExceptionHandler
{
    IExceptionHandler SetNext(IExceptionHandler handler);
    ProblemDetails? Handle(Exception exception, HttpContext context, bool isDevelopment);
}