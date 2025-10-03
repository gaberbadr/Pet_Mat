using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Animals
{
    public class SpeciesConfig : IEntityTypeConfiguration<Species>
    {
        public void Configure(EntityTypeBuilder<Species> builder)
        {
            builder.HasIndex(s => s.Name).IsUnique();

            builder.HasMany(s => s.SubSpecies)
                .WithOne(ss => ss.Species)
                .HasForeignKey(ss => ss.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Animals)
                .WithOne(a => a.Species)
                .HasForeignKey(a => a.SpeciesId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
