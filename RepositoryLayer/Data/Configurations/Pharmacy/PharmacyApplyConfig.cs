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
    public class PharmacyApplyConfig : IEntityTypeConfiguration<PharmacyApply>
    {
        public void Configure(EntityTypeBuilder<PharmacyApply> builder)
        {
            builder.HasIndex(pa => pa.UserId).IsUnique();
            builder.HasIndex(pa => pa.Status);
        }
    }
}
