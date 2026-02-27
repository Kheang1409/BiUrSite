using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Tests.TestFixtures;
using Xunit;

namespace Tests.Integration;

public class EndToEndNotificationFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EndToEndNotificationFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private string ExtractTokenFromLoginResponse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("login", out var login) && login.TryGetProperty("token", out var tokenProp))
        {
            return tokenProp.GetString()!;
        }
        throw new InvalidOperationException("token not found in login response");
    }

    [Fact]
    public async Task SignalR_And_GraphQL_Notification_Flow_Works_EndToEnd()
    {
        var client = _factory.CreateClient();
        var baseUrl = _factory.Server.BaseAddress!.ToString().TrimEnd('/');

        // Register owner
        var ownerEmail = $"owner-{Guid.NewGuid()}@example.com";
        var ownerUser = new { Email = ownerEmail, Username = "owner", Password = "P@ssW0rd123!" };
        var registerOwnerQuery = JsonSerializer.Serialize(new
        {
            query = $"mutation {{ register(input: {{ email:\"{ownerUser.Email}\", username:\"{ownerUser.Username}\", password:\"{ownerUser.Password}\" }}) {{ id username email }} }}"
        });
        var resOwner = await client.PostAsync("/graphql", new StringContent(registerOwnerQuery, Encoding.UTF8, "application/json"));
        resOwner.EnsureSuccessStatusCode();

        // Login owner -> token
        var loginOwnerQuery = JsonSerializer.Serialize(new { query = $"mutation {{ login(email:\"{ownerUser.Email}\", password:\"{ownerUser.Password}\") {{ token }} }}" });
        var resLoginOwner = await client.PostAsync("/graphql", new StringContent(loginOwnerQuery, Encoding.UTF8, "application/json"));
        resLoginOwner.EnsureSuccessStatusCode();
        var ownerToken = ExtractTokenFromLoginResponse(await resLoginOwner.Content.ReadAsStringAsync());

        // Register commenter
        var commenterEmail = $"commenter-{Guid.NewGuid()}@example.com";
        var commenterUser = new { Email = commenterEmail, Username = "commenter", Password = "P@ssW0rd123!" };
        var registerCommenterQuery = JsonSerializer.Serialize(new
        {
            query = $"mutation {{ register(input: {{ email:\"{commenterUser.Email}\", username:\"{commenterUser.Username}\", password:\"{commenterUser.Password}\" }}) {{ id username email }} }}"
        });
        var resCommenter = await client.PostAsync("/graphql", new StringContent(registerCommenterQuery, Encoding.UTF8, "application/json"));
        resCommenter.EnsureSuccessStatusCode();

        // Login commenter -> token
        var loginCommenterQuery = JsonSerializer.Serialize(new { query = $"mutation {{ login(email:\"{commenterUser.Email}\", password:\"{commenterUser.Password}\") {{ token }} }}" });
        var resLoginCommenter = await client.PostAsync("/graphql", new StringContent(loginCommenterQuery, Encoding.UTF8, "application/json"));
        resLoginCommenter.EnsureSuccessStatusCode();
        var commenterToken = ExtractTokenFromLoginResponse(await resLoginCommenter.Content.ReadAsStringAsync());

        // Start SignalR connection as owner and listen for notification
        var tcs = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);

        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(baseUrl + "/notificationHub"), options =>
            {
                options.AccessTokenProvider = () => Task.FromResult<string?>(ownerToken);
                // Use the test server handler so SignalR client negotiates against the in-memory test server
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            })
            .WithAutomaticReconnect()
            .Build();

        connection.On<object>("ReceiveCommentNotification", payload =>
        {
            try
            {
                tcs.TrySetResult(JsonSerializer.Serialize(payload));
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        await connection.StartAsync();

        // Create a post as owner
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var createPostQuery = JsonSerializer.Serialize(new { query = $"mutation {{ createPost(text:\"Hello from owner\") {{ id }} }}" });
        var resCreatePost = await client.PostAsync("/graphql", new StringContent(createPostQuery, Encoding.UTF8, "application/json"));
        resCreatePost.EnsureSuccessStatusCode();
        var createPostText = await resCreatePost.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(createPostText);
        var postId = doc.RootElement.GetProperty("data").GetProperty("createPost").GetProperty("id").GetString();
        postId.Should().NotBeNullOrEmpty();

        // Create comment as commenter (should trigger notification to owner)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", commenterToken);
        var createCommentQuery = JsonSerializer.Serialize(new { query = $"mutation {{ createComment(postId: \"{postId}\", text:\"Nice post!\") {{ id }} }}" });
        var resCreateComment = await client.PostAsync("/graphql", new StringContent(createCommentQuery, Encoding.UTF8, "application/json"));
        resCreateComment.EnsureSuccessStatusCode();

        // Wait for SignalR notification (timeout after 5s)
        var finished = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromSeconds(5)));
        finished.Should().BeSameAs(tcs.Task, "SignalR notification should arrive within timeout");
        var payloadJson = await tcs.Task;
        payloadJson.Should().NotBeNullOrEmpty();

        // Query Notifications GraphQL endpoint as owner and assert persisted notification exists
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerToken);
        var notificationsQuery = JsonSerializer.Serialize(new { query = "query { notifications(pageNumber:1){ id message createdDate } }" });
        var resNotifications = await client.PostAsync("/graphql", new StringContent(notificationsQuery, Encoding.UTF8, "application/json"));
        resNotifications.EnsureSuccessStatusCode();
        var notifText = await resNotifications.Content.ReadAsStringAsync();
        notifText.Should().Contain("data");
        notifText.Should().Contain("notifications");
        notifText.Should().Contain("Nice post!");

        await connection.StopAsync();
    }
}
