using LendingSolution.Core.Settings;
using LendingSolution.API.Extensions;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureRepositories(builder.Configuration);
builder.Services.RegisterServices();
builder.Services.RegisterRepositories();
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureHttpClient(builder.Configuration);
builder.Services.ConfigureCors();
builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.ConfigureEndpointExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    // db.Database.Migrate();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var roles = new[] { "SuperAdmin", "Admin", "LoanOfficer", "CollectionsOfficer", "Underwriter", "SupportAgent", "Auditor", "Viewer" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}


app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();

// Enable Swagger in development environment
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI();
// }


app.MapControllers();
// app.UseHttpsRedirection();
app.Run();
