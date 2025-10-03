
using System;
using System.Text;
using System.Threading.RateLimiting;
using CoreLayer;
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
using RepositoryLayer;
using RepositoryLayer.Data.Context;
using RepositoryLayer.Data.Data_seeding;
using ServiceLayer.Services.Auth.AuthUser;
using ServiceLayer.Services.Auth.Jwt;
using ServiceLayer.Services.Auth.LoginRateLimiter;

namespace petmat
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);




            // Configuration
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            // Database configuration
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

          

            // Configure Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // services
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IJwtService, JwtService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ILoginRateLimiterService, LoginRateLimiterService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

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
            });

            // Google Authentication (conditional)
            var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
            var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
            {
                builder.Services.AddAuthentication().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                    options.Scope.Add("email");
                    options.Scope.Add("profile");
                });
            }

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // Configure model validation response
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errors = actionContext.ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .SelectMany(x => x.Value.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToArray();

                    return new BadRequestObjectResult(new ApiValidationErrorResponse { Errors = errors });
                };
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Rate Limiting - 15 requests per minute per IP
            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 30,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        }));
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

            // Use CORS first
            app.UseCors("AllowAll");

            // Database initialization
            await InitializeDatabaseAsync(app);


            // Configure pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                // Always enable Swagger for easy testing in production
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "petmat API V1");
                    c.RoutePrefix = "swagger";
                });
            }



            app.UseHttpsRedirection();

            app.UseRateLimiter();
            app.UseAuthentication();

            // Enable serving static files from wwwroot
            app.UseStaticFiles();

            // Configure file serving with proper MIME types
            var filesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");

            var allowedExtensions = new[]
            {
            ".jpg", ".jpeg", ".png", ".gif", ".webp",
            ".mp4", ".webm", ".ogg", ".mov", ".avi", ".mkv",
            ".pdf", ".docx", ".xlsx", ".pptx", ".txt", ".rtf"

            };

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(filesPath),
                RequestPath = "/files",
                OnPrepareResponse = context =>
                {
                    var fileExtension = Path.GetExtension(context.File.Name).ToLowerInvariant();

                    // Block if not in allowed list
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        context.Context.Response.StatusCode = StatusCodes.Status404NotFound;
                        context.Context.Response.ContentLength = 0;
                        context.Context.Response.Body = Stream.Null;
                        return;
                    }

                    // Set caching headers for 1 year and correct MIME type
                    context.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000"; // 1 year cache
                    context.Context.Response.ContentType = fileExtension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        ".mp4" => "video/mp4",
                        ".webm" => "video/webm",
                        ".ogg" => "video/ogg",
                        ".mov" => "video/quicktime",
                        ".avi" => "video/x-msvideo",
                        ".mkv" => "video/x-matroska",
                        ".pdf" => "application/pdf",
                        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                        ".txt" => "text/plain",
                        ".rtf" => "application/rtf",
                        _ => "application/octet-stream"
                    };
                }
            });


            app.UseAuthorization();

            // Custom middleware
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseStatusCodePagesWithReExecute("/error/{0}");



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
