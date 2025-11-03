using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Galileo_API;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger + Bearer
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Galileo API", Version = "v1" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Ingresa: Bearer {tu_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
});

// === JWT Auth (SIN clave en appsettings) ===
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyString = builder.Configuration["Jwt:Key"]; // viene de user-secrets (dev) o env var Jwt__Key (prod)
if (string.IsNullOrWhiteSpace(keyString))
    throw new InvalidOperationException("Jwt:Key no está configurada. Define la key con 'dotnet user-secrets set \"Jwt:Key\" \"...\"' en dev, o como variable Jwt__Key en prod.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],

            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };

        // No loguear contenido sensible
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = _ => Task.CompletedTask,
            OnTokenValidated = _ => Task.CompletedTask
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ===== Endpoint protegido de prueba =====
app.MapGet("/secure-ping", () => Results.Ok("Pong seguro ✅"))
   .RequireAuthorization();

// ===== Endpoint de login (DEMO) que emite un JWT =====
app.MapPost("/login", (LoginRequest req, IConfiguration cfg) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest("Usuario/Password requeridos");

    var jwt = cfg.GetSection("Jwt");

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, req.Username),
        new Claim(ClaimTypes.Name, req.Username),
        new Claim(ClaimTypes.Role, "User")
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var expiresMinutes = int.TryParse(jwt["AccessTokenMinutes"], out var m) ? m : 60;

    var token = new JwtSecurityToken(
        issuer: jwt["Issuer"],
        audience: jwt["Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
        signingCredentials: creds
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
    return Results.Ok(new { token = tokenString });
})
.WithName("Login");

app.MapControllers();

await app.RunAsync();

namespace Galileo_API
{
    public record LoginRequest(string Username, string Password);
}
