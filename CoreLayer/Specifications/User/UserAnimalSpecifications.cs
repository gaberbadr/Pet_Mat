using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.User;
using CoreLayer.Entities.Animals;


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
            return al =>
                al.Status == "Active" &&
                (!filterParams.SpeciesId.HasValue || al.Animal.SpeciesId == filterParams.SpeciesId.Value) &&
                (string.IsNullOrEmpty(filterParams.Type) || al.Type == filterParams.Type) &&
                (string.IsNullOrEmpty(filterParams.Status) || al.Status == filterParams.Status) &&
                (!filterParams.MinPrice.HasValue || al.Price >= filterParams.MinPrice.Value) &&
                (!filterParams.MaxPrice.HasValue || al.Price <= filterParams.MaxPrice.Value) &&
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
            return al =>
                al.Status == "Active" &&
                (!filterParams.SpeciesId.HasValue || al.Animal.SpeciesId == filterParams.SpeciesId.Value) &&
                (string.IsNullOrEmpty(filterParams.Type) || al.Type == filterParams.Type) &&
                (string.IsNullOrEmpty(filterParams.Status) || al.Status == filterParams.Status) &&
                (!filterParams.MinPrice.HasValue || al.Price >= filterParams.MinPrice.Value) &&
                (!filterParams.MaxPrice.HasValue || al.Price <= filterParams.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filterParams.Search) ||
                    al.Title.ToLower().Contains(filterParams.Search.ToLower()) ||
                    al.Description.ToLower().Contains(filterParams.Search.ToLower()) ||
                    al.Animal.PetName.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }


    public class AnimalListingWithDetailsByOwnerSpecification : BaseSpecifications<AnimalListing, int>
    {
        public AnimalListingWithDetailsByOwnerSpecification(string ownerId)
            : base(al => al.OwnerId == ownerId)
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