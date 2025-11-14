using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Pharmacies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Food
{
    public class ProductConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasOne(p => p.Brand)
               .WithMany(pb => pb.Products)
               .HasForeignKey(p => p.BrandId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Type)
                .WithMany(pt => pt.Products)
                .HasForeignKey(p => p.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Species)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SpeciesId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => p.BrandId);
            builder.HasIndex(p => p.TypeId);
            builder.HasIndex(p => p.Price);
            builder.HasIndex(p => p.Stock);
        }
    }
}
