using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using LendingSolution.Infrastructure.Data;
using LendingSolution.Core.Settings;
using LendingSolution.Core.Models;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Application.Services.Implementations;
using Microsoft.Extensions.Options;

namespace LendingSolution.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "LendingSolution API",
                    Version = "v1",
                    Description = "LendingSolution API by Naries",
                    Contact = new OpenApiContact
                    {
                        Name = "Mayokun Ajiboye",
                        Email = "phynormynal@gmail.com",
                        Url = new Uri("https://www.linkedin.com/in/mayokunayobami")
                    }
                });
                opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your token in the text input below.\n\nExample: `Bearer eyJhbGciOiJIUzI1NiIsInR..."
                });

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                                        Scheme = "oauth2",
                                        Name = "Bearer",
                                        In = ParameterLocation.Header
                        },
                        new string[] { }
                    }
                });
            });
        }

        // public static void ConfigureNpgsqlContext(this IServiceCollection services, IConfiguration configuration) =>
        //     services.AddDbContext<ApplicationDbContext>(opt =>
        //             opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) =>
            services.AddDbContext<ApplicationDbContext>(opt =>
                opt.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure()
                )
            );



        // Identity and Authentication
        public static void AddJwtConfiguration(this IServiceCollection services, IConfiguration configuration) =>
                services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        public static void ConfigureJwt(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
                opt.IncludeErrorDetails = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings!.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key!)),
                };
            });
        }

        public static void ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
            })
         .AddEntityFrameworkStores<ApplicationDbContext>()
         .AddDefaultTokenProviders()
         .AddSignInManager<SignInManager<ApplicationUser>>();

        }

        // public static void ConfigureLoggerService(this IServiceCollection services) =>
        //     services.AddSingleton<ILoggerManager, LoggerManager>();

        // public static void ConfigureRepositoryManager(this IServiceCollection services) =>
        //     services.AddScoped<IRepositoryManager, RepositoryManager>();

        // public static void ConfigureRepositoryManager(this IServiceCollection services) =>
        //     services.AddScoped<IRepositoryManager, RepositoryManager>();

        public static void ConfigureServiceManager(this IServiceCollection services)
        {
            services.AddScoped<IServiceManager, ServiceManager>(provider =>
            {
                var contextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
                var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
                var configuration = provider.GetRequiredService<IOptions<JwtSettings>>();
                var tokenService = provider.GetRequiredService<ITokenService>();
                var db = provider.GetRequiredService<ApplicationDbContext>();
                return new ServiceManager(contextAccessor, userManager, configuration, tokenService, db);
            });
        }

        public static void ConfigureRemita(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RemitaSettings>(configuration.GetSection("Remita"));
            services.AddScoped<IRemitaService, RemitaService>();
            services.AddScoped<IRemitaAuthService, RemitaAuthService>();
            services.AddHttpClient();
        }

    }
}