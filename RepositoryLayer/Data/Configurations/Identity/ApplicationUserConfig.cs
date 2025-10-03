using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Carts;
using CoreLayer.Entities.Doctors;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Pharmacies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Identity
{
    public class ApplicationUserConfig : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {

            builder.HasOne(u => u.Address)
                 .WithMany(a => a.Users)
                 .HasForeignKey(u => u.AddressId)
                 .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.DoctorProfile)
                .WithOne(dp => dp.User)
                .HasForeignKey<DoctorProfile>(dp => dp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.DoctorApplication)
                .WithOne(da => da.User)
                .HasForeignKey<DoctorApply>(da => da.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.PharmacyProfile)
                .WithOne(pp => pp.User)
                .HasForeignKey<PharmacyProfile>(pp => pp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.PharmacyApplication)
                .WithOne(pa => pa.User)
                .HasForeignKey<PharmacyApply>(pa => pa.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(u => u.ProfilePicture)
                .IsRequired(false);
        }
    }
}
