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
    public class DoctorRatingConfig : IEntityTypeConfiguration<DoctorRating>
    {
        public void Configure(EntityTypeBuilder<DoctorRating> builder)
        {
            builder.HasOne(dr => dr.Doctor)
                    .WithMany()
                    .HasForeignKey(dr => dr.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(dr => dr.User)
                .WithMany(u => u.DoctorRatingsGiven)
                .HasForeignKey(dr => dr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(dr => new { dr.DoctorId, dr.UserId }).IsUnique();
            builder.HasIndex(dr => dr.DoctorId);
        }
    }
}
