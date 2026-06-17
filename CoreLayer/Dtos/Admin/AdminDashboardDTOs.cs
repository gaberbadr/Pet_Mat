using System;
using System.Collections.Generic;

namespace CoreLayer.Dtos.Admin
{
    public class AdminDashboardDTOs
    {
        // ==================== USER STATISTICS ====================

        public class UserStatisticsDto
        {
            public int TotalUsers { get; set; }
            public int NormalUsers { get; set; }
            public int DoctorUsers { get; set; }
            public int PharmacyUsers { get; set; }
            public int AdminAssistants { get; set; }
            public decimal NormalUserPercentage { get; set; }
            public decimal DoctorUserPercentage { get; set; }
            public decimal PharmacyUserPercentage { get; set; }
            public decimal AdminAssistantPercentage { get; set; }
        }

        // ==================== ANIMAL STATISTICS ====================

        public class AnimalStatisticsDto
        {
            public int TotalAnimals { get; set; }
            public Dictionary<string, int> AnimalsBySpecies { get; set; }
            public Dictionary<string, decimal> SpeciesPercentages { get; set; }
            public int TotalActiveAnimals { get; set; }
            public int TotalInactiveAnimals { get; set; }
        }

        // ==================== ORDER STATISTICS ====================

        public class OrderStatisticsDto
        {
            public int TotalOrders { get; set; }
            public int PendingOrders { get; set; }
            public int PendingPaymentOrders { get; set; }
            public int ProcessingOrders { get; set; }
            public int ShippedOrders { get; set; }
            public int DeliveredOrders { get; set; }
            public int CancelledOrders { get; set; }
            public decimal PendingPercentage { get; set; }
            public decimal PendingPaymentPercentage { get; set; }
            public decimal ProcessingPercentage { get; set; }
            public decimal ShippedPercentage { get; set; }
            public decimal DeliveredPercentage { get; set; }
            public decimal CancelledPercentage { get; set; }
            public decimal TotalRevenue { get; set; }
            public decimal AverageOrderValue { get; set; }
        }

        // ==================== OVERALL DASHBOARD ====================

        public class AdminDashboardOverviewDto
        {
            public UserStatisticsDto UserStatistics { get; set; }
            public AnimalStatisticsDto AnimalStatistics { get; set; }
            public OrderStatisticsDto OrderStatistics { get; set; }
            public DateTime LastUpdated { get; set; }
        }

        // ==================== LISTING STATISTICS ====================

        public class ListingStatisticsDto
        {
            public int TotalAnimalListings { get; set; }
            public int TotalAccessoryListings { get; set; }
            public int TotalPharmacyListings { get; set; }
            public Dictionary<string, int> AnimalListingsByType { get; set; }
            public Dictionary<string, int> AccessoriesByCategory { get; set; }
        }

        // ==================== HEALTHCARE STATISTICS ====================

        public class HealthcareStatisticsDto
        {
            public int ApprovedDoctors { get; set; }
            public int PendingDoctorApplications { get; set; }
            public int RejectedDoctorApplications { get; set; }
            public int ApprovedPharmacies { get; set; }
            public int PendingPharmacyApplications { get; set; }
            public int RejectedPharmacyApplications { get; set; }
            public decimal AverageDoctorRating { get; set; }
            public decimal AveragePharmacyRating { get; set; }
        }
    }
}