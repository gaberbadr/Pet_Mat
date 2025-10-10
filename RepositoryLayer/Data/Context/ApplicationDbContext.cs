using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CoreLayer.Entities.Accessories;
using CoreLayer.Entities.Animals;
using CoreLayer.Entities.Carts;
using CoreLayer.Entities.Community;
using CoreLayer.Entities.Doctors;
using CoreLayer.Entities.Foods;
using CoreLayer.Entities.Identity;
using CoreLayer.Entities.Messages;
using CoreLayer.Entities.Orders;
using CoreLayer.Entities.Pharmacies;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RepositoryLayer.Data.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> opts) : base(opts) { }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }




        // Add DbSets for all entities
        public DbSet<Address> Addresses { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserBlock> UserBlocks { get; set; }
        public DbSet<Species> Species { get; set; }
        public DbSet<SubSpecies> SubSpecies { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<AnimalListing> AnimalListings { get; set; }
        public DbSet<DoctorApply> DoctorApplications { get; set; }
        public DbSet<DoctorProfile> DoctorProfiles { get; set; }
        public DbSet<DoctorRating> DoctorRatings { get; set; }
        public DbSet<PharmacyApply> PharmacyApplications { get; set; }
        public DbSet<PharmacyProfile> PharmacyProfiles { get; set; }
        public DbSet<PharmacyListing> PharmacyListings { get; set; }
        public DbSet<PharmacyRating> PharmacyRatings { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductBrand> ProductBrands { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderAddress> OrderAddresses { get; set; }
        public DbSet<DeliveryMethod> DeliveryMethods { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<AccessoryListing> AccessoryListings { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PostReaction> PostReactions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserConnection> UserConnections { get; set; }
        public DbSet<LoginAttempt> LoginAttempts { get; set; }



    }
}
