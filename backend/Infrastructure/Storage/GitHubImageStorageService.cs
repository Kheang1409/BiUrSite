using Backend.Application.Storage;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Backend.Infrastructure.Storage;

public class GitHubImageStorageService : IImageStorageService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _token;
    private readonly string _username;
    private readonly string _repo;
    private readonly string _branch;

    public GitHubImageStorageService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _token = Environment.GetEnvironmentVariable("GIT_TOKEN")
                    ?? configuration["GitHub:Token"]
                    ?? throw new InvalidOperationException("GitHub Token is not configured.");
        _username = Environment.GetEnvironmentVariable("GIT_USERNAME")
                    ?? configuration["GitHub:Username"]
                    ?? throw new InvalidOperationException("GitHub Username is not configured.");
        _repo = Environment.GetEnvironmentVariable("GIT_REPO")
                    ?? configuration["GitHub:Repo"]
                    ?? throw new InvalidOperationException("GitHub Repo is not configured.");
        _branch = Environment.GetEnvironmentVariable("GIT_BRANCH")
                    ?? configuration["GitHub:Branch"]
                    ?? throw new InvalidOperationException("GitHub Branch is not configured.");
    }

    public async Task<string> UploadImageAsync(string fileName, byte[] content)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("BackendApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        string? sha = null;
        var getUrl = $"https://api.github.com/repos/{_username}/{_repo}/contents/{fileName}";
        var getResponse = await client.GetAsync(getUrl);

        if (getResponse.IsSuccessStatusCode)
        {
            var getContent = await getResponse.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(getContent);
            sha = doc.RootElement.TryGetProperty("sha", out var shaProp) ? shaProp.GetString() : null;
        }

        var payload = new
        {
            message = sha == null ? $"Upload {fileName}" : $"Replace {fileName}",
            content = Convert.ToBase64String(content),
            branch = _branch,
            sha
        };

        var json = JsonSerializer.Serialize(payload);
        var body = new StringContent(json, Encoding.UTF8, "application/json");

        var putUrl = $"https://api.github.com/repos/{_username}/{_repo}/contents/{fileName}";
        var response = await client.PutAsync(putUrl, body);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"GitHub upload failed: {response.StatusCode} - {error}");
        }

        var cacheBuster = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return $"https://raw.githubusercontent.com/{_username}/{_repo}/{_branch}/{fileName}?v={cacheBuster}";
    }

    public async Task DeleteImageAsync(string fileName)
    {
        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("BackendApi");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        var getUrl = $"https://api.github.com/repos/{_username}/{_repo}/contents/{fileName}";
        var getResponse = await client.GetAsync(getUrl);

        if (!getResponse.IsSuccessStatusCode)
            return; 

        var getContent = await getResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(getContent);
        var sha = doc.RootElement.GetProperty("sha").GetString();
        if (sha == null)
            throw new Exception("Failed to get file SHA for deletion");

        var payload = new
        {
            message = $"Delete {fileName}",
            sha,
            branch = _branch
        };

        var json = JsonSerializer.Serialize(payload);
        var body = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Delete, getUrl)
        {
            Content = body
        };

        var deleteResponse = await client.SendAsync(request);
        if (!deleteResponse.IsSuccessStatusCode)
        {
            var error = await deleteResponse.Content.ReadAsStringAsync();
            throw new Exception($"GitHub delete failed: {deleteResponse.StatusCode} - {error}");
        }
    }
}
