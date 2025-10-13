using System;
using System.Linq.Expressions;
using CoreLayer.Dtos.Doctor;
using CoreLayer.Entities;
using CoreLayer.Entities.Doctors;
using CoreLayer.Entities.Identity;

namespace CoreLayer.Specifications
{
    // ==================== DOCTOR FILTER SPECIFICATION ====================

    public class DoctorFilterSpecification : BaseSpecifications<DoctorProfile, Guid>
    {
        public DoctorFilterSpecification(DoctorFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
            // Include related data
            Includes.Add(dp => dp.User);
            Includes.Add(dp => dp.User.Address);


            // Order by rating (highest first)
            OrderByDescending = dp => dp.AverageRating;

            // Apply pagination
            applyPagnation(
                skip: (filterParams.PageIndex - 1) * filterParams.PageSize,
                take: filterParams.PageSize
            );
        }

        private static Expression<Func<DoctorProfile, bool>> BuildCriteria(DoctorFilterParams filterParams)
        {
            return dp =>
                dp.IsActive == true &&
                 dp.IsActive == true &&
                (string.IsNullOrEmpty(filterParams.Specialization) || dp.Specialization.ToLower() == filterParams.Specialization.ToLower()) &&
                (!filterParams.MinExperienceYears.HasValue || dp.ExperienceYears >= filterParams.MinExperienceYears.Value) &&

                // City and Government by User Address (not ClinicAddress)
                (string.IsNullOrEmpty(filterParams.City) ||
                    dp.User.Address.City.ToLower() == filterParams.City.ToLower()) &&
                (string.IsNullOrEmpty(filterParams.Government) ||
                    dp.User.Address.Government.ToLower() == filterParams.Government.ToLower()) &&

                // Search by name, specialization, clinic name, or bio
                (string.IsNullOrEmpty(filterParams.Search) ||
                    dp.ClinicName.ToLower().Contains(filterParams.Search.ToLower()) ||
                    dp.Specialization.ToLower().Contains(filterParams.Search.ToLower()) ||
                    dp.Bio.ToLower().Contains(filterParams.Search.ToLower()) ||
                    dp.User.FirstName.ToLower().Contains(filterParams.Search.ToLower()) ||
                    dp.User.LastName.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    // ==================== DOCTOR COUNT SPECIFICATION ====================

    public class DoctorFilterCountSpecification : BaseSpecifications<DoctorProfile, Guid>
    {
        public DoctorFilterCountSpecification(DoctorFilterParams filterParams)
            : base(BuildCriteria(filterParams))
        {
        }

        private static Expression<Func<DoctorProfile, bool>> BuildCriteria(DoctorFilterParams filterParams)
        {
            return dp =>
                 dp.IsActive == true &&
                (string.IsNullOrEmpty(filterParams.Specialization) || dp.Specialization.ToLower() == filterParams.Specialization.ToLower()) &&
                (!filterParams.MinExperienceYears.HasValue || dp.ExperienceYears >= filterParams.MinExperienceYears.Value) &&

                //  City and Government by User Address (not ClinicAddress)
                (string.IsNullOrEmpty(filterParams.City) ||
                    dp.User.Address.City.ToLower() == filterParams.City.ToLower()) &&
                (string.IsNullOrEmpty(filterParams.Government) ||
                    dp.User.Address.Government.ToLower() == filterParams.Government.ToLower()) &&

                //  Search by name, specialization, clinic name, or bio
                (string.IsNullOrEmpty(filterParams.Search) ||
                    dp.ClinicName.ToLower().Contains(filterParams.Search.ToLower()) ||
                    dp.Specialization.ToLower().Contains(filterParams.Search.ToLower()) ||
                    dp.Bio.ToLower().Contains(filterParams.Search.ToLower()) ||
                    dp.User.FirstName.ToLower().Contains(filterParams.Search.ToLower()) ||
                    dp.User.LastName.ToLower().Contains(filterParams.Search.ToLower()));
        }
    }

    // ==================== DOCTOR BY USER ID SPECIFICATION ====================

    public class DoctorByUserIdSpecification : BaseSpecifications<DoctorProfile, Guid>
    {
        public DoctorByUserIdSpecification(string userId)
            : base(dp => dp.UserId == userId && dp.IsActive == true)
        {
            Includes.Add(dp => dp.User);
            Includes.Add(dp => dp.User.Address);
            Includes.Add(dp => dp.Ratings);
        }
    }
}
