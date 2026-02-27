using Backend.API.Middleware.Handlers;
using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Middleware;

public class ExceptionHandlerContext
{
    private readonly IExceptionHandler _chain;

    public ExceptionHandlerContext(IHostEnvironment env)
    {
        var validation = new ValidationExceptionHandler();
        var unauthorized = new UnauthorizedAccessExceptionHandler();
        var conflict = new ConflictExceptionHandler();
        var notFound = new NotFoundExceptionHandler();
        var forbidden = new ForbiddenHandler();
        var fallback = new DefaultExceptionHandler();

        validation
            .SetNext(unauthorized)
            .SetNext(conflict)
            .SetNext(notFound)
            .SetNext(forbidden)
            .SetNext(fallback);

        _chain = validation;
        IsDevelopment = env.IsDevelopment();
    }

    private bool IsDevelopment { get; }

    public ProblemDetails Handle(Exception ex, HttpContext context)
    {
        return _chain.Handle(ex, context, IsDevelopment)!;
    }
}