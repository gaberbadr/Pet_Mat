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
    public class PostReactionConfig : IEntityTypeConfiguration<PostReaction>
    {
        public void Configure(EntityTypeBuilder<PostReaction> builder)
        {
            builder.HasOne(pr => pr.User)
                    .WithMany(u => u.PostReactions)
                    .HasForeignKey(pr => pr.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(pr => pr.Post)
                .WithMany(p => p.Reactions)
                .HasForeignKey(pr => pr.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(pr => new { pr.UserId, pr.PostId }).IsUnique();
            builder.HasIndex(pr => pr.PostId);
        }
    }
}
