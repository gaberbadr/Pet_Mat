using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RepositoryLayer.Data.Configurations.Context
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.HasOne(o => o.User)
                   .WithMany(u => u.Orders)
                   .HasForeignKey(o => o.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(o => o.Cart)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CartId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(o => o.DeliveryMethod)
                .WithMany(dm => dm.Orders)
                .HasForeignKey(o => o.DeliveryMethodId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(o => o.ShippingAddress)
                .WithMany()
                .HasForeignKey(o => o.ShippingAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(o => o.UserId);
            builder.HasIndex(o => o.OrderDate);
            builder.HasIndex(o => o.Status);
            builder.HasIndex(o => o.PaymentIntentId);
        }
    }
}
