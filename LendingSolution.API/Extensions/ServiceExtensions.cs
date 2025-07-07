using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;

using LendingSolution.Infrastructure.Data;
using LendingSolution.Core.Settings;
using LendingSolution.Core.Models;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Application.Services.Implementations;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Repositories.Implementations;

namespace LendingSolution.API.Extensions;

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
                        Array.Empty<string>()
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
                sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                    sqlOptions.MigrationsAssembly("LendingSolution.Infrastructure");
                    sqlOptions.MigrationsAssembly("LendingSolution.Infrastructure");
                }
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
        .AddRoles<IdentityRole>()
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
            var jwtSettings = provider.GetRequiredService<IOptions<JwtSettings>>();
            var IAuthService = provider.GetRequiredService<IAuthService>();
            var tokenService = provider.GetRequiredService<ITokenService>();
            var db = provider.GetRequiredService<ApplicationDbContext>();
            var companyRepository = provider.GetRequiredService<ICompanyRepository>();
            var loanRepository = provider.GetRequiredService<ILoanRepository>();
            var loanProductRepository = provider.GetRequiredService<ILoanProductRepository>();
            var remitaService = provider.GetRequiredService<IRemitaService>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var remitaSettings = provider.GetRequiredService<IOptions<RemitaSettings>>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var logger = provider.GetRequiredService<ILogger<RemitaService>>();
            var combinedRepository = provider.GetRequiredService<ICombinedRepository>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var refreshTokenRepository = provider.GetRequiredService<IRefreshTokenRepository>();
            var disbursementRepository = provider.GetRequiredService<IDisbursementRepository>();
            var repaymentRepository = provider.GetRequiredService<IRepaymentRepository>();
            var supportTicketRepository = provider.GetRequiredService<ISupportTicketRepository>();

            return new ServiceManager(
                contextAccessor,
                userManager,
                roleManager,
                jwtSettings,
                tokenService,
                db,
                companyRepository,
                loanRepository,
                loanProductRepository,
                remitaService,
                configuration,
                remitaSettings,
                httpClientFactory,
                logger,
                combinedRepository,
                refreshTokenRepository,
                disbursementRepository,
                repaymentRepository,
                supportTicketRepository
            );
        });
    }

    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<ILoanProductService, LoanProductService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IRemitaService, RemitaService>();
        services.AddScoped<ISupportToolsService, SupportToolsService>();
        services.AddScoped<IAdminSettingsService, AdminSettingsService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<ISupportService, SupportService>();
    }

    public static void RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICombinedRepository, CombinedRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ILoanProductRepository, LoanProductRepository>();
        services.AddScoped<ILoanRepository, LoanRespository>();
        services.AddScoped<IAdminSettingsRepository, AdminSettingsRepository>();
        services.AddScoped<IApprovalRepository, ApprovalRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IDisbursementRepository, DisbursementRepository>();
        services.AddScoped<IRepaymentRepository, RepaymentRepository>();
        services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
        services.AddScoped<ISupportCommentRepository, SupportCommentRepository>();
    }

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // services.Configure<RemitaSettings>(configuration.GetSection("Remita"));
        // services.AddScoped<IRemitaService, RemitaService>();
        // services.AddScoped<IAuthService, AuthService>();
        // services.AddScoped<IProfileService, ProfileService>();
        // services.AddScoped<ITokenService, TokenService>();
        // services.AddScoped<ISupportToolsService, SupportToolsService>();

    }

    public static void ConfigureRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ILoanProductRepository, LoanProductRepository>();
    }

    public static void ConfigureHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
    }

    public static void ConfigureEndpointExplorer(this IServiceCollection services)
    {
        services.ConfigureSwagger();
        services.AddControllers();
        services.AddEndpointsApiExplorer();

    }

    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(opt =>
        {
            opt.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
    }

}
