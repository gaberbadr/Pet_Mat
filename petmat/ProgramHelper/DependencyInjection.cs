using System.Text;
using System.Threading.RateLimiting;
using CoreLayer;
using CoreLayer.AutoMapper.AdminMapping;
using CoreLayer.AutoMapper.AnimalMapping;
using CoreLayer.AutoMapper.DoctorMapping;
using CoreLayer.AutoMapper.OrderMapping;
using CoreLayer.AutoMapper.PharmacyMapping;
using CoreLayer.AutoMapper.ProductMapping;
using CoreLayer.Entities.Identity;
using CoreLayer.Helper.EmailSend;
using CoreLayer.Service_Interface;
using CoreLayer.Service_Interface.Accessory;
using CoreLayer.Service_Interface.Admin;
using CoreLayer.Service_Interface.Doctor;
using CoreLayer.Service_Interface.IAuth;
using CoreLayer.Service_Interface.Orders;
using CoreLayer.Service_Interface.Pharmacy;
using CoreLayer.Service_Interface.Products;
using CoreLayer.Service_Interface.User;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using petmat.Errors;
using RepositoryLayer;
using RepositoryLayer.Data.Context;
using ServiceLayer.Services.Accessory;
using ServiceLayer.Services.Admin;
using ServiceLayer.Services.Animals;
using ServiceLayer.Services.Auth.AuthUser;
using ServiceLayer.Services.Auth.Jwt;
using ServiceLayer.Services.Auth.LoginRateLimiter;
using ServiceLayer.Services.Doctor;
using ServiceLayer.Services.Orders;
using ServiceLayer.Services.Pharmacy;
using ServiceLayer.Services.Products;
using ServiceLayer.Services.User;
using Stripe;
using Stripe.Climate;

namespace petmat.ProgramHelper
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependency(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddProgramBuiltInServices();
            services.AddSwaggerServices();
            services.AddDataBaseServices(configuration);
            services.AddGoogleAuthenticationServices(configuration);
            services.AddUserDefinedServices();
            services.AddAutoMapperServices(configuration);
            services.AddIdentityServices();
            services.AddCorsServices();
            services.AddApiValidationErrorResponseServices();
            services.AddLimiterServices();
            services.AddJwtAuthenticationServices(configuration);

            //for caching  Performance Optimization
            services.AddMemoryCache();
            services.AddResponseCaching();

            return services;
        }

        private static IServiceCollection AddProgramBuiltInServices(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    // This allows string enum conversion and makes it case-insensitive
                    options.JsonSerializerOptions.Converters.Add(
                        new System.Text.Json.Serialization.JsonStringEnumConverter());
                });
            return services;
        }

        private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            return services;
        }

        private static IServiceCollection AddDataBaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            return services;
        }

        private static IServiceCollection AddJwtAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtKey = configuration["JWT:Key"] ?? "DefaultKeyForDevelopmentOnlyNotForProduction123";
            var key = Encoding.UTF8.GetBytes(jwtKey);

            services.AddAuthentication(options =>
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
                    ValidIssuer = configuration["JWT:Issuer"] ?? "http://localhost:5000/",
                    ValidAudience = configuration["JWT:Audience"] ?? "petmatAPI",
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

            return services;
        }

        private static IServiceCollection AddUserDefinedServices(this IServiceCollection services)
        {
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ILoginRateLimiterService, LoginRateLimiterService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IAuthUserService, AuthUserService>();
            services.AddScoped<IAdminAnimalManagement, AdminAnimalManagement>();
            services.AddScoped<IAdminUserManagement, AdminUserManagement>();
            services.AddScoped<IAdminDoctorApplicationManagement, AdminDoctorApplicationManagement>();
            services.AddScoped<IUserAnimalManagement, UserAnimalManagement>();
            services.AddScoped<IUserDoctorManagement, UserDoctorManagement>();
            services.AddScoped<IDoctorService, DoctorService>();
            services.AddScoped<IPharmacyService, PharmacyService>();
            services.AddScoped<IAdminPharmacyApplicationManagement, AdminPharmacyApplicationManagement>();
            services.AddScoped<IUserPharmacyManagement, UserPharmacyManagement>();
            services.AddScoped<IUserAccessoryManagement, UserAccessoryManagement>();
            // Cart & Order Services
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IOrderService, ServiceLayer.Services.Orders.OrderService>();
            services.AddScoped<IAdminOrderService, AdminOrderService>();
            services.AddScoped<ICouponService, ServiceLayer.Services.Orders.CouponService>();
            services.AddScoped<IDeliveryMethodService, DeliveryMethodService>();
            services.AddScoped<IPaymentService, PaymentService>();

            // Product Services
            services.AddScoped<IProductService, ServiceLayer.Services.Products.ProductService>();
            services.AddScoped<IAdminProductService, AdminProductService>();




            return services;
        }

        private static IServiceCollection AddAutoMapperServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register IConfiguration as singleton if not already registered
            services.AddSingleton(configuration);

            services.AddAutoMapper(config =>
            {
                // Add profiles that don't need configuration
                config.AddProfile<AdminMappingProfile>();
                config.AddProfile<DoctorMappingProfile>();

                // Add profiles that need configuration through factory
                config.AddProfile(new UserMappingProfile(configuration));
                config.AddProfile(new PharmacyMappingProfile(configuration));
                config.AddProfile(new AccessoryMappingProfile(configuration));


                // Add Order and Product Mapping Profiles
                config.AddProfile<OrderMappingProfile>();
                config.AddProfile<ProductMappingProfile>();
            });

            return services;
        }

        private static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
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

            return services;
        }

        private static IServiceCollection AddGoogleAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var googleClientId = configuration["Authentication:Google:ClientId"];
            var googleClientSecret = configuration["Authentication:Google:ClientSecret"];

            if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
            {
                services.AddAuthentication().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                    options.Scope.Add("email");
                    options.Scope.Add("profile");
                });
            }

            return services;
        }

        private static IServiceCollection AddCorsServices(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            return services;
        }

        private static IServiceCollection AddApiValidationErrorResponseServices(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
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

            return services;
        }

        private static IServiceCollection AddLimiterServices(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
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

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
                };
            });

            return services;
        }
    }
}