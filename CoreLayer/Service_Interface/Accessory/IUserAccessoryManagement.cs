using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Accessory;
using CoreLayer.Enums;
using CoreLayer.Helper.Pagination;

namespace CoreLayer.Service_Interface.Accessory
{
    public interface IUserAccessoryManagement
    {
        Task<PaginationResponse<AccessoryListingResponseDto>> GetAllAccessoryListingsAsync(AccessoryListingFilterParams filterParams);
        Task<AccessoryListingResponseDto> GetAccessoryListingByIdAsync(int id);
        Task<AccessoryListingListDto> GetMyAccessoryListingsAsync(string userId);
        Task<AccessoryOperationResponseDto> AddAccessoryListingAsync(AddAccessoryListingDto dto, string userId);
        Task<AccessoryOperationResponseDto> UpdateAccessoryListingAsync(int id, UpdateAccessoryListingDto dto, string userId);
        Task<AccessoryOperationResponseDto> UpdateAccessoryListingStatusAsync(int listingId, string userId, ListingStatus newStatus);
        Task<AccessoryOperationResponseDto> DeleteAccessoryListingAsync(int id, string userId);
    }
}
