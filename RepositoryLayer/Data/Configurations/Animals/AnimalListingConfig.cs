using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Animals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Animals
{
    public class AnimalListingConfig : IEntityTypeConfiguration<AnimalListing>
    {
        public void Configure(EntityTypeBuilder<AnimalListing> builder)
        {
            builder.HasOne(al => al.Animal)
                .WithMany(a => a.Listings)
                .HasForeignKey(al => al.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(al => al.Owner)
                .WithMany(u => u.AnimalListings)
                .HasForeignKey(al => al.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(al => al.Status);
            builder.HasIndex(al => al.CreatedAt);
        }
    }
}
