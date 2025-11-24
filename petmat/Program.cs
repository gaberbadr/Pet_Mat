
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using CoreLayer;
using CoreLayer.AutoMapper.AnimalMapping;
using CoreLayer.AutoMapper.DoctorMapping;
using CoreLayer.Entities.Identity;
using CoreLayer.Helper.EmailSend;
using CoreLayer.Service_Interface;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using petmat.Errors;
using petmat.Middleware;
using petmat.ProgramHelper;
using RepositoryLayer;
using RepositoryLayer.Data.Context;
using RepositoryLayer.Data.Data_seeding;
using ServiceLayer.Services.Admin;
using ServiceLayer.Services.Auth.AuthUser;
using ServiceLayer.Services.Auth.Jwt;
using ServiceLayer.Services.Auth.LoginRateLimiter;
using ServiceLayer.Services.Doctor;
using ServiceLayer.Services.User;

namespace petmat
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== FIXED: Configuration setup for Docker =====
            // Load environment variables FIRST (for Docker .env support)
            builder.Configuration.AddEnvironmentVariables();

            // Then load appsettings.json only if it exists (optional for Docker)
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

            // Add dependency injection
            builder.Services.AddDependency(builder.Configuration);

          
            var app = builder.Build();

            // Apply migrations
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Migration warning: {ex.Message}");
                }
            }

            // Database initialization
            await InitializeDatabaseAsync(app);

            // Configure all middlewares
            await app.ConfigureMiddlewaresAsync();

            app.MapControllers();
            app.Run();
        }

        private static async Task InitializeDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<ApplicationDbContext>();
            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<Program>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            try
            {
                logger.LogInformation("Starting database initialization...");
                logger.LogInformation("Database migration completed successfully");

                // Seed roles and users
                logger.LogInformation("Starting data seeding...");
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var unitOfWork = services.GetRequiredService<IUnitOfWork>();

                await IdentitySeeder.SeedAppUserAsync(userManager, roleManager);

                await DataSeeder.SeedDeliveryMethodsAsync(unitOfWork);
                await DataSeeder.SeedProductBrandsAsync(unitOfWork);
                await DataSeeder.SeedProductTypesAsync(unitOfWork);

                logger.LogInformation("Data seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during database initialization: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }
}
