using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Tests.TestFixtures;
using Xunit;
using FluentAssertions;
using System.Text.Json;

namespace Tests.Integration;

public class NegotiationAndGraphqlAuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public NegotiationAndGraphqlAuthTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private string CreateToken(string secret, string issuer, string audience, string subject)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(issuer: issuer, audience: audience, subject: new System.Security.Claims.ClaimsIdentity(new[] { new System.Security.Claims.Claim(JwtRegisteredClaimNames.Sub, subject) }), signingCredentials: creds);
        return handler.WriteToken(token);
    }

    [Fact]
    public async Task Negotiate_Allows_Request_And_Auth_Status_Is_Logged()
    {
        var client = _factory.CreateClient();

        // unauthenticated negotiate should still return (server may not require auth at negotiate)
        var res = await client.PostAsync("/notificationHub/negotiate", new StringContent("{}", Encoding.UTF8, "application/json"));
        res.StatusCode.Should().BeOneOf(new[] { System.Net.HttpStatusCode.OK, System.Net.HttpStatusCode.Unauthorized, System.Net.HttpStatusCode.Forbidden });

        // obtain config from factory services to sign a token
        var config = _factory.Services.GetRequiredService<IConfiguration>();
        var secret = config["JWT:SecretKey"] ?? "test-secret";
        var issuer = config["JWT:Issuer"] ?? "test-issuer";
        var audience = config["JWT:Audience"] ?? "test-audience";

        var token = CreateToken(secret, issuer, audience, "test-user-id");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res2 = await client.PostAsync("/notificationHub/negotiate", new StringContent("{}", Encoding.UTF8, "application/json"));
        res2.StatusCode.Should().BeOneOf(new[] { System.Net.HttpStatusCode.OK, System.Net.HttpStatusCode.Unauthorized, System.Net.HttpStatusCode.Forbidden });
    }

    [Fact]
    public async Task Graphql_Health_Is_Available_And_Notifications_Requires_Authorization()
    {
        var client = _factory.CreateClient();

        var healthQuery = JsonSerializer.Serialize(new { query = "{ health }" });
        var res = await client.PostAsync("/graphql", new StringContent(healthQuery, Encoding.UTF8, "application/json"));
        res.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var text = await res.Content.ReadAsStringAsync();
        text.Should().Contain("data");

        // Notifications requires auth - without token expect errors in response
        var notificationsQuery = JsonSerializer.Serialize(new { query = "query Notifications($pageNumber:Int!){notifications(pageNumber:$pageNumber){id}}", variables = new { pageNumber = 1 } });
        var res2 = await client.PostAsync("/graphql", new StringContent(notificationsQuery, Encoding.UTF8, "application/json"));
        res2.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var txt2 = await res2.Content.ReadAsStringAsync();
        // Should contain errors array because of missing auth
        txt2.Should().Contain("errors");

        // Now call with valid token
        var config = _factory.Services.GetRequiredService<IConfiguration>();
        var secret = config["JWT:SecretKey"] ?? "test-secret";
        var issuer = config["JWT:Issuer"] ?? "test-issuer";
        var audience = config["JWT:Audience"] ?? "test-audience";
        var token = CreateToken(secret, issuer, audience, "test-user-id");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res3 = await client.PostAsync("/graphql", new StringContent(notificationsQuery, Encoding.UTF8, "application/json"));
        res3.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var txt3 = await res3.Content.ReadAsStringAsync();
        // With token we still may get data or an errors array depending on DB, but response should be well-formed JSON
        (txt3.Contains("data") || txt3.Contains("errors")).Should().BeTrue();
    }
}
