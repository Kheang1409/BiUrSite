using Backend.SharedKernel.Exceptions;
using HotChocolate;
using HotChocolate.Execution;
using Microsoft.Extensions.Hosting;

namespace Backend.API.GraphQL;

internal sealed class GraphQLErrorFilter : IErrorFilter
{
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GraphQLErrorFilter> _logger;

    public GraphQLErrorFilter(IHostEnvironment environment, ILogger<GraphQLErrorFilter> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public IError OnError(IError error)
    {
        if (error.Exception is null)
            return error;

        try
        {
            _logger.LogError(error.Exception, "GraphQL error on path {Path}: {Message}", error.Path, error.Exception.Message);
        }
        catch { }

        return error.Exception switch
        {
            NotFoundException => error.WithCode("NOT_FOUND").WithMessage(error.Exception.Message),
            ForbiddenException => error.WithCode("FORBIDDEN").WithMessage(error.Exception.Message),
            ConflictException => error.WithCode("CONFLICT").WithMessage(error.Exception.Message),
            UnauthorizedAccessException => error.WithCode("UNAUTHORIZED").WithMessage(error.Exception.Message),
            _ when _environment.IsDevelopment() => error.WithCode("UNEXPECTED").WithMessage(error.Exception.Message),
            _ => error
        };
    }
}
