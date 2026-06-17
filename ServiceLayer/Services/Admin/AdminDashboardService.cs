using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreLayer.Dtos.Admin;
using CoreLayer.Enums;
using CoreLayer.Service_Interface.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Data.Context;
using CoreLayer.Entities.Identity;
using static CoreLayer.Dtos.Admin.AdminDashboardDTOs;

namespace ServiceLayer.Services.Admin
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminDashboardService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var totalUsers = allUsers.Count;

            // Count users by role
            var normalUsers = 0;
            var doctorUsers = 0;
            var pharmacyUsers = 0;
            var adminAssistants = 0;

            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Doctor"))
                    doctorUsers++;
                else if (await _userManager.IsInRoleAsync(user, "Pharmacy"))
                    pharmacyUsers++;
                else if (await _userManager.IsInRoleAsync(user, "AdminAssistant"))
                    adminAssistants++;
                else
                    normalUsers++;
            }

            var statistics = new UserStatisticsDto
            {
                TotalUsers = totalUsers,
                NormalUsers = normalUsers,
                DoctorUsers = doctorUsers,
                PharmacyUsers = pharmacyUsers,
                AdminAssistants = adminAssistants,
                NormalUserPercentage = totalUsers > 0 ? Math.Round((normalUsers * 100m) / totalUsers, 2) : 0,
                DoctorUserPercentage = totalUsers > 0 ? Math.Round((doctorUsers * 100m) / totalUsers, 2) : 0,
                PharmacyUserPercentage = totalUsers > 0 ? Math.Round((pharmacyUsers * 100m) / totalUsers, 2) : 0,
                AdminAssistantPercentage = totalUsers > 0 ? Math.Round((adminAssistants * 100m) / totalUsers, 2) : 0
            };

            return statistics;
        }

        public async Task<AnimalStatisticsDto> GetAnimalStatisticsAsync()
        {
            var animals = await _context.Animals
                .Include(a => a.Species)
                .ToListAsync();

            var totalAnimals = animals.Count;
            var totalActiveAnimals = animals.Count(a => a.IsActive);
            var totalInactiveAnimals = animals.Count(a => !a.IsActive);

            // Group by species
            var animalsBySpecies = animals
                .GroupBy(a => a.Species?.Name ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate percentages
            var speciesPercentages = new Dictionary<string, decimal>();
            foreach (var species in animalsBySpecies)
            {
                var percentage = totalAnimals > 0 
                    ? Math.Round((species.Value * 100m) / totalAnimals, 2) 
                    : 0;
                speciesPercentages.Add(species.Key, percentage);
            }

            return new AnimalStatisticsDto
            {
                TotalAnimals = totalAnimals,
                AnimalsBySpecies = animalsBySpecies,
                SpeciesPercentages = speciesPercentages,
                TotalActiveAnimals = totalActiveAnimals,
                TotalInactiveAnimals = totalInactiveAnimals
            };
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync()
        {
            var orders = await _context.Orders.ToListAsync();
            var totalOrders = orders.Count;

            var pendingOrders = orders.Count(o => o.Status == OrderStatus.Pending);
            var pendingPaymentOrders = orders.Count(o => o.Status == OrderStatus.PendingPayment);
            var processingOrders = orders.Count(o => o.Status == OrderStatus.Processing);
            var shippedOrders = orders.Count(o => o.Status == OrderStatus.Shipped);
            var deliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered);
            var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);

            var totalRevenue = orders.Sum(o => o.SubTotal - o.DiscountAmount);
            var averageOrderValue = totalOrders > 0 
                ? Math.Round(totalRevenue / totalOrders, 2) 
                : 0;

            return new OrderStatisticsDto
            {
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                PendingPaymentOrders = pendingPaymentOrders,
                ProcessingOrders = processingOrders,
                ShippedOrders = shippedOrders,
                DeliveredOrders = deliveredOrders,
                CancelledOrders = cancelledOrders,
                PendingPercentage = totalOrders > 0 ? Math.Round((pendingOrders * 100m) / totalOrders, 2) : 0,
                PendingPaymentPercentage = totalOrders > 0 ? Math.Round((pendingPaymentOrders * 100m) / totalOrders, 2) : 0,
                ProcessingPercentage = totalOrders > 0 ? Math.Round((processingOrders * 100m) / totalOrders, 2) : 0,
                ShippedPercentage = totalOrders > 0 ? Math.Round((shippedOrders * 100m) / totalOrders, 2) : 0,
                DeliveredPercentage = totalOrders > 0 ? Math.Round((deliveredOrders * 100m) / totalOrders, 2) : 0,
                CancelledPercentage = totalOrders > 0 ? Math.Round((cancelledOrders * 100m) / totalOrders, 2) : 0,
                TotalRevenue = Math.Round(totalRevenue, 2),
                AverageOrderValue = averageOrderValue
            };
        }

        public async Task<ListingStatisticsDto> GetListingStatisticsAsync()
        {
            var animalListings = await _context.AnimalListings.ToListAsync();
            var accessoryListings = await _context.AccessoryListings.ToListAsync();
            var pharmacyListings = await _context.PharmacyListings.ToListAsync();

            // Group animal listings by type
            var animalListingsByType = animalListings
                .GroupBy(al => al.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count());

            // For accessories, we'll group by category if available
            var accessoriesByCategory = new Dictionary<string, int>();
            // Note: Adjust this based on your AccessoryListing entity structure

            return new ListingStatisticsDto
            {
                TotalAnimalListings = animalListings.Count,
                TotalAccessoryListings = accessoryListings.Count,
                TotalPharmacyListings = pharmacyListings.Count,
                AnimalListingsByType = animalListingsByType,
                AccessoriesByCategory = accessoriesByCategory
            };
        }

        public async Task<HealthcareStatisticsDto> GetHealthcareStatisticsAsync()
        {
            var doctorApplications = await _context.DoctorApplications.ToListAsync();
            var pharmacyApplications = await _context.PharmacyApplications.ToListAsync();
            var doctorRatings = await _context.DoctorRatings.ToListAsync();
            var pharmacyRatings = await _context.PharmacyRatings.ToListAsync();

            var approvedDoctors = doctorApplications.Count(da => da.Status == ApplicationStatus.Approved);
            var pendingDoctorApplications = doctorApplications.Count(da => da.Status == ApplicationStatus.Pending);
            var rejectedDoctorApplications = doctorApplications.Count(da => da.Status == ApplicationStatus.Rejected);

            var approvedPharmacies = pharmacyApplications.Count(pa => pa.Status == ApplicationStatus.Approved);
            var pendingPharmacyApplications = pharmacyApplications.Count(pa => pa.Status == ApplicationStatus.Pending);
            var rejectedPharmacyApplications = pharmacyApplications.Count(pa => pa.Status == ApplicationStatus.Rejected);

            var averageDoctorRating = doctorRatings.Count > 0 
                ? Math.Round(doctorRatings.Average(dr => dr.Rating), 2) 
                : 0;
            var averagePharmacyRating = pharmacyRatings.Count > 0 
                ? Math.Round(pharmacyRatings.Average(pr => pr.Rating), 2) 
                : 0;

            return new HealthcareStatisticsDto
            {
                ApprovedDoctors = approvedDoctors,
                PendingDoctorApplications = pendingDoctorApplications,
                RejectedDoctorApplications = rejectedDoctorApplications,
                ApprovedPharmacies = approvedPharmacies,
                PendingPharmacyApplications = pendingPharmacyApplications,
                RejectedPharmacyApplications = rejectedPharmacyApplications,
                AverageDoctorRating = (decimal)averageDoctorRating,
                AveragePharmacyRating = (decimal)averagePharmacyRating
            };
        }

        public async Task<AdminDashboardOverviewDto> GetDashboardOverviewAsync()
        {
            var userStats = await GetUserStatisticsAsync();
            var animalStats = await GetAnimalStatisticsAsync();
            var orderStats = await GetOrderStatisticsAsync();

            return new AdminDashboardOverviewDto
            {
                UserStatistics = userStats,
                AnimalStatistics = animalStats,
                OrderStatistics = orderStats,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}