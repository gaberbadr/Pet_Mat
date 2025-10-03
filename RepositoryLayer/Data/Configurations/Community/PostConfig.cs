using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Community;
using CoreLayer.Entities.Foods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Community
{
    public class PostConfig : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.HasOne(p => p.User)
                     .WithMany(u => u.Posts)
                     .HasForeignKey(p => p.UserId)
                     .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Species)
                .WithMany(s => s.Posts)
                .HasForeignKey(p => p.SpeciesId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(p => p.UserId);
            builder.HasIndex(p => p.IsActive);
            builder.HasIndex(p => p.CreatedAt);
        }
    }
}
