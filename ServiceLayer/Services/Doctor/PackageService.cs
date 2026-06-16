using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Entities.Doctors;
using CoreLayer.Service_Interface.Doctor;

namespace ServiceLayer.Services.Doctor
{
    public class PackageService : IPackageService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PackageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PackageDto>> GetAllPackagesAsync(bool includeInactive = false)
        {
            var packages = await _unitOfWork.Repository<SubscriptionPackage, int>().GetAllAsync();
            var filtered = includeInactive ? packages : packages.Where(p => p.IsActive);
            return filtered.Select(MapToDto);
        }

        public async Task<PackageDto> GetPackageByIdAsync(int id)
        {
            var package = await _unitOfWork.Repository<SubscriptionPackage, int>().GetAsync(id);
            if (package == null)
                throw new KeyNotFoundException($"Package with id {id} not found");
            return MapToDto(package);
        }

        public async Task<PackageDto> CreatePackageAsync(CreatePackageDto dto)
        {
            var package = new SubscriptionPackage
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                DurationInDays = dto.DurationInDays,
                IsActive = true, // ✅ Default to true for new packages
                Features = dto.Features ?? new List<string>()
            };

            await _unitOfWork.Repository<SubscriptionPackage, int>().AddAsync(package);
            await _unitOfWork.CompleteAsync();
            return MapToDto(package);
        }

        public async Task<PackageDto> UpdatePackageAsync(int id, UpdatePackageDto dto)
        {
            var package = await _unitOfWork.Repository<SubscriptionPackage, int>().GetAsync(id);
            if (package == null)
                throw new KeyNotFoundException($"Package with id {id} not found");

            // ✅ Only update properties if they were provided (not null)
            if (!string.IsNullOrEmpty(dto.Name))
                package.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Description))
                package.Description = dto.Description;

            if (dto.Price.HasValue)
                package.Price = dto.Price.Value;

            if (dto.DurationInDays.HasValue)
                package.DurationInDays = dto.DurationInDays.Value;

            if (dto.IsActive.HasValue)
                package.IsActive = dto.IsActive.Value;

            if (dto.Features != null)
                package.Features = dto.Features;

            _unitOfWork.Repository<SubscriptionPackage, int>().Update(package);
            await _unitOfWork.CompleteAsync();
            return MapToDto(package);
        }

        public async Task DeletePackageAsync(int id)
        {
            var package = await _unitOfWork.Repository<SubscriptionPackage, int>().GetAsync(id);
            if (package == null)
                throw new KeyNotFoundException($"Package with id {id} not found");

            _unitOfWork.Repository<SubscriptionPackage, int>().Delete(package);
            await _unitOfWork.CompleteAsync();
        }

        private static PackageDto MapToDto(SubscriptionPackage p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            DurationInDays = p.DurationInDays,
            IsActive = p.IsActive,
            Features = p.Features ?? new List<string>()
        };
    }
}
