using LendingSolution.API.Extensions;
using LendingSolution.Infrastructure.Data;
using LendingSolution.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add database logger provider for persisting logs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Logging.AddProvider(new DatabaseLoggerProvider(connectionString, LogLevel.Information));
}

builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureRepositories(builder.Configuration);
builder.Services.RegisterServices();
builder.Services.RegisterRepositories();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureHttpClient(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.ConfigureHealthChecks(builder.Configuration);
builder.Services.ConfigureDatabaseResilience();
builder.Services.ConfigureFirebase(builder.Configuration);
builder.Services.ConfigurePaystack(builder.Configuration);
builder.Services.ConfigureMono(builder.Configuration);
builder.Services.ConfigureEmailSettings(builder.Configuration);
builder.Services.ConfigureSmsSettings(builder.Configuration);
builder.Services.ConfigureServices(builder.Configuration);
builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.ConfigureApiVersioning();
builder.Services.ConfigureEndpointExplorer();

var nigeriaCulture = new CultureInfo("en-NG");
CultureInfo.DefaultThreadCurrentCulture = nigeriaCulture;
CultureInfo.DefaultThreadCurrentUICulture = nigeriaCulture;

var app = builder.Build();

// Apply database migrations with error handling
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Attempting to apply database migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 40615) // Firewall rule error
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError("==================== DATABASE CONNECTION BLOCKED ====================");
        logger.LogError("Your IP address is not allowed to access the Azure SQL Server.");
        logger.LogError("Current IP: Check your public IP at https://whatismyipaddress.com/");
        logger.LogError("To fix this:");
        logger.LogError("1. Go to Azure Portal > SQL Server 'lending-app' > Networking");
        logger.LogError("2. Add your IP address to the firewall rules");
        logger.LogError("3. Wait up to 5 minutes for changes to take effect");
        logger.LogError("=====================================================================");
        
        if (app.Environment.IsDevelopment())
        {
            logger.LogWarning("Continuing in development mode without database migrations...");
        }
        else
        {
            throw;
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while applying database migrations");
        
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
        
        logger.LogWarning("Continuing in development mode despite migration error...");
    }
}

using (var scope = app.Services.CreateScope())
{
    try
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "SuperAdmin", "Admin", "LoanOfficer", "CollectionsOfficer", "Underwriter", "SupportAgent", "Auditor", "Viewer" };
        
        logger.LogInformation("Seeding roles...");
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role: {Role}", role);
            }
        }
        logger.LogInformation("Role seeding completed");
    }
    catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 40615) // Firewall rule error
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Skipping role seeding due to database connection issues (firewall blocked)");
        
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles");
        
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
        
        logger.LogWarning("Continuing in development mode despite role seeding error...");
    }
}

app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();

// Enable Swagger unless explicitly disabled in configuration
var disableSwagger = app.Configuration.GetValue<bool>("DisableSwagger");
if (!disableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseReDoc(c =>
    {
        c.RoutePrefix = "docs"; // ReDoc UI will be at /docs
        c.DocumentTitle = "My API Docs";
        c.SpecUrl("/swagger/v1/swagger.json");
    });
}

app.MapControllers();
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});

app.Run();
