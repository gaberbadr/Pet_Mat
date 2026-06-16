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
    public class SubscriptionPackageConfig : IEntityTypeConfiguration<SubscriptionPackage>
    {
        public void Configure(EntityTypeBuilder<SubscriptionPackage> builder)
        {
            // Configure Name
            builder.Property(sp => sp.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("nvarchar(100)");

            // Configure Description
            builder.Property(sp => sp.Description)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            // Configure Price
            builder.Property(sp => sp.Price)
                .HasColumnType("decimal(18,2)");

            // Configure Features as JSON with proper value converter and comparer
            builder.Property(sp => sp.Features)
                .HasConversion(
                    features => string.Join(",", features),
                    csv => csv.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .HasColumnType("nvarchar(max)")
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => new List<string>(c)
                ));

            // Configure IsActive
            builder.Property(sp => sp.IsActive)
                .HasColumnType("bit");

            // Configure DurationInDays
            builder.Property(sp => sp.DurationInDays)
                .HasColumnType("int");

            // Configure table name
            builder.ToTable("SubscriptionPackage");

            // Configure indexes
            builder.HasIndex(sp => sp.Name)
                .HasDatabaseName("IX_SubscriptionPackage_Name");

            builder.HasIndex(sp => sp.IsActive)
                .HasDatabaseName("IX_SubscriptionPackage_IsActive");

            // Configure relationship with DoctorSubscription
            builder.HasMany(sp => sp.Subscriptions)
                .WithOne(ds => ds.Package)
                .HasForeignKey(ds => ds.PackageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}