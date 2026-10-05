using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;

using LendingSolution.Infrastructure.Data;
using LendingSolution.Core.Settings;
using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.API.Filters;
using LendingSolution.API.Auth;
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
                Description = "LendingSolution API by Naries (Version 1)",
                Contact = new OpenApiContact
                {
                    Name = "Mayokun Ajiboye",
                    Email = "phynormynal@gmail.com",
                    Url = new Uri("https://www.linkedin.com/in/mayokunayobami")
                }
            });
            
            // Enable XML comments for Swagger documentation
            var xmlFilename = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                opt.IncludeXmlComments(xmlPath);
            }
            
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

    public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure retry policy settings
        var retrySettings = configuration.GetSection("DatabaseRetryPolicy").Get<DatabaseRetryPolicySettings>() 
                          ?? new DatabaseRetryPolicySettings();
        
        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
                    sqlOptions.MigrationsAssembly("LendingSolution.Infrastructure");
                    
                    // Configure retry policy if enabled
                    if (retrySettings.EnableRetryOnFailure)
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: retrySettings.MaxRetryCount,
                            maxRetryDelay: TimeSpan.FromSeconds(retrySettings.MaxRetryDelaySeconds),
                            errorNumbersToAdd: DatabaseRetryPolicySettings.AzureSqlTransientErrors);
                    }
                    
                    // Configure command timeout
                    sqlOptions.CommandTimeout(retrySettings.CommandTimeoutSeconds);
                }
            )
        );
    }



    public static void ConfigureHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheckService>("database_connectivity");
    }

    public static void ConfigureDatabaseResilience(this IServiceCollection services)
    {
        services.AddScoped<IDatabaseResilienceService, DatabaseResilienceService>();
    }

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
            opt.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context =>
                {
                    var validator = context.HttpContext.RequestServices.GetRequiredService<UserSessionValidator>();
                    if (!await validator.IsValidAsync(context.Principal!))
                    {
                        context.Fail("Session is no longer valid");
                    }
                },
                // Give permission/role failures a JSON body with a code instead of an empty 403
                OnForbidden = async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsJsonAsync(
                        ApiResponse.Fail("You don't have permission to perform this action")
                            .WithCode(ErrorCodes.PermissionDenied));
                }
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
            // Must match the admin frontend's rule: 8+ chars with upper, lower, digit and special character
            options.Password.RequireNonAlphanumeric = true;
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
            var employeeRepository = provider.GetRequiredService<IEmployeeRepository>();
            var userRepository = provider.GetRequiredService<IUserRepository>();
            var remitaService = provider.GetRequiredService<IRemitaService>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var remitaSettings = provider.GetRequiredService<IOptions<RemitaSettings>>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var logger = provider.GetRequiredService<ILogger<RemitaService>>();
            var loanServiceLogger = provider.GetRequiredService<ILogger<LoanService>>();
            var combinedRepository = provider.GetRequiredService<ICombinedRepository>();
            var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
            var refreshTokenRepository = provider.GetRequiredService<IRefreshTokenRepository>();
            var disbursementRepository = provider.GetRequiredService<IDisbursementRepository>();
            var repaymentRepository = provider.GetRequiredService<IRepaymentRepository>();
            var supportTicketRepository = provider.GetRequiredService<ISupportTicketRepository>();
            var borrowerApplicationRepository = provider.GetRequiredService<IBorrowerApplicationRepository>();
            var remitaSalaryHistoryRepository = provider.GetRequiredService<IRemitaSalaryHistoryRepository>();
            var walletService = provider.GetRequiredService<IWalletService>();
            var emailService = provider.GetRequiredService<IEmailService>();
            var documentService = provider.GetRequiredService<IDocumentService>();
            var providusDisbursementService = provider.GetRequiredService<IProvidusDisbursementService>();
            var auditService = provider.GetRequiredService<IAuditService>();
            var otpService = provider.GetRequiredService<IOtpService>();
            var mfaSessionRepository = provider.GetRequiredService<IMfaSessionRepository>();

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
                employeeRepository,
                userRepository,
                remitaService,
                configuration,
                remitaSettings,
                httpClientFactory,
                logger,
                loanServiceLogger,
                combinedRepository,
                refreshTokenRepository,
                disbursementRepository,
                repaymentRepository,
                supportTicketRepository,
                borrowerApplicationRepository,
                remitaSalaryHistoryRepository,
                walletService,
                emailService,
                documentService,
                providusDisbursementService,
                auditService,
                otpService,
                mfaSessionRepository
            );
        });
    }

    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddMemoryCache();
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<UserSessionValidator>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<ILoanProductService, LoanProductService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IRemitaService, RemitaService>();
        services.AddScoped<IMonoService, MonoService>();
        services.AddScoped<ISupportToolsService, SupportToolsService>();
        services.AddScoped<IAdminSettingsService, AdminSettingsService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IFinanceService, FinanceService>();
        services.AddScoped<ISupportService, SupportService>();
        services.AddScoped<IDatabaseResilienceService, DatabaseResilienceService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IThirdPartyDocumentService, FirebaseDocumentService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IPaystackService, PaystackService>();
        services.AddScoped<IBorrowerOnboardingService, BorrowerOnboardingService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<INotificationOrchestrationService, NotificationOrchestrationService>();
        services.AddScoped<IOtpService, OtpService>();
        services.AddScoped<ISalaryEligibilityService, SalaryEligibilityService>();
        services.AddScoped<ISalaryHistoryViewService, SalaryHistoryViewService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IProvidusDisbursementService, ProvidusDisbursementService>();
        // Singleton so the Mono bank list is fetched daily rather than per disbursement.
        services.AddSingleton<IBankCodeResolver, BankCodeResolver>();
        services.AddScoped<IEmbedlyWalletService, EmbedlyWalletService>();
    }

    public static void RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped<ICombinedRepository, CombinedRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<ILoanProductRepository, LoanProductRepository>();
        services.AddScoped<ILoanRepository, LoanRespository>();
        services.AddScoped<IAdminSettingsRepository, AdminSettingsRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IApprovalRepository, ApprovalRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IDisbursementRepository, DisbursementRepository>();
        services.AddScoped<IRepaymentRepository, RepaymentRepository>();
        services.AddScoped<ISupportTicketRepository, SupportTicketRepository>();
        services.AddScoped<ISupportCommentRepository, SupportCommentRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
        services.AddScoped<IBorrowerApplicationRepository, BorrowerApplicationRepository>();
        services.AddScoped<IRemitaSalaryHistoryRepository, RemitaSalaryHistoryRepository>();
        services.AddScoped<IRemitaSalaryHistoryRepository, RemitaSalaryHistoryRepository>();
        services.AddScoped<IMfaSessionRepository, MfaSessionRepository>();
        services.AddScoped<IEWalletRepository, EWalletRepository>();
        services.AddScoped<IEWalletTransactionRepository, EWalletTransactionRepository>();
    }

    public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RemitaSettings>(configuration.GetSection("Remita"));
        services.Configure<ProvidusSettings>(configuration.GetSection("Providus"));
        services.Configure<EmbedlySettings>(configuration.GetSection("Embedly"));
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
        services.AddHttpClient()
            .ConfigureHttpClientDefaults(builder =>
            {
                builder.ConfigureHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30); // 30 seconds timeout
                });
            });

        // Named client for Remita — demo server can be slow, so use a longer timeout
        services.AddHttpClient("RemitaClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(200);
        });

        services.AddHttpClient("EmbedlyWalletClient", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(60);
        });
    }

    public static void ConfigureEndpointExplorer(this IServiceCollection services)
    {
        services.ConfigureSwagger();
        services.AddControllers(options => options.Filters.Add<PasswordChangeRequiredFilter>())
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                // Applies to DateTime? too: System.Text.Json wraps it for nullable values
                options.JsonSerializerOptions.Converters.Add(new LendingSolution.API.Json.UtcDateTimeConverter());
            });
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

    public static void ConfigureFirebase(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FirebaseSettings>(configuration.GetSection("Firebase"));
    }

    public static void ConfigurePaystack(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaystackSettings>(configuration.GetSection("Paystack"));
    }

    public static void ConfigureMono(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MonoSettings>(configuration.GetSection("Mono"));
    }

    public static void ConfigureEmailSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
    }

    public static void ConfigureSmsSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmsSettings>(configuration.GetSection("SmsSettings"));
    }

    public static void ConfigureApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"),
                new QueryStringApiVersionReader("api-version")
            );
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });
    }

}
