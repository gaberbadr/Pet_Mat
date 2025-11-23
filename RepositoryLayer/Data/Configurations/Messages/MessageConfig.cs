using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Messages;
using CoreLayer.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Messages
{
    public class MessageConfig : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            // Convert MessageType enum to string
            builder.Property(m => m.Type)
                .HasConversion(
                    type => type.ToString(),
                    type => (MessageType)Enum.Parse(typeof(MessageType), type))
                .HasMaxLength(50);

            // Convert MessageContextType enum to string
            builder.Property(m => m.ContextType)
                .HasConversion(
                    type => type.ToString(),
                    type => (MessageContextType)Enum.Parse(typeof(MessageContextType), type))
                .HasMaxLength(50);

            builder.HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Receiver)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(m => new { m.SenderId, m.ReceiverId });
            builder.HasIndex(m => m.SentAt);
            builder.HasIndex(m => m.IsRead);
            builder.HasIndex(m => m.ContextType);
        }
    }
}
