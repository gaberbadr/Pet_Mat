using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Accessory;
using CoreLayer.Entities.Accessories;
using CoreLayer.Enums;

namespace CoreLayer.Specifications.Accessory
{
    public class AccessoryListingFilterSpecification : BaseSpecifications<AccessoryListing, int>
    {
        public AccessoryListingFilterSpecification(AccessoryListingFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
            Includes.Add(al => al.Owner);
            Includes.Add(al => al.Owner.Address);
            Includes.Add(al => al.Species);

            OrderByDescending = al => al.CreatedAt;

            // Apply pagination
            applyPagnation(
                skip: (filterParams.PageIndex - 1) * filterParams.PageSize,
                take: filterParams.PageSize
            );
        }

        private static Expression<Func<AccessoryListing, bool>> BuildCriteria(AccessoryListingFilterParams filterParams)
        {
            var categoryEnum = filterParams.GetCategoryEnum();
            var conditionEnum = filterParams.GetConditionEnum();
            var statusEnum = filterParams.GetListingStatusEnum();

            return al =>
                al.IsActive == true &&
                (!filterParams.SpeciesId.HasValue || al.SpeciesId == filterParams.SpeciesId.Value) &&
                (!categoryEnum.HasValue || al.Category == categoryEnum.Value) &&
                (!conditionEnum.HasValue || al.Condition == conditionEnum.Value) &&
                (!statusEnum.HasValue || al.Status == statusEnum.Value) &&
                (!filterParams.MinPrice.HasValue || al.Price >= filterParams.MinPrice.Value) &&
                (!filterParams.MaxPrice.HasValue || al.Price <= filterParams.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filterParams.City) || al.Owner.Address.City.ToLower() == filterParams.City.ToLower()) &&
                (string.IsNullOrEmpty(filterParams.Government) || al.Owner.Address.Government.ToLower() == filterParams.Government.ToLower()) &&
                (string.IsNullOrEmpty(filterParams.Search) ||
                    al.Title.ToLower().Contains(filterParams.Search.ToLower()) ||
                    al.Description.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    public class AccessoryListingCountSpecification : BaseSpecifications<AccessoryListing, int>
    {
        public AccessoryListingCountSpecification(AccessoryListingFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
            // No includes needed for count
        }

        private static Expression<Func<AccessoryListing, bool>> BuildCriteria(AccessoryListingFilterParams filterParams)
        {
            var categoryEnum = filterParams.GetCategoryEnum();
            var conditionEnum = filterParams.GetConditionEnum();
            var statusEnum = filterParams.GetListingStatusEnum();

            return al =>
                al.IsActive == true &&
                (!filterParams.SpeciesId.HasValue || al.SpeciesId == filterParams.SpeciesId.Value) &&
                (!categoryEnum.HasValue || al.Category == categoryEnum.Value) &&
                (!conditionEnum.HasValue || al.Condition == conditionEnum.Value) &&
                (!statusEnum.HasValue || al.Status == statusEnum.Value) &&
                (!filterParams.MinPrice.HasValue || al.Price >= filterParams.MinPrice.Value) &&
                (!filterParams.MaxPrice.HasValue || al.Price <= filterParams.MaxPrice.Value) &&
                (string.IsNullOrEmpty(filterParams.City) || al.Owner.Address.City.ToLower() == filterParams.City.ToLower()) &&
                (string.IsNullOrEmpty(filterParams.Government) || al.Owner.Address.Government.ToLower() == filterParams.Government.ToLower()) &&
                (string.IsNullOrEmpty(filterParams.Search) ||
                    al.Title.ToLower().Contains(filterParams.Search.ToLower()) ||
                    al.Description.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    public class AccessoryListingByIdSpecification : BaseSpecifications<AccessoryListing, int>
    {
        public AccessoryListingByIdSpecification(int id)
            : base(al => al.Id == id && al.IsActive == true)
        {
            Includes.Add(al => al.Owner);
            Includes.Add(al => al.Owner.Address);
            Includes.Add(al => al.Species);
        }
    }

    public class AccessoryListingWithDetailsByOwnerSpecification : BaseSpecifications<AccessoryListing, int>
    {
        public AccessoryListingWithDetailsByOwnerSpecification(string ownerId)
            : base(al => al.OwnerId == ownerId && al.IsActive == true)
        {
            Includes.Add(al => al.Owner);
            Includes.Add(al => al.Owner.Address);
            Includes.Add(al => al.Species);

            OrderByDescending = al => al.CreatedAt;
        }
    }
}
