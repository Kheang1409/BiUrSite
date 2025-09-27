using Backend.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Backend.Infrastructure.Configuration;
public class AppOptionsAdapter : IAppOptions
{
    private readonly AppOptions _options;

    public AppOptionsAdapter(IOptions<AppOptions> options)
    {
        _options = options.Value;
    }

    public string BaseUrl => _options.BaseUrl;
}