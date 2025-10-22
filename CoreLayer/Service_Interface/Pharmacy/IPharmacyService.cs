using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Helper.Pagination;

namespace CoreLayer.Service_Interface.Pharmacy
{
    public interface IPharmacyService
    {
        // Profile Management
        Task<PharmacyProfileResponseDto> GetPharmacyProfileAsync(string userId);
        Task<PharmacyProfileOperationResponseDto> UpdatePharmacyProfileAsync(string userId, UpdatePharmacyProfileDto dto);
        Task<PharmacyProfileOperationResponseDto> UpdatePharmacyLocationAsync(string userId, UpdatePharmacyLocationDto dto);
        Task<PharmacyProfileOperationResponseDto> DeletePharmacyAccountAsync(string userId);

        // Ratings
        Task<PharmacyRatingListDto> GetPharmacyRatingsAsync(string pharmacyId);

        // Pharmacy Listings (Products)
        Task<PaginationResponse<PharmacyListingResponseDto>> GetMyListingsAsync(string userId, PharmacyListingFilterParams filterParams);
        Task<PharmacyListingResponseDto> GetMyListingByIdAsync(int id, string userId);
        Task<PharmacyListingOperationResponseDto> AddListingAsync(AddPharmacyListingDto dto, string userId);
        Task<PharmacyListingOperationResponseDto> UpdateListingAsync(int id, UpdatePharmacyListingDto dto, string userId);
        Task<PharmacyListingOperationResponseDto> DeleteListingAsync(int id, string userId);
        Task<PharmacyListingOperationResponseDto> UpdateListingStockAsync(int id, UpdateListingStockDto dto, string userId);
    }
}
