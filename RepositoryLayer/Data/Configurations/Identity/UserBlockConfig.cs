using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Identity
{
    public class UserBlockConfig : IEntityTypeConfiguration<UserBlock>
    {
        public void Configure(EntityTypeBuilder<UserBlock> builder)
        {

            builder.HasOne(ub => ub.Blocker)
                    .WithMany(u => u.BlocksInitiated)
                    .HasForeignKey(ub => ub.BlockerId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ub => ub.Blocked)
                .WithMany(u => u.BlocksReceived)
                .HasForeignKey(ub => ub.BlockedId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(ub => new { ub.BlockerId, ub.BlockedId })
                .IsUnique()
                .HasFilter("[IsActive] = 1");
        }
    }
}
