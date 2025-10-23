using Microsoft.AspNetCore.Mvc;

namespace Backend.API.Controllers
{
    [ApiController]
    [Route("api/health")]
    public class HealthController : ControllerBase
    {
        [HttpHead]
        [HttpGet]
        public IActionResult Head()
        {
            return Ok();
        }
    }
}