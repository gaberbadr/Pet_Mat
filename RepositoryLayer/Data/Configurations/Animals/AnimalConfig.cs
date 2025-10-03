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
    public class AnimalConfig : IEntityTypeConfiguration<Animal>
    {
        public void Configure(EntityTypeBuilder<Animal> builder)
        {
            builder.HasOne(a => a.Owner)
                    .WithMany(u => u.Animals)
                    .HasForeignKey(a => a.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Species)
                .WithMany(s => s.Animals)
                .HasForeignKey(a => a.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.SubSpecies)
                .WithMany(ss => ss.Animals)
                .HasForeignKey(a => a.SubSpeciesId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(a => a.Color)
                .WithMany(c => c.Animals)
                .HasForeignKey(a => a.ColorId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(a => a.OwnerId);
            builder.HasIndex(a => a.SpeciesId);
            builder.HasIndex(a => a.IsActive);
        }
    }
}
