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
    public class DoctorApplyConfig : IEntityTypeConfiguration<DoctorApply>
    {
        public void Configure(EntityTypeBuilder<DoctorApply> builder)
        {
            builder.HasIndex(da => da.UserId).IsUnique();
            builder.HasIndex(da => da.Status);
        }
    }
}
