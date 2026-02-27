using Backend.Application.Configuration;

namespace Backend.Infrastructure.Configuration;
public class AppOptions: IAppOptions
{
    public string BaseUrl { get; set; } = string.Empty;
}