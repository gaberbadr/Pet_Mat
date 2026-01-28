using Microsoft.AspNetCore.Mvc;

namespace petmat.Controllers
{
    [ApiController]
    [Route("")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new
            {
                message = "Welcome to PetMat API",
                version = "4.3",
                status = "Running",
                documentation = $"{Request.Scheme}://{Request.Host}/swagger",
                endpoints = new
                {
                    swagger = $"{Request.Scheme}://{Request.Host}/swagger",
                    health = $"{Request.Scheme}://{Request.Host}/health"
                },
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }
    }
}