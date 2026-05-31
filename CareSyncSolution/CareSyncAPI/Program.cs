using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CareSyncAPI.Data;
using CareSyncAPI.Models;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = "Development"
});

//var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// SignalR
builder.Services.AddSignalR();

// CORS for SignalR clients hosted on the MVC origin
const string SignalRCorsPolicy = "SignalRCors";

// Allowed MVC client origins. The deployed origin is supplied via the
// "MvcClientOrigin" app setting / environment variable (e.g. set in the
// Azure portal); local dev origins are always allowed as a fallback.
var allowedOrigins = new List<string>
{
    "https://localhost:7230",
    "http://localhost:5216"
};
var mvcClientOrigin = builder.Configuration["MvcClientOrigin"];
if (!string.IsNullOrWhiteSpace(mvcClientOrigin))
{
    foreach (var origin in mvcClientOrigin.Split(
        ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        // CORS origins must include a scheme; default to https when omitted
        // (Azure app setting values are often just the host name).
        allowedOrigins.Add(
            origin.Contains("://", StringComparison.Ordinal) ? origin : $"https://{origin}");
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(SignalRCorsPolicy, policy =>
    {
        policy
            .WithOrigins(allowedOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "CareSyncAPI", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token here"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed database
try
{
    await DbSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    Console.WriteLine($"Seeding error (non-fatal): {ex.Message}");
}

// Middleware pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(SignalRCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<CareSyncAPI.Hubs.AppointmentHub>("/hubs/appointment");

app.Run();