using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Azure.Core;
using CoreLayer;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Dtos.User;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Doctors;
using CoreLayer.Entities.Identity;
using CoreLayer.Helper.Documents;
using CoreLayer.Helper.Pagination;
using CoreLayer.Service_Interface;
using CoreLayer.Specifications;
using CoreLayer.Specifications.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration;
            _userManager = userManager;
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
       
        public async Task<AnimalListingResponseDto> GetListingByIdAsync(int id)
        {
            var spec = new AnimalListingByIdSpecification(id);
            var listing = await _unitOfWork.Repository<AnimalListing, int>()
                .GetWithSpecficationAsync(spec);

            if (listing == null)
                throw new KeyNotFoundException("Listing not found or inactive");

            return _mapper.Map<AnimalListingResponseDto>(listing);
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

            // Verify animal belongs to the user
            if (animal.OwnerId != userId)
                throw new UnauthorizedAccessException("You can only create listings for your own animals");

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
                IsActive = true,
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

            // Soft delete
            listing.IsActive = false;
            _unitOfWork.Repository<AnimalListing, int>().Update(listing);
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

        // ==================== DOCTOR ====================

        public async Task<PaginationResponse<PublicDoctorProfileDto>> GetDoctorsAsync(DoctorFilterParams filterParams)
        {
            // Build specifications 
            var spec = new DoctorFilterSpecification(filterParams);
            var countSpec = new DoctorFilterCountSpecification(filterParams);

            // Get total count 
            var totalCount = await _unitOfWork.Repository<DoctorProfile, Guid>().GetCountAsync(countSpec);

            // Get filtered + paginated doctors
            var doctors = await _unitOfWork.Repository<DoctorProfile, Guid>().GetAllWithSpecficationAsync(spec);

            // Map to DTOs
            var doctorDtos = doctors.Select(dp => new PublicDoctorProfileDto
            {
                Id = dp.Id,
                DoctorName = $"{dp.User.FirstName} {dp.User.LastName}",
                ProfilePicture = dp.User.ProfilePicture,
                Specialization = dp.Specialization,
                ExperienceYears = dp.ExperienceYears,
                ClinicAddress = dp.ClinicAddress,
                ClinicName = dp.ClinicName,
                AverageRating = dp.AverageRating,
                TotalRatings = dp.TotalRatings,
                City = dp.User?.Address?.City,
                Government = dp.User?.Address?.Government
            }).ToList();

            // Return paginated response
            return new PaginationResponse<PublicDoctorProfileDto>(
                filterParams.PageSize,
                filterParams.PageIndex,
                totalCount,
                doctorDtos
            );
        }

        // ==================== DOCTOR APPLICATION (USER) ====================

        public async Task<DoctorApplicationOperationResponseDto> ApplyToBeDoctorAsync(ApplyDoctorDto dto, string userId)
        {
            // Check if user already has a role
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Any())
                throw new InvalidOperationException("You already have a role and cannot apply to be a doctor");

            // Delete any existing application
            var existingApplication = (await _unitOfWork.Repository<DoctorApply, Guid>()
                .FindAsync(da => da.UserId == userId)).FirstOrDefault();

            if (existingApplication != null)
            {
                // Delete old documents
                if (!string.IsNullOrEmpty(existingApplication.NationalIdFront))
                    DocumentSetting.Delete(existingApplication.NationalIdFront, "doctor-documents");
                if (!string.IsNullOrEmpty(existingApplication.NationalIdBack))
                    DocumentSetting.Delete(existingApplication.NationalIdBack, "doctor-documents");
                if (!string.IsNullOrEmpty(existingApplication.SelfieWithId))
                    DocumentSetting.Delete(existingApplication.SelfieWithId, "doctor-documents");
                if (!string.IsNullOrEmpty(existingApplication.SyndicateCard))
                    DocumentSetting.Delete(existingApplication.SyndicateCard, "doctor-documents");
                if (!string.IsNullOrEmpty(existingApplication.MedicalLicense))
                    DocumentSetting.Delete(existingApplication.MedicalLicense, "doctor-documents");

                _unitOfWork.Repository<DoctorApply, Guid>().Delete(existingApplication);
                await _unitOfWork.CompleteAsync();
            }

            // Upload new documents
            var nationalIdFront = DocumentSetting.Upload(dto.NationalIdFront, "doctor-documents");
            var nationalIdBack = DocumentSetting.Upload(dto.NationalIdBack, "doctor-documents");
            var selfieWithId = DocumentSetting.Upload(dto.SelfieWithId, "doctor-documents");
            var syndicateCard = DocumentSetting.Upload(dto.SyndicateCard, "doctor-documents");
            var medicalLicense = DocumentSetting.Upload(dto.MedicalLicense, "doctor-documents");

            // Create new application
            var application = new DoctorApply
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Specialization = dto.Specialization,
                ExperienceYears = dto.ExperienceYears,
                ClinicAddress = dto.ClinicAddress,
                NationalIdFront = nationalIdFront,
                NationalIdBack = nationalIdBack,
                SelfieWithId = selfieWithId,
                SyndicateCard = syndicateCard,
                MedicalLicense = medicalLicense,
                Status = "Pending",
                AppliedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<DoctorApply, Guid>().AddAsync(application);
            await _unitOfWork.CompleteAsync();

            return new DoctorApplicationOperationResponseDto
            {
                Success = true,
                Message = "Doctor application submitted successfully",
                ApplicationId = application.Id
            };
        }

        public async Task<UserDoctorApplicationStatusDto> GetDoctorApplicationStatusAsync(string userId)
        {
            // Check if user already has doctor role
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            var isDoctor = await _userManager.IsInRoleAsync(user, "Doctor");
            if (isDoctor)
                throw new InvalidOperationException("You are already a doctor and cannot view application status");

            // Get application
            var application = (await _unitOfWork.Repository<DoctorApply, Guid>()
                .FindAsync(da => da.UserId == userId)).FirstOrDefault();

            if (application == null)
                throw new KeyNotFoundException("No doctor application found");

            return new UserDoctorApplicationStatusDto
            {
                ApplicationId = application.Id,
                Status = application.Status,
                AppliedAt = application.AppliedAt,
                RejectionReason = application.RejectionReason
            };
        }
        // ==================== DOCTOR RATING (USER) ====================

        public async Task<RatingOperationResponseDto> RateDoctorAsync(string doctorId, RateDoctorDto dto, string userId)
        {
            // Verify doctor exists and has doctor role
            var doctor = await _userManager.FindByIdAsync(doctorId);
            if (doctor == null)
                throw new KeyNotFoundException("Doctor not found");

            var isDoctor = await _userManager.IsInRoleAsync(doctor, "Doctor");
            if (!isDoctor)
                throw new KeyNotFoundException("User is not a doctor");

            // Check if user already rated this doctor
            var existingRating = (await _unitOfWork.Repository<DoctorRating, int>()
                .FindAsync(dr => dr.DoctorId == doctorId && dr.UserId == userId)).FirstOrDefault();

            if (existingRating != null)
                throw new InvalidOperationException("You have already rated this doctor. Use update endpoint to modify your rating.");

            // Create rating
            var rating = new DoctorRating
            {
                DoctorId = doctorId,
                UserId = userId,
                Rating = dto.Rating,
                Review = dto.Review,
                CommunicationRating = dto.CommunicationRating,
                KnowledgeRating = dto.KnowledgeRating,
                ResponsivenessRating = dto.ResponsivenessRating,
                ProfessionalismRating = dto.ProfessionalismRating,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Repository<DoctorRating, int>().AddAsync(rating);

            // Update doctor profile average rating
            var profile = (await _unitOfWork.Repository<DoctorProfile, Guid>()
                .FindAsync(dp => dp.UserId == doctorId && dp.IsActive)).FirstOrDefault();

            if (profile != null)
            {
                var allRatings = await _unitOfWork.Repository<DoctorRating, int>()
                    .FindAsync(dr => dr.DoctorId == doctorId);

                var ratingsList = allRatings.ToList();
                ratingsList.Add(rating); // Include the new rating

                profile.TotalRatings = ratingsList.Count;
                profile.AverageRating = Math.Round(ratingsList.Average(r => r.Rating), 2);

                _unitOfWork.Repository<DoctorProfile, Guid>().Update(profile);
            }

            await _unitOfWork.CompleteAsync();

            return new RatingOperationResponseDto
            {
                Success = true,
                Message = "Doctor rated successfully",
                RatingId = rating.Id
            };
        }

        public async Task<RatingOperationResponseDto> UpdateDoctorRatingAsync(string doctorId, RateDoctorDto dto, string userId)
        {
            // Get existing rating
            var rating = (await _unitOfWork.Repository<DoctorRating, int>()
                .FindAsync(dr => dr.DoctorId == doctorId && dr.UserId == userId)).FirstOrDefault();

            if (rating == null)
                throw new KeyNotFoundException("Rating not found");

            if (rating.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own ratings");

            // Update rating
            rating.Rating = dto.Rating;
            rating.Review = dto.Review;
            rating.CommunicationRating = dto.CommunicationRating;
            rating.KnowledgeRating = dto.KnowledgeRating;
            rating.ResponsivenessRating = dto.ResponsivenessRating;
            rating.ProfessionalismRating = dto.ProfessionalismRating;

            _unitOfWork.Repository<DoctorRating, int>().Update(rating);

            // Update doctor profile average rating
            var profile = (await _unitOfWork.Repository<DoctorProfile, Guid>()
                .FindAsync(dp => dp.UserId == doctorId && dp.IsActive)).FirstOrDefault();

            if (profile != null)
            {
                var allRatings = await _unitOfWork.Repository<DoctorRating, int>()
                    .FindAsync(dr => dr.DoctorId == doctorId);

                var ratingsList = allRatings.ToList();
                profile.AverageRating = Math.Round(ratingsList.Average(r => r.Rating), 2);

                _unitOfWork.Repository<DoctorProfile, Guid>().Update(profile);
            }

            await _unitOfWork.CompleteAsync();

            return new RatingOperationResponseDto
            {
                Success = true,
                Message = "Rating updated successfully",
                RatingId = rating.Id
            };
        }


        public async Task<PublicDoctorProfileDto> GetPublicDoctorProfileAsync(string doctorId)
        {
            // Verify doctor exists
            var doctor = await _userManager.FindByIdAsync(doctorId);
            if (doctor == null)
                throw new KeyNotFoundException("Doctor not found");

            var isDoctor = await _userManager.IsInRoleAsync(doctor, "Doctor");
            if (!isDoctor)
                throw new KeyNotFoundException("User is not a doctor");

            // Use specification to get doctor profile with User and Ratings included
            var spec = new DoctorByUserIdSpecification(doctorId);
            var profile = await _unitOfWork.Repository<DoctorProfile, Guid>().GetWithSpecficationAsync(spec);

            if (profile == null)
                throw new KeyNotFoundException("Doctor profile not found");

            // Get recent ratings
            var recentRatings = profile.Ratings
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .Select(r =>
                {
                    var user = _userManager.FindByIdAsync(r.UserId).Result; 
                    return new DoctorRatingDto
                    {
                        Id = r.Id,
                        UserName = user != null ? $"{user.FirstName} {user.LastName}" : "Unknown User",
                        UserProfilePicture = user?.ProfilePicture,
                        Rating = r.Rating,
                        Review = r.Review,
                        CommunicationRating = r.CommunicationRating,
                        KnowledgeRating = r.KnowledgeRating,
                        ResponsivenessRating = r.ResponsivenessRating,
                        ProfessionalismRating = r.ProfessionalismRating,
                        CreatedAt = r.CreatedAt
                    };
                }).ToList();

            return new PublicDoctorProfileDto
            {
                Id = profile.Id,
                DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                ProfilePicture = doctor.ProfilePicture,
                Latitude = (double)profile.Latitude,
                Longitude = (double)profile.Longitude,
                Specialization = profile.Specialization,
                ExperienceYears = profile.ExperienceYears,
                ClinicAddress = profile.ClinicAddress,
                ClinicName = profile.ClinicName,
                Bio = profile.Bio,
                WorkingHours = profile.WorkingHours,
                Phone = profile.Phone,
                IsAvailableForConsultation = profile.IsAvailableForConsultation,
                Services = profile.Services,
                Languages = profile.Languages,
                AverageRating = profile.AverageRating,
                TotalRatings = profile.TotalRatings,
                City = profile.User?.Address?.City,
                Government = profile.User?.Address?.Government,
                RecentRatings = recentRatings
            };
        }


    }
}
