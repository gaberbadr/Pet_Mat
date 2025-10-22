
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


            // Configuration - Load this FIRST before any services
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();


            builder.Services.AddDependency(builder.Configuration);



            // JWT Configuration
            var jwtKey = builder.Configuration["JWT:Key"] ?? "DefaultKeyForDevelopmentOnlyNotForProduction123";
            var key = Encoding.UTF8.GetBytes(jwtKey);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = builder.Configuration["JWT:Issuer"] ?? "http://localhost:5000/",
                    ValidAudience = builder.Configuration["JWT:Audience"] ?? "petmatAPI",
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        // Block default reply
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        return context.Response.WriteAsync(
                            "{\"error\": \"Unauthorized - Please login or provide a valid token.\"}");
                    },
                    OnForbidden = context =>//rather than  redirect to 403 page return empty json message
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var message = $"Access denied . You don't have permission to access this area.";

                        return context.Response.WriteAsync($"{{\"error\": \"{message}\"}}");
                    }
                };
            });


          
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

                await IdentitySeeder.SeedAppUserAsync(userManager, roleManager, unitOfWork);

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
