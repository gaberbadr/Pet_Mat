using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Doctors;
using CoreLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Doctors
{
    public class DoctorApplyConfig : IEntityTypeConfiguration<DoctorApply>
    {
        public void Configure(EntityTypeBuilder<DoctorApply> builder)
        {
            // Convert Status enum to string
            builder.Property(al => al.Status)
                .HasConversion(
                    status => status.ToString(),// to database
                    status => (ApplicationStatus)Enum.Parse(typeof(ApplicationStatus), status)//Take the string value from the database ("Active") and convert it back to the corresponding enum value (ListingStatus.Active).
                )
                .HasMaxLength(50);

            builder.HasIndex(da => da.UserId).IsUnique();
            builder.HasIndex(da => da.Status);
        }
    }
}
