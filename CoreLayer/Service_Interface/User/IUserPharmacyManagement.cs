using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Helper.Pagination;

namespace CoreLayer.Service_Interface.User
{
    public interface IUserPharmacyManagement
    {
        // Browse pharmacies
        Task<PaginationResponse<PublicPharmacyProfileDto>> GetPharmaciesAsync(PharmacyFilterParams filterParams);
        Task<PublicPharmacyProfileDto> GetPublicPharmacyProfileAsync(string pharmacyId);

        // Browse pharmacy listings
        Task<PaginationResponse<PharmacyListingResponseDto>> GetAllPharmacyListingsAsync(PharmacyListingFilterParams filterParams);
        Task<PharmacyListingResponseDto> GetPharmacyListingByIdAsync(int id);
        Task<PaginationResponse<PharmacyListingResponseDto>> GetListingsByPharmacyIdAsync(string pharmacyId, PharmacyListingFilterParams filterParams);

        // Pharmacy Application (User)
        Task<PharmacyApplicationOperationResponseDto> ApplyToBePharmacyAsync(ApplyPharmacyDto dto, string userId);
        Task<UserPharmacyApplicationStatusDto> GetPharmacyApplicationStatusAsync(string userId);

        // Ratings (User)
        Task<RatingOperationResponseDto> RatePharmacyAsync(string pharmacyId, RatePharmacyDto dto, string userId);
        Task<RatingOperationResponseDto> UpdatePharmacyRatingAsync(string pharmacyId, RatePharmacyDto dto, string userId);
    }
}

