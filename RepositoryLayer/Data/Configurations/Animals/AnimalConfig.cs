using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Animals;
using CoreLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Animals
{
    public class AnimalConfig : IEntityTypeConfiguration<Animal>
    {
        public void Configure(EntityTypeBuilder<Animal> builder)
        {

            // Convert Status enum to string
            builder.Property(a => a.Gender)
                .HasConversion(
                    status => status.ToString(),// to database
                    status => (Gender)Enum.Parse(typeof(Gender), status)//Take the string value from the database ("male") and convert it back to the corresponding enum value (Gender.Male).
                )
                .HasMaxLength(50);
            // Convert Status enum to string
            builder.Property(a => a.Size)
                .HasConversion(
                    Size => Size.ToString(),// to database
                    Size => (AnimalSize)Enum.Parse(typeof(AnimalSize), Size)//Take the string value from the database ("Small") and convert it back to the corresponding enum value (size.small).
                )
                .HasMaxLength(50);

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
