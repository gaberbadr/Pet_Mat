using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.User;
using CoreLayer.Entities.Animals;
using CoreLayer.Enums;


namespace CoreLayer.Specifications.User
{


    // ==================== ANIMAL SPECIFICATIONS ====================

    public class AnimalWithDetailsByOwnerSpecification : BaseSpecifications<Animal, int>
    {
        public AnimalWithDetailsByOwnerSpecification(string ownerId)
            : base(a => a.OwnerId == ownerId && a.IsActive)
        {
            Includes.Add(a => a.Species);
            Includes.Add(a => a.SubSpecies);
            Includes.Add(a => a.Color);

            OrderByDescending = a => a.CreatedAt;
        }
    }

    // ==================== ANIMAL LISTING SPECIFICATIONS ====================

    public class AnimalListingFilterSpecification : BaseSpecifications<AnimalListing, int>
    {
        public AnimalListingFilterSpecification(AnimalListingFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
            Includes.Add(al => al.Animal);
            Includes.Add(al => al.Animal.Species);
            Includes.Add(al => al.Animal.SubSpecies);
            Includes.Add(al => al.Animal.Color);
            Includes.Add(al => al.Owner);
            Includes.Add(al => al.Owner.Address);

            OrderByDescending = al => al.CreatedAt;

            // Apply pagination
            applyPagnation(
                skip: (filterParams.PageIndex - 1) * filterParams.PageSize,
                take: filterParams.PageSize
            );
        }

        private static Expression<Func<AnimalListing, bool>> BuildCriteria(AnimalListingFilterParams filterParams)
        {
            var typeEnum = filterParams.GetAnimalListingTypeEnum();
            var statusEnum = filterParams.GetListingStatusEnum();

            return al =>
         al.IsActive == true &&
       (!filterParams.SpeciesId.HasValue || al.Animal.SpeciesId == filterParams.SpeciesId.Value) &&
       (!typeEnum.HasValue || al.Type == typeEnum.Value) &&
       (!statusEnum.HasValue || al.Status == statusEnum.Value) &&
       (!filterParams.MinPrice.HasValue || al.Price >= filterParams.MinPrice.Value) &&
       (!filterParams.MaxPrice.HasValue || al.Price <= filterParams.MaxPrice.Value) &&

                 // City and Government filters
                 (string.IsNullOrEmpty(filterParams.City) || al.Owner.Address.City.ToLower() == filterParams.City.ToLower()) &&
                 (string.IsNullOrEmpty(filterParams.Government) || al.Owner.Address.Government.ToLower() == filterParams.Government.ToLower()) &&

                 // Search filter
                 (string.IsNullOrEmpty(filterParams.Search) ||
                     al.Title.ToLower().Contains(filterParams.Search.ToLower()) ||
                     al.Description.ToLower().Contains(filterParams.Search.ToLower()) ||
                     al.Animal.PetName.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    public class AnimalListingCountSpecification : BaseSpecifications<AnimalListing, int>
    {
        public AnimalListingCountSpecification(AnimalListingFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
            // No includes needed for count
        }

        private static Expression<Func<AnimalListing, bool>> BuildCriteria(AnimalListingFilterParams filterParams)
        {   
            var typeEnum = filterParams.GetAnimalListingTypeEnum();
            var statusEnum = filterParams.GetListingStatusEnum();

            return al =>
         al.IsActive == true &&
       (!filterParams.SpeciesId.HasValue || al.Animal.SpeciesId == filterParams.SpeciesId.Value) &&
       (!typeEnum.HasValue || al.Type == typeEnum.Value) &&
       (!statusEnum.HasValue || al.Status == statusEnum.Value) &&
       (!filterParams.MinPrice.HasValue || al.Price >= filterParams.MinPrice.Value) &&
       (!filterParams.MaxPrice.HasValue || al.Price <= filterParams.MaxPrice.Value) &&

                 // City and Government filters
                 (string.IsNullOrEmpty(filterParams.City) || al.Owner.Address.City.ToLower() == filterParams.City.ToLower()) &&
                 (string.IsNullOrEmpty(filterParams.Government) || al.Owner.Address.Government.ToLower() == filterParams.Government.ToLower()) &&

                 // Search filter
                 (string.IsNullOrEmpty(filterParams.Search) ||
                     al.Title.ToLower().Contains(filterParams.Search.ToLower()) ||
                     al.Description.ToLower().Contains(filterParams.Search.ToLower()) ||
                     al.Animal.PetName.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    public class AnimalListingByIdSpecification : BaseSpecifications<AnimalListing, int>
    {
        public AnimalListingByIdSpecification(int id)
            : base(al => al.Id == id && al.IsActive == true)
        {
            Includes.Add(al => al.Animal);
            Includes.Add(al => al.Animal.Species);
            Includes.Add(al => al.Animal.SubSpecies);
            Includes.Add(al => al.Animal.Color);
            Includes.Add(al => al.Owner);
            Includes.Add(al => al.Owner.Address);
        }
    }
    public class AnimalListingWithDetailsByOwnerSpecification : BaseSpecifications<AnimalListing, int>
    {
        public AnimalListingWithDetailsByOwnerSpecification(string ownerId)
            : base(al => al.OwnerId == ownerId && al.IsActive == true)
        {
            Includes.Add(al => al.Animal);
            Includes.Add(al => al.Animal.Species);
            Includes.Add(al => al.Animal.SubSpecies);
            Includes.Add(al => al.Animal.Color);
            Includes.Add(al => al.Owner);
            Includes.Add(al => al.Owner.Address);

            OrderByDescending = al => al.CreatedAt;
        }
    }
}