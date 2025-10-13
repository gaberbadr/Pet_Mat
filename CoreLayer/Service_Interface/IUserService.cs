using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Dtos.User;
using CoreLayer.Helper.Pagination;

namespace CoreLayer.Service_Interface
{
    public interface IUserService
    {
        // ==================== ANIMAL MANAGEMENT ====================


        Task<AnimalListDto> GetMyAnimalsAsync(string userId);


        Task<AnimalOperationResponseDto> AddAnimalAsync(AddAnimalDto dto, string userId);


        Task<AnimalOperationResponseDto> UpdateAnimalAsync(int id, UpdateAnimalDto dto, string userId);


        Task<AnimalOperationResponseDto> DeleteAnimalAsync(int id, string userId);

        // ==================== ANIMAL LISTINGS ====================

        Task<PaginationResponse<AnimalListingResponseDto>> GetAllListingsAsync(
            AnimalListingFilterParams filterParams);

        Task<AnimalListingResponseDto> GetListingByIdAsync(int id);
        Task<AnimalListingListDto> GetMyListingsAsync(string userId);


        Task<ListingOperationResponseDto> AddAnimalListingAsync(AddAnimalListingDto dto, string userId);


        Task<ListingOperationResponseDto> DeleteAnimalListingAsync(int id, string userId);

        // ==================== SPECIES INFO ====================

        Task<SpeciesListDto> GetAllSpeciesAsync();


        Task<SubSpeciesListDto> GetAllSubSpeciesAsync();


        Task<SubSpeciesListDto> GetSubSpeciesBySpeciesIdAsync(int speciesId);


        Task<ColorListDto> GetAllColorsAsync();

        // ==================== Doctor INFO ====================
        Task<PaginationResponse<PublicDoctorProfileDto>> GetDoctorsAsync(DoctorFilterParams filterParams);

        Task<DoctorApplicationOperationResponseDto> ApplyToBeDoctorAsync(ApplyDoctorDto dto, string userId);
        Task<UserDoctorApplicationStatusDto> GetDoctorApplicationStatusAsync(string userId);


        Task<RatingOperationResponseDto> RateDoctorAsync(string doctorId, RateDoctorDto dto, string userId);
        Task<RatingOperationResponseDto> UpdateDoctorRatingAsync(string doctorId, RateDoctorDto dto, string userId);
        Task<PublicDoctorProfileDto> GetPublicDoctorProfileAsync(string doctorId);
    }
}
