using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Core;
using CoreLayer;
using CoreLayer.Dtos.User;
using CoreLayer.Entities.Animals;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface;
using CoreLayer.Specifications.User;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
        }

        // ==================== ANIMAL MANAGEMENT ====================

        public async Task<AnimalListDto> GetMyAnimalsAsync(string userId)
        {
            var spec = new AnimalWithDetailsByOwnerSpecification(userId);
            var animalDtos = await _unitOfWork.Repository<Animal, int>()
                .GetAllWithProjectionAsync<AnimalDto>(spec, _mapper.ConfigurationProvider);

            return new AnimalListDto
            {
                Count = animalDtos.Count(),
                Data = animalDtos
            };
        }

        public async Task<AnimalOperationResponseDto> AddAnimalAsync(AddAnimalDto dto, string userId)
        {
            // Validate species
            var species = await _unitOfWork.Repository<Species, int>().GetAsync(dto.SpeciesId);
            if (species == null || species.IsActive==false)
                throw new KeyNotFoundException("Species not found or inactive");

            // Validate subspecies if provided
            if (dto.SubSpeciesId.HasValue)
            {
                var subSpecies = await _unitOfWork.Repository<SubSpecies, int>().GetAsync(dto.SubSpeciesId.Value);
                if (subSpecies == null || subSpecies.SpeciesId != dto.SpeciesId || subSpecies.IsActive == false)
                    throw new InvalidOperationException("Invalid SubSpecies for this Species or SubSpecies is inactive");
            }

            // Validate color if provided
            if (dto.ColorId.HasValue)
            {
                var color = await _unitOfWork.Repository<Color, int>().GetAsync(dto.ColorId.Value);
                if (color == null || color.IsActive == false)
                    throw new KeyNotFoundException("Color not found or inactive");
            }

            // Handle image upload
            string imageUrl = null;
            if (dto.Image != null)
            {
                var fileName = DocumentSetting.Upload(dto.Image, "animals");
                imageUrl = $"animals/{fileName}";
            }

            // Create animal
            var animal = new Animal
            {
                PetName = dto.PetName,
                SpeciesId = dto.SpeciesId,
                SubSpeciesId = dto.SubSpeciesId,
                ColorId = dto.ColorId,
                Age = dto.Age,
                Size = dto.Size,
                Gender = dto.Gender,
                Description = dto.Description,
                ImageUrl = imageUrl,
                OwnerId = userId,
                ExtraPropertiesJson = dto.ExtraPropertiesJson,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<Animal, int>().AddAsync(animal);
            await _unitOfWork.CompleteAsync();

            return new AnimalOperationResponseDto
            {
                Success = true,
                Message = "Animal added successfully",
                AnimalId = animal.Id
            };
        }

        public async Task<AnimalOperationResponseDto> UpdateAnimalAsync(int id, UpdateAnimalDto dto, string userId)
        {
            var animal = await _unitOfWork.Repository<Animal, int>().GetAsync(id);
            if (animal == null || animal.IsActive == false)
                throw new KeyNotFoundException("Animal not found or inactive");

            if (animal.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to update this animal");

            // Validate species if provided
            if (dto.SpeciesId.HasValue)
            {
                var species = await _unitOfWork.Repository<Species, int>().GetAsync(dto.SpeciesId.Value);
                if (species == null || species.IsActive == false)
                    throw new KeyNotFoundException("Species not found or inactive");
                animal.SpeciesId = dto.SpeciesId.Value;
            }

            // Validate subspecies if provided
            if (dto.SubSpeciesId.HasValue)
            {
                var subSpecies = await _unitOfWork.Repository<SubSpecies, int>().GetAsync(dto.SubSpeciesId.Value);
                if (subSpecies == null || subSpecies.SpeciesId != animal.SpeciesId || subSpecies.IsActive==false )
                    throw new InvalidOperationException("Invalid SubSpecies for this Species or SubSpecies is inactive");
                animal.SubSpeciesId = dto.SubSpeciesId;
            }

            // Validate color if provided
            if (dto.ColorId.HasValue)
            {
                var color = await _unitOfWork.Repository<Color, int>().GetAsync(dto.ColorId.Value);
                if (color == null || color.IsActive==false)
                    throw new KeyNotFoundException("Color not found or inactive");
                animal.ColorId = dto.ColorId;
            }

            // Handle image update
            if (dto.Image != null)
            {
                if (!string.IsNullOrEmpty(animal.ImageUrl))
                {
                    DocumentSetting.Delete(animal.ImageUrl, "animals");
                }
                animal.ImageUrl = DocumentSetting.Upload(dto.Image, "animals");
            }

            // Update other fields
            if (!string.IsNullOrEmpty(dto.PetName)) animal.PetName = dto.PetName;
            if (!string.IsNullOrEmpty(dto.Age)) animal.Age = dto.Age;
            if (!string.IsNullOrEmpty(dto.Size)) animal.Size = dto.Size;
            if (!string.IsNullOrEmpty(dto.Gender)) animal.Gender = dto.Gender;
            if (!string.IsNullOrEmpty(dto.Description)) animal.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.ExtraPropertiesJson)) animal.ExtraPropertiesJson = dto.ExtraPropertiesJson;

            _unitOfWork.Repository<Animal, int>().Update(animal);
            await _unitOfWork.CompleteAsync();

            return new AnimalOperationResponseDto
            {
                Success = true,
                Message = "Animal updated successfully",
                AnimalId = animal.Id
            };
        }

        public async Task<AnimalOperationResponseDto> DeleteAnimalAsync(int id, string userId)
        {
            var animal = await _unitOfWork.Repository<Animal, int>().GetAsync(id);
            if (animal == null || animal.IsActive == false)
                throw new KeyNotFoundException("Animal not found or already deleted");

            if (animal.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this animal");

            // Check for active listings
            var activeListings = await _unitOfWork.Repository<AnimalListing, int>()
                .FindAsync(al => al.AnimalId == id);


            if (activeListings.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete animal. It has {activeListings.Count()} active listing(s). " +
                    "Please delete the listings first.");
            }

            // Soft delete
            animal.IsActive = false;
            _unitOfWork.Repository<Animal, int>().Update(animal);
            await _unitOfWork.CompleteAsync();

            return new AnimalOperationResponseDto
            {
                Success = true,
                Message = "Animal deleted successfully",
                AnimalId = id
            };
        }

        // ==================== ANIMAL LISTINGS ====================

        public async Task<PaginationResponse<AnimalListingResponseDto>> GetAllListingsAsync(AnimalListingFilterParams filterParams)
        {
            if (filterParams.PageIndex < 1)
                throw new ArgumentException("PageIndex must be greater than 0");

            if (filterParams.PageSize < 1)
                throw new ArgumentException("PageSize must be greater than 0");

            var spec = new AnimalListingFilterSpecification(filterParams);
            var countSpec = new AnimalListingCountSpecification(filterParams);

            var listingDtos = await _unitOfWork.Repository<AnimalListing, int>()
                .GetAllWithProjectionAsync<AnimalListingResponseDto>(spec, _mapper.ConfigurationProvider);

            var totalCount = await _unitOfWork.Repository<AnimalListing, int>()
                .GetCountAsync(countSpec);

            return new PaginationResponse<AnimalListingResponseDto>(
                filterParams.PageSize,
                filterParams.PageIndex,
                totalCount,
                listingDtos
            );
        }

        public async Task<AnimalListingListDto> GetMyListingsAsync(string userId)
        {
            var spec = new AnimalListingWithDetailsByOwnerSpecification(userId);
            var listingDtos = await _unitOfWork.Repository<AnimalListing, int>()
                .GetAllWithProjectionAsync<AnimalListingResponseDto>(spec, _mapper.ConfigurationProvider);

            return new AnimalListingListDto
            {
                Count = listingDtos.Count(),
                Data = listingDtos
            };
        }

        public async Task<ListingOperationResponseDto> AddAnimalListingAsync(AddAnimalListingDto dto, string userId)
        {
            // Validate animal exists and is active
            var animal = await _unitOfWork.Repository<Animal, int>().GetAsync(dto.AnimalId);
            if (animal == null || animal.IsActive == false)
                throw new KeyNotFoundException("Animal not found or inactive");

            if (animal.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to list this animal");

            var listing = new AnimalListing
            {
                Title = dto.Title,
                Description = dto.Description,
                Price = dto.Price,
                Type = dto.Type,
                AnimalId = dto.AnimalId,
                OwnerId = userId,
                ExtraPropertiesJson = dto.ExtraPropertiesJson,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<AnimalListing, int>().AddAsync(listing);
            await _unitOfWork.CompleteAsync();

            return new ListingOperationResponseDto
            {
                Success = true,
                Message = "Animal listing created successfully",
                ListingId = listing.Id
            };
        }

        public async Task<ListingOperationResponseDto> DeleteAnimalListingAsync(int id, string userId)
        {
            var listing = await _unitOfWork.Repository<AnimalListing, int>().GetAsync(id);
            if (listing == null)
                throw new KeyNotFoundException("Listing not found or already deleted");

            if (listing.OwnerId != userId)
                throw new UnauthorizedAccessException("You don't have permission to delete this listing");

            //Hard delete (actual removal)
            _unitOfWork.Repository<AnimalListing, int>().Delete(listing);
            await _unitOfWork.CompleteAsync();

            return new ListingOperationResponseDto
            {
                Success = true,
                Message = "Listing deleted successfully",
                ListingId = id
            };
        }

        // ==================== SPECIES INFO ====================

        public async Task<SpeciesListDto> GetAllSpeciesAsync()
        {
            // Only active species due to global filter
            var speciesList = await _unitOfWork.Repository<Species, int>().FindAsync(s=>s.IsActive==true);
            var speciesDtos = _mapper.Map<IEnumerable<SpeciesInfoDto>>(speciesList);

            return new SpeciesListDto
            {
                Count = speciesDtos.Count(),
                Data = speciesDtos
            };
        }

        public async Task<SubSpeciesListDto> GetAllSubSpeciesAsync()
        {
            // Only active subspecies due to global filter
            var subSpeciesList = await _unitOfWork.Repository<SubSpecies, int>().FindAsync(s => s.IsActive == true);
            var subSpeciesDtos = _mapper.Map<IEnumerable<SubSpeciesInfoDto>>(subSpeciesList);

            return new SubSpeciesListDto
            {
                Count = subSpeciesDtos.Count(),
                Data = subSpeciesDtos
            };
        }

        public async Task<SubSpeciesListDto> GetSubSpeciesBySpeciesIdAsync(int speciesId)
        {
            var species = await _unitOfWork.Repository<Species, int>().GetAsync(speciesId);
            if (species == null || species.IsActive == false)
                throw new KeyNotFoundException("Species not found or inactive");
            

            // Only active subspecies due to global filter
            var subSpecies = await _unitOfWork.Repository<SubSpecies, int>()
                .FindAsync(ss => ss.SpeciesId == speciesId && ss.IsActive==true);

            var subSpeciesDtos = _mapper.Map<IEnumerable<SubSpeciesInfoDto>>(subSpecies);

            return new SubSpeciesListDto
            {
                Count = subSpeciesDtos.Count(),
                Data = subSpeciesDtos
            };
        }

        public async Task<ColorListDto> GetAllColorsAsync()
        {
            // Only active colors due to global filter
            var colors = await _unitOfWork.Repository<Color, int>().FindAsync(c=>c.IsActive==true);
            var colorDtos = _mapper.Map<IEnumerable<ColorInfoDto>>(colors);

            return new ColorListDto
            {
                Count = colorDtos.Count(),
                Data = colorDtos
            };
        }
    }
}
