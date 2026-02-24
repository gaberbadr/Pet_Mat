using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Doctors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Doctors
{
    public class DoctorProfileConfig : IEntityTypeConfiguration<DoctorProfile>
    {
        public void Configure(EntityTypeBuilder<DoctorProfile> builder)
        {
            builder.HasIndex(dp => dp.UserId).IsUnique();
            builder.HasIndex(dp => dp.IsActive);
            builder.HasIndex(dp => new { dp.Latitude, dp.Longitude });
            builder.HasIndex(dp => dp.Specialization);

            builder.HasMany(dp => dp.Ratings)
                   .WithOne()
                   .HasForeignKey(dr => dr.DoctorId)
                   .HasPrincipalKey(dp => dp.UserId); //we link the DoctorId(fk) in Ratings table with UserId secondary key in DoctorProfile table, not the primary key Id of DoctorProfile
        }
    }
}
