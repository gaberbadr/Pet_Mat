using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    [ApiController]
    [Route("error")] // Changed from inheriting BaseApiController route
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : BaseApiController
    {
        [HttpGet("{code}")]
        public IActionResult Error(int code)
        {
            return code switch
            {
                404 => NotFound(new ApiErrorResponse(404, "The requested endpoint was not found")),
                401 => Unauthorized(new ApiErrorResponse(401, "Unauthorized access")),
                403 => StatusCode(403, new ApiErrorResponse(403, "Forbidden")),
                500 => StatusCode(500, new ApiErrorResponse(500, "Internal server error")),
                _ => StatusCode(code, new ApiErrorResponse(code))
            };
        }
    }
}