using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using CoreLayer;
using CoreLayer.Dtos.Admin;
using CoreLayer.Entities.Accessories;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Community;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Pharmacies;
using CoreLayer.Service_Interface.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace ServiceLayer.Services.Admin
{
    public class AdminAnimalManagement : IAdminAnimalManagement
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AdminAnimalManagement(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
            _configuration = configuration;
        }


        // ==================== SPECIES MANAGEMENT ====================

        public async Task<SpeciesResponseDto> AddSpeciesAsync(SpeciesAdminDto dto)
        {

            // Check ALL species including soft-deleted ones
            var existingSpecies = await _unitOfWork.Repository<Species, int>()
                .FindAsync(s => s.Name.ToLower() == dto.Name.ToLower());

            var existingSpeciesItem = existingSpecies.FirstOrDefault();

            if (existingSpeciesItem != null)
            {
                if (!existingSpeciesItem.IsActive)
                {
                    // Reactivate soft-deleted species
                    existingSpeciesItem.IsActive = true;
                    _unitOfWork.Repository<Species, int>().Update(existingSpeciesItem);
                    await _unitOfWork.CompleteAsync();

                    return new SpeciesResponseDto
                    {
                        Id = existingSpeciesItem.Id,
                        Name = existingSpeciesItem.Name,
                        Message = "Species reactivated successfully"
                    };
                }

                throw new InvalidOperationException("Species already exists and is active");
            }

            // Create new species
            var species = new Species
            {
                Name = dto.Name,
                IsActive = true
            };

            await _unitOfWork.Repository<Species, int>().AddAsync(species);
            await _unitOfWork.CompleteAsync();

            return new SpeciesResponseDto
            {
                Id = species.Id,
                Name = species.Name,
                Message = "Species added successfully"
            };
        }

        public async Task<DeleteResponseDto> DeleteSpeciesAsync(int id)
        {
            var species = await _unitOfWork.Repository<Species, int>().GetAsync(id);
            if (species == null)
                throw new KeyNotFoundException("Species not found or already deleted");

            // Check for ACTIVE related entities only
            var activeAnimals = await _unitOfWork.Repository<Animal, int>()

                .FindAsync(a => a.SpeciesId == id);
            if (activeAnimals.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete species. It has {activeAnimals.Count()} active animal(s). " +
                    "Please delete or reassign these animals first.");
            }

            var activeListings = await _unitOfWork.Repository<AccessoryListing, int>()
                .FindAsync(al => al.SpeciesId == id);

            if (activeListings.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete species. It has {activeListings.Count()} active accessory listing(s).");
            }

            var activePharmacyListings = await _unitOfWork.Repository<PharmacyListing, int>()
                .FindAsync(pl => pl.SpeciesId == id);

            if (activePharmacyListings.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete species. It has {activePharmacyListings.Count()} active pharmacy listing(s).");
            }

            var activeProducts = await _unitOfWork.Repository<Product, int>()
                .FindAsync(p => p.SpeciesId == id);

            if (activeProducts.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete species. It has {activeProducts.Count()} active product(s).");
            }

            var activePosts = await _unitOfWork.Repository<Post, int>()
                .FindAsync(p => p.SpeciesId == id);

            if (activePosts.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete species. It has {activePosts.Count()} active post(s).");
            }

            // Soft delete
            species.IsActive = false;
            _unitOfWork.Repository<Species, int>().Update(species);
            await _unitOfWork.CompleteAsync();

            return new DeleteResponseDto
            {
                Success = true,
                Message = "Species deleted successfully",
                DeletedId = id
            };
        }

        // ==================== SUBSPECIES MANAGEMENT ====================

        public async Task<SubSpeciesResponseDto> AddSubSpeciesAsync(SubSpeciesAdminDto dto)
        {
            // Validate parent species is active
            var species = await _unitOfWork.Repository<Species, int>().GetAsync(dto.SpeciesId);
            if (species == null)
                throw new KeyNotFoundException("Species not found or inactive");

            if (!species.IsActive)
                throw new InvalidOperationException("Cannot add SubSpecies to an inactive Species");


            // Check ALL subspecies including soft-deleted ones
            var existingSubSpecies = await _unitOfWork.Repository<SubSpecies, int>()
                .FindAsync(ss => ss.Name.ToLower() == dto.Name.ToLower() && ss.SpeciesId == dto.SpeciesId);

            var existingSubSpeciesItem = existingSubSpecies.FirstOrDefault();

            if (existingSubSpeciesItem != null)
            {
                if (!existingSubSpeciesItem.IsActive)
                {
                    // Reactivate soft-deleted subspecies
                    existingSubSpeciesItem.IsActive = true;
                    _unitOfWork.Repository<SubSpecies, int>().Update(existingSubSpeciesItem);
                    await _unitOfWork.CompleteAsync();

                    return new SubSpeciesResponseDto
                    {
                        Id = existingSubSpeciesItem.Id,
                        Name = existingSubSpeciesItem.Name,
                        SpeciesId = existingSubSpeciesItem.SpeciesId,
                        SpeciesName = species.Name,
                        Message = "SubSpecies reactivated successfully"
                    };
                }

                throw new InvalidOperationException("SubSpecies already exists and is active for this species");
            }

            // Create new subspecies
            var subSpecies = new SubSpecies
            {
                Name = dto.Name,
                SpeciesId = dto.SpeciesId,
                IsActive = true
            };

            await _unitOfWork.Repository<SubSpecies, int>().AddAsync(subSpecies);
            await _unitOfWork.CompleteAsync();

            return new SubSpeciesResponseDto
            {
                Id = subSpecies.Id,
                Name = subSpecies.Name,
                SpeciesId = subSpecies.SpeciesId,
                SpeciesName = species.Name,
                Message = "SubSpecies added successfully"
            };
        }

        public async Task<DeleteResponseDto> DeleteSubSpeciesAsync(int id)
        {
            var subSpecies = await _unitOfWork.Repository<SubSpecies, int>().GetAsync(id);
            if (subSpecies == null)
                throw new KeyNotFoundException("SubSpecies not found or already deleted");

            // Check for active related animals (global filter handles IsActive check)
            var activeAnimals = await _unitOfWork.Repository<Animal, int>()
                .FindAsync(a => a.SubSpeciesId == id);

            if (activeAnimals.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete subspecies. It has {activeAnimals.Count()} active animal(s). " +
                    "Please delete or reassign these animals first.");
            }

            // Soft delete
            subSpecies.IsActive = false;
            _unitOfWork.Repository<SubSpecies, int>().Update(subSpecies);
            await _unitOfWork.CompleteAsync();

            return new DeleteResponseDto
            {
                Success = true,
                Message = "SubSpecies deleted successfully",
                DeletedId = id
            };
        }

        // ==================== COLOR MANAGEMENT ====================

        public async Task<ColorResponseDto> AddColorAsync(ColorAdminDto dto)
        {
            // Check ALL colors including soft-deleted ones
            var existingColor = await _unitOfWork.Repository<Color, int>()
                .FindAsync(c => c.Name.ToLower() == dto.Name.ToLower());

            var existingColorItem = existingColor.FirstOrDefault();

            if (existingColorItem != null)
            {
                if (!existingColorItem.IsActive)
                {
                    // Reactivate soft-deleted color
                    existingColorItem.IsActive = true;
                    _unitOfWork.Repository<Color, int>().Update(existingColorItem);
                    await _unitOfWork.CompleteAsync();

                    return new ColorResponseDto
                    {
                        Id = existingColorItem.Id,
                        Name = existingColorItem.Name,
                        Message = "Color reactivated successfully"
                    };
                }

                throw new InvalidOperationException("Color already exists and is active");
            }

            // Create new color
            var color = new Color
            {
                Name = dto.Name,
                IsActive = true
            };

            await _unitOfWork.Repository<Color, int>().AddAsync(color);
            await _unitOfWork.CompleteAsync();

            return new ColorResponseDto
            {
                Id = color.Id,
                Name = color.Name,
                Message = "Color added successfully"
            };
        }

        public async Task<DeleteResponseDto> DeleteColorAsync(int id)
        {
            var color = await _unitOfWork.Repository<Color, int>().GetAsync(id);
            if (color == null)
                throw new KeyNotFoundException("Color not found or already deleted");

            // Check for active related animals (global filter handles IsActive check)
            var activeAnimals = await _unitOfWork.Repository<Animal, int>()
                .FindAsync(a => a.ColorId == id);

            if (activeAnimals.Any())
            {
                throw new InvalidOperationException(
                    $"Cannot delete color. It has {activeAnimals.Count()} active animal(s). " +
                    "Please delete or reassign these animals first.");
            }

            // Soft delete
            color.IsActive = false;
            _unitOfWork.Repository<Color, int>().Update(color);
            await _unitOfWork.CompleteAsync();

            return new DeleteResponseDto
            {
                Success = true,
                Message = "Color deleted successfully",
                DeletedId = id
            };
        }


    }
}
