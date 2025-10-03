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
    public class SubSpeciesConfig : IEntityTypeConfiguration<SubSpecies>
    {
        public void Configure(EntityTypeBuilder<SubSpecies> builder)
        {

            builder.HasIndex(ss => new { ss.Name, ss.SpeciesId }).IsUnique();
        }
    }
}
