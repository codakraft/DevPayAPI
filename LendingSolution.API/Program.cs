using Microsoft.AspNetCore.Authentication.JwtBearer;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Application.Services.Implementations;
using LendingSolution.Core.Settings;
using LendingSolution.API.Extensions;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.ConfigureRepositories(builder.Configuration);
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
    db.Database.Migrate();
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
