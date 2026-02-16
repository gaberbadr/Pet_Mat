using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Dtos.Pharmacy;
using CoreLayer.Entities.Pharmacies;

namespace CoreLayer.Specifications.Pharmacy
{
    // ==================== PHARMACY LISTING SPECIFICATIONS ====================

    public class PharmacyListingByOwnerSpecification : BaseSpecifications<PharmacyListing, int>
    {
        public PharmacyListingByOwnerSpecification(string pharmacyId, PharmacyListingFilterParams filterParams)
            : base(BuildCriteria(pharmacyId, filterParams))
        {
            Includes.Add(pl => pl.Pharmacy);
            Includes.Add(pl => pl.Species);

            OrderByDescending = pl => pl.CreatedAt;

            applyPagnation(
                skip: (filterParams.PageIndex - 1) * filterParams.PageSize,
                take: filterParams.PageSize
            );
        }

        private static Expression<Func<PharmacyListing, bool>> BuildCriteria(string pharmacyId, PharmacyListingFilterParams filterParams)
        {
            var categoryEnum = filterParams.GetCategoryEnum();

            return pl =>
                pl.PharmacyId == pharmacyId &&
                (!filterParams.SpeciesId.HasValue || pl.SpeciesId == filterParams.SpeciesId.Value) &&
                (!categoryEnum.HasValue || pl.Category == categoryEnum.Value) &&
                (!filterParams.MinPrice.HasValue || pl.Price >= filterParams.MinPrice.Value) &&
                (!filterParams.MaxPrice.HasValue || pl.Price <= filterParams.MaxPrice.Value) &&
                (!filterParams.InStock.HasValue || (filterParams.InStock.Value ? pl.Stock > 0 : pl.Stock == 0)) &&
                (string.IsNullOrEmpty(filterParams.Search) ||
                    pl.Title.ToLower().Contains(filterParams.Search.ToLower()) ||
                    pl.Description.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    public class PharmacyListingByOwnerCountSpecification : BaseSpecifications<PharmacyListing, int>
    {
        public PharmacyListingByOwnerCountSpecification(string pharmacyId, PharmacyListingFilterParams filterParams)
            : base(BuildCriteria(pharmacyId, filterParams))
        {
        }

        private static Expression<Func<PharmacyListing, bool>> BuildCriteria(string pharmacyId, PharmacyListingFilterParams filterParams)
        {
            var categoryEnum = filterParams.GetCategoryEnum();
            return pl =>
                pl.PharmacyId == pharmacyId &&
                (!filterParams.SpeciesId.HasValue || pl.SpeciesId == filterParams.SpeciesId.Value) &&
                (!categoryEnum.HasValue || pl.Category == categoryEnum.Value) &&
                (!filterParams.MinPrice.HasValue || pl.Price >= filterParams.MinPrice.Value) &&
                (!filterParams.MaxPrice.HasValue || pl.Price <= filterParams.MaxPrice.Value) &&
                (!filterParams.InStock.HasValue || (filterParams.InStock.Value ? pl.Stock > 0 : pl.Stock == 0)) &&
                (string.IsNullOrEmpty(filterParams.Search) ||
                    pl.Title.ToLower().Contains(filterParams.Search.ToLower()) ||
                    pl.Description.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    public class PharmacyListingByIdSpecification : BaseSpecifications<PharmacyListing, int>
    {
        public PharmacyListingByIdSpecification(int id)
            : base(pl => pl.Id == id)
        {
            Includes.Add(pl => pl.Pharmacy);
            Includes.Add(pl => pl.Species);
        }
    }

    public class PharmacyListingFilterSpecification : BaseSpecifications<PharmacyListing, int>
    {
        public PharmacyListingFilterSpecification(PharmacyListingFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
            Includes.Add(pl => pl.Pharmacy);
            Includes.Add(pl => pl.Pharmacy.Address);
            Includes.Add(pl => pl.Species);

            OrderByDescending = pl => pl.CreatedAt;

            applyPagnation(
                skip: (filterParams.PageIndex - 1) * filterParams.PageSize,
                take: filterParams.PageSize
            );
        }

        private static Expression<Func<PharmacyListing, bool>> BuildCriteria(PharmacyListingFilterParams filterParams)
        {
            var categoryEnum = filterParams.GetCategoryEnum();
            return pl =>
                pl.IsActive == true &&
                (!filterParams.SpeciesId.HasValue || pl.SpeciesId == filterParams.SpeciesId.Value) &&
                (!categoryEnum.HasValue || pl.Category == categoryEnum.Value) &&
                (!filterParams.MinPrice.HasValue || pl.Price >= filterParams.MinPrice.Value) &&
                (!filterParams.MaxPrice.HasValue || pl.Price <= filterParams.MaxPrice.Value) &&
                (!filterParams.InStock.HasValue || (filterParams.InStock.Value ? pl.Stock > 0 : pl.Stock == 0)) &&
                (string.IsNullOrEmpty(filterParams.Search) ||
                    pl.Title.ToLower().Contains(filterParams.Search.ToLower()) ||
                    pl.Description.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    public class PharmacyListingCountSpecification : BaseSpecifications<PharmacyListing, int>
    {
        public PharmacyListingCountSpecification(PharmacyListingFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
        }

        private static Expression<Func<PharmacyListing, bool>> BuildCriteria(PharmacyListingFilterParams filterParams)
        {
            var categoryEnum = filterParams.GetCategoryEnum();
            return pl =>
                pl.IsActive == true &&
                (!filterParams.SpeciesId.HasValue || pl.SpeciesId == filterParams.SpeciesId.Value) &&
                (!categoryEnum.HasValue || pl.Category == categoryEnum.Value) &&
                (!filterParams.MinPrice.HasValue || pl.Price >= filterParams.MinPrice.Value) &&
                (!filterParams.MaxPrice.HasValue || pl.Price <= filterParams.MaxPrice.Value) &&
                (!filterParams.InStock.HasValue || (filterParams.InStock.Value ? pl.Stock > 0 : pl.Stock == 0)) &&
                (string.IsNullOrEmpty(filterParams.Search) ||
                    pl.Title.ToLower().Contains(filterParams.Search.ToLower()) ||
                    pl.Description.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    // ==================== PHARMACY PROFILE SPECIFICATIONS ====================

    public class PharmacyFilterSpecification : BaseSpecifications<PharmacyProfile, Guid>
    {
        public PharmacyFilterSpecification(PharmacyFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
            Includes.Add(pp => pp.User);
            Includes.Add(pp => pp.User.Address);
            Includes.Add(pp => pp.Ratings);

            OrderByDescending = pp => pp.AverageRating;

            applyPagnation(
                skip: (filterParams.PageIndex - 1) * filterParams.PageSize,
                take: filterParams.PageSize
            );
        }

        private static Expression<Func<PharmacyProfile, bool>> BuildCriteria(PharmacyFilterParams filterParams)
        {
            return pp =>
                pp.IsActive == true &&
                (string.IsNullOrEmpty(filterParams.City) ||
                    pp.User.Address.City.ToLower() == filterParams.City.ToLower()) &&
                (string.IsNullOrEmpty(filterParams.Government) ||
                    pp.User.Address.Government.ToLower() == filterParams.Government.ToLower()) &&
                (string.IsNullOrEmpty(filterParams.Specialization) ||
                    pp.Specializations.ToLower().Contains(filterParams.Specialization.ToLower())) &&
                (string.IsNullOrEmpty(filterParams.Search) ||
                    pp.PharmacyName.ToLower().Contains(filterParams.Search.ToLower()) ||
                    pp.Description.ToLower().Contains(filterParams.Search.ToLower()) ||
                    pp.Address.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    public class PharmacyFilterCountSpecification : BaseSpecifications<PharmacyProfile, Guid>
    {
        public PharmacyFilterCountSpecification(PharmacyFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
        }

        private static Expression<Func<PharmacyProfile, bool>> BuildCriteria(PharmacyFilterParams filterParams)
        {
            return pp =>
                pp.IsActive == true &&
                (string.IsNullOrEmpty(filterParams.City) ||
                    pp.User.Address.City.ToLower() == filterParams.City.ToLower()) &&
                (string.IsNullOrEmpty(filterParams.Government) ||
                    pp.User.Address.Government.ToLower() == filterParams.Government.ToLower()) &&
                (string.IsNullOrEmpty(filterParams.Specialization) ||
                    pp.Specializations.ToLower().Contains(filterParams.Specialization.ToLower())) &&
                (string.IsNullOrEmpty(filterParams.Search) ||
                    pp.PharmacyName.ToLower().Contains(filterParams.Search.ToLower()) ||
                    pp.Description.ToLower().Contains(filterParams.Search.ToLower()) ||
                    pp.Address.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    public class PharmacyByUserIdSpecification : BaseSpecifications<PharmacyProfile, Guid>
    {
        public PharmacyByUserIdSpecification(string userId)
            : base(pp => pp.UserId == userId && pp.IsActive == true)
        {
            Includes.Add(pp => pp.User);
            Includes.Add(pp => pp.User.Address);
            Includes.Add(pp => pp.Ratings);
        }
    }
}
