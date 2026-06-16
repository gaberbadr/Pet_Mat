using CoreLayer.Dtos.Doctor;
using CoreLayer.Service_Interface.Doctor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;

namespace petmat.Controllers
{
    /// <summary>
    /// Admin-only CRUD for subscription packages (Standard, Premium, etc.)
    /// Base route: api/admin/packages
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class PackageController : BaseApiController
    {
        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        // GET api/admin/packages
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PackageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PackageDto>>> GetAll([FromQuery] bool includeInactive = false)
        {
            var packages = await _packageService.GetAllPackagesAsync(includeInactive);
            return Ok(packages);
        }

        // GET api/admin/packages/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PackageDto>> GetById(int id)
        {
            try
            {
                var package = await _packageService.GetPackageByIdAsync(id);
                return Ok(package);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        // POST api/admin/packages
        [HttpPost]
        [ProducesResponseType(typeof(PackageDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PackageDto>> Create([FromBody] CreatePackageDto dto)
        {
            var created = await _packageService.CreatePackageAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT api/admin/packages/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PackageDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PackageDto>> Update(int id, [FromBody] UpdatePackageDto dto)
        {
            try
            {
                var updated = await _packageService.UpdatePackageAsync(id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }

        // DELETE api/admin/packages/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _packageService.DeletePackageAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiErrorResponse(404, ex.Message));
            }
        }
    }
}
