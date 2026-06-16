using CoreLayer.Entities.Doctors;
using CoreLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Doctors
{
    public class DoctorSubscriptionConfig : IEntityTypeConfiguration<DoctorSubscription>
    {
        public void Configure(EntityTypeBuilder<DoctorSubscription> builder)
        {
            // Configure Status enum conversion (enum → int in database)
            builder.Property(ds => ds.Status)
                .HasConversion(
                    status => (int)status,
                    value => (SubscriptionStatus)value
                )
                .HasColumnType("int");

            // Configure string constraints
            builder.Property(ds => ds.PaymentIntentId)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("nvarchar(200)");

            builder.Property(ds => ds.ClientSecret)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("nvarchar(500)");

            // Configure decimal for AmountPaid
            builder.Property(ds => ds.AmountPaid)
                .HasColumnType("decimal(18,2)");

            // Configure foreign key to DoctorProfile
            builder.HasOne(ds => ds.DoctorProfile)
                .WithMany(dp => dp.Subscriptions)
                .HasForeignKey(ds => ds.DoctorId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Configure foreign key to SubscriptionPackage
            builder.HasOne(ds => ds.Package)
                .WithMany(sp => sp.Subscriptions)
                .HasForeignKey(ds => ds.PackageId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Configure indexes
            builder.HasIndex(ds => ds.DoctorId)
                .HasDatabaseName("IX_DoctorSubscriptions_DoctorId");

            builder.HasIndex(ds => ds.PackageId)
                .HasDatabaseName("IX_DoctorSubscriptions_PackageId");

            builder.HasIndex(ds => ds.PaymentIntentId)
                .IsUnique()
                .HasDatabaseName("IX_DoctorSubscriptions_PaymentIntentId");

            builder.HasIndex(ds => ds.Status)
                .HasDatabaseName("IX_DoctorSubscriptions_Status");

            builder.HasIndex(ds => ds.IsActive)
                .HasDatabaseName("IX_DoctorSubscriptions_IsActive");

            builder.ToTable("DoctorSubscriptions");
        }
    }
}
