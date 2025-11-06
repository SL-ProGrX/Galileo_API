using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Galileo_API;
using System.Text.Json;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger + Bearer
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMemoryCache();

builder.Services.AddMvcCore()
    .AddAuthorization();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new() { Title = "Galileo API", Version = "v2", Description = "API para gestión de Galileo" });

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
var keyString = builder.Configuration["Jwt:Secret"]; // viene de user-secrets (dev) o env var Jwt__Secret (prod)
if (string.IsNullOrWhiteSpace(keyString))
    throw new InvalidOperationException("Jwt:Secret no está configurada. Define la key con 'dotnet user-secrets set \"Jwt:Secret\" \"...\"' en dev, o como variable Jwt__Secret en prod.");

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
            OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Token inválido: " + context.Exception.Message);
            return Task.CompletedTask;
        },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token válido: " + context.SecurityToken);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    error = "Token inválido o no autorizado"
                });

                return context.Response.WriteAsync(result);
            }

        };
    });

string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        var devOrigins = new[]
        {
            "http://localhost:4200",
            "http://localhost:4201",
            "http://localhost:4202",
            "http://localhost:61968",
            "http://localhost:61969"
        };

        var prodOrigins = new[]
        {
            "https://progrxpruebas.aseccss.com",
            "https://progrxweb.com"
        };

        policy.SetIsOriginAllowed(origin =>
        {
            if (builder.Environment.IsDevelopment())
                return devOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);

            return prodOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Establecer la cultura global
var cultureInfo = new CultureInfo("en-US"); // Cambia a "en-US" para MM/dd/yyyy
cultureInfo.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy"; // Configura el patr�n de fecha
cultureInfo.DateTimeFormat.LongTimePattern = "HH:mm:ss"; // Configura el patr�n de hora

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(MyAllowSpecificOrigins);

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

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["Jwt:Secret"]!));
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
