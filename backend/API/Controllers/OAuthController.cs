using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers
{
    [ApiController]
    public class OAuthController : ControllerBase
    {
        [HttpGet("/signin-facebook")]
        public IActionResult FacebookOAuthCallback([FromQuery] string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return Ok("Facebook OAuth callback is active.");
            }
            return BadRequest();
        }
    }
}