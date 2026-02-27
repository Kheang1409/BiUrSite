using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit;
using Backend.API.GraphQL;
using FluentAssertions;

namespace Tests.Unit
{
    public class GraphQLUserContextTests
    {
        [Fact]
        public void GetRequiredUserId_From_NameIdentifier_ReturnsGuid()
        {
            var guid = Guid.NewGuid();
            var ctx = new DefaultHttpContext();
            ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, guid.ToString()) }));

            var accessor = new HttpContextAccessor { HttpContext = ctx };
            var g = new GraphQLUserContext(accessor);

            var id = g.GetRequiredUserId();
            id.Should().Be(guid);
        }

        [Fact]
        public void GetRequiredUserId_From_IdClaim_ReturnsGuid()
        {
            var guid = Guid.NewGuid();
            var ctx = new DefaultHttpContext();
            ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("id", guid.ToString()) }));

            var accessor = new HttpContextAccessor { HttpContext = ctx };
            var g = new GraphQLUserContext(accessor);

            var id = g.GetRequiredUserId();
            id.Should().Be(guid);
        }

        [Fact]
        public void GetRequiredUserId_From_SubClaim_ReturnsGuid()
        {
            var guid = Guid.NewGuid();
            var ctx = new DefaultHttpContext();
            ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, guid.ToString()) }));

            var accessor = new HttpContextAccessor { HttpContext = ctx };
            var g = new GraphQLUserContext(accessor);

            var id = g.GetRequiredUserId();
            id.Should().Be(guid);
        }

        [Fact]
        public void GetRequiredUserId_MissingClaim_ThrowsUnauthorizedAccessException()
        {
            var ctx = new DefaultHttpContext();
            ctx.User = new ClaimsPrincipal(new ClaimsIdentity());

            var accessor = new HttpContextAccessor { HttpContext = ctx };
            var g = new GraphQLUserContext(accessor);

            Action act = () => g.GetRequiredUserId();
            act.Should().Throw<UnauthorizedAccessException>();
        }
    }
}
