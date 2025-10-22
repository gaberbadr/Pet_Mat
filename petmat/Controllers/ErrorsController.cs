using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : BaseApiController
    {
        public IActionResult Error(int code)
        {
            //#8 in program

            return NotFound(new ApiErrorResponse(StatusCodes.Status404NotFound, "Not Found End Point !"));
        }
    }
}
