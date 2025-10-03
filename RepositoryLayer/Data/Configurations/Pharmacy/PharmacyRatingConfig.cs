using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Pharmacies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Pharmacy
{
    public class PharmacyRatingConfig : IEntityTypeConfiguration<PharmacyRating>
    {
        public void Configure(EntityTypeBuilder<PharmacyRating> builder)
        {
            builder.HasOne(pr => pr.Pharmacy)
                   .WithMany()
                   .HasForeignKey(pr => pr.PharmacyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pr => pr.User)
                .WithMany(u => u.PharmacyRatingsGiven)
                .HasForeignKey(pr => pr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(pr => new { pr.PharmacyId, pr.UserId }).IsUnique();
            builder.HasIndex(pr => pr.PharmacyId);
        }
    }
}
