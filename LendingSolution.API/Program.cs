using Microsoft.AspNetCore.Authentication.JwtBearer;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Application.Services.Implementations;
using LendingSolution.Core.Settings;
using LendingSolution.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.ConfigureNpgsqlContext(builder.Configuration);
builder.Services.ConfigureIdentity();
builder.Services.AddJwtConfiguration(builder.Configuration);
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.ConfigureServiceManager();
builder.Services.ConfigureSwagger();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors(x => x.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());
app.UseAuthentication();
app.UseAuthorization();

// Enable Swagger in development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapControllers();
// app.UseHttpsRedirection();
app.Run();