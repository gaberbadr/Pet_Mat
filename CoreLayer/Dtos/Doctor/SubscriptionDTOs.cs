using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CoreLayer.Enums;

namespace CoreLayer.Dtos.Doctor
{
    // ==================== SUBSCRIPTION PACKAGE DTOs ====================

    public class PackageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public bool IsActive { get; set; }
        public List<string> Features { get; set; } = new();
    }

    public class CreatePackageDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(1, 365)]
        public int DurationInDays { get; set; }

        public List<string> Features { get; set; } = new();
    }

    public class UpdatePackageDto
    {
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal? Price { get; set; }

        [Range(1, 365)]
        public int? DurationInDays { get; set; }

        public bool? IsActive { get; set; }

        public List<string> Features { get; set; }
    }

    // ==================== SUBSCRIPTION DTOs ====================

    public class CreateSubscriptionDto
    {
        [Required]
        public int PackageId { get; set; }
    }

    public class UpdateSubscriptionDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int PackageId { get; set; }

        [Required]
        public decimal AmountPaid { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string Status { get; set; } // "Active" or "Pending"
    }

    // ==================== SUBSCRIPTION OUTPUT DTOs ====================

    public class SubscriptionDto
    {
        public int Id { get; set; }
        public Guid DoctorId { get; set; }
        public int PackageId { get; set; }
        public string PackageName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentIntentId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class SubscriptionPackageDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int DurationInDays { get; set; }
        public bool IsActive { get; set; }
        public List<string> Features { get; set; } = new();
    }

    public class SubscriptionResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public SubscriptionDto Data { get; set; }
    }
}