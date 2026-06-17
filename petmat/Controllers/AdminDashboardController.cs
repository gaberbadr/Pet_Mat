using System.Threading.Tasks;
using CoreLayer.Service_Interface.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using petmat.Errors;
using static CoreLayer.Dtos.Admin.AdminDashboardDTOs;

namespace petmat.Controllers
{
    [Authorize(Roles = "Admin,AdminAssistant")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminDashboardController : BaseApiController
    {
        private readonly IAdminDashboardService _dashboardService;

        public AdminDashboardController(IAdminDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get overall dashboard overview with all statistics
        /// </summary>
        [ProducesResponseType(typeof(AdminDashboardOverviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [HttpGet("overview")]
        public async Task<ActionResult<AdminDashboardOverviewDto>> GetDashboardOverview()
        {
            try
            {
                var result = await _dashboardService.GetDashboardOverviewAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Error retrieving dashboard overview: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get user statistics (total users, breakdown by role, and percentages)
        /// </summary>
        [ProducesResponseType(typeof(UserStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [HttpGet("users-statistics")]
        public async Task<ActionResult<UserStatisticsDto>> GetUserStatistics()
        {
            try
            {
                var result = await _dashboardService.GetUserStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Error retrieving user statistics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get animal statistics (total animals, breakdown by species, and percentages)
        /// </summary>
        [ProducesResponseType(typeof(AnimalStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [HttpGet("animals-statistics")]
        public async Task<ActionResult<AnimalStatisticsDto>> GetAnimalStatistics()
        {
            try
            {
                var result = await _dashboardService.GetAnimalStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Error retrieving animal statistics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get order statistics (total orders, breakdown by status, revenue, and percentages)
        /// </summary>
        [ProducesResponseType(typeof(OrderStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [HttpGet("orders-statistics")]
        public async Task<ActionResult<OrderStatisticsDto>> GetOrderStatistics()
        {
            try
            {
                var result = await _dashboardService.GetOrderStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Error retrieving order statistics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get listing statistics (animal listings, accessories, pharmacy listings)
        /// </summary>
        [ProducesResponseType(typeof(ListingStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [HttpGet("listings-statistics")]
        public async Task<ActionResult<ListingStatisticsDto>> GetListingStatistics()
        {
            try
            {
                var result = await _dashboardService.GetListingStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Error retrieving listing statistics: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get healthcare statistics (doctor and pharmacy applications, ratings)
        /// </summary>
        [ProducesResponseType(typeof(HealthcareStatisticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
        [HttpGet("healthcare-statistics")]
        public async Task<ActionResult<HealthcareStatisticsDto>> GetHealthcareStatistics()
        {
            try
            {
                var result = await _dashboardService.GetHealthcareStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorResponse(500, $"Error retrieving healthcare statistics: {ex.Message}"));
            }
        }
    }
}