using System.Threading.Tasks;
using CoreLayer.Dtos.Admin;
using static CoreLayer.Dtos.Admin.AdminDashboardDTOs;

namespace CoreLayer.Service_Interface.Admin
{
    public interface IAdminDashboardService
    {
        Task<UserStatisticsDto> GetUserStatisticsAsync();
        Task<AnimalStatisticsDto> GetAnimalStatisticsAsync();
        Task<OrderStatisticsDto> GetOrderStatisticsAsync();
        Task<ListingStatisticsDto> GetListingStatisticsAsync();
        Task<HealthcareStatisticsDto> GetHealthcareStatisticsAsync();
        Task<AdminDashboardOverviewDto> GetDashboardOverviewAsync();
    }
}