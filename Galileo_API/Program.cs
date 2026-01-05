using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Galileo_API;
using System.Text.Json;
using System.Globalization;
using Galileo.DataBaseTier;

// ✅ Asegúrate que este using apunte al namespace real donde está tu filtro
var builder = WebApplication.CreateBuilder(args);

// ✅ Registrar MemoryCache (si luego cacheas permisos)
builder.Services.AddMemoryCache();

// ✅ Registrar el filtro como servicio
builder.Services.AddScoped<EmpresaAccessFilter>();

// ✅ Controllers + filtro global (NO dupliques AddControllers en otro lado)
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<EmpresaAccessFilter>();
});

// ✅ Authorization (esto reemplaza tu AddMvcCore().AddAuthorization())
builder.Services.AddAuthorization();

// Swagger + Bearer
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Galileo API", Version = "v1", Description = "API para gestión de Galileo" });

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

// ✅ HSTS
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// === JWT Auth ===
var jwtSection = builder.Configuration.GetSection("Jwt");
var keyString = builder.Configuration["Jwt:Secret"]; // user-secrets (dev) o env var Jwt__Secret (prod)
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

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            if (builder.Environment.IsDevelopment())
                return CorsOrigins.Dev.Contains(origin);

            return CorsOrigins.Prod.Contains(origin);
        })
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Cultura global
var cultureInfo = new CultureInfo("en-US");
cultureInfo.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
cultureInfo.DateTimeFormat.LongTimePattern = "HH:mm:ss";

CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

// ✅ Orden recomendado
app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();

namespace Galileo_API
{
    public record LoginRequestTest(string Username, string Password);
}

// ======= Soporte CORS (estático) =======
namespace Galileo_API
{
    internal static class CorsOrigins
    {
        public static readonly HashSet<string> Dev = new(StringComparer.OrdinalIgnoreCase)
        {
            "http://localhost:4200",
            "http://localhost:4201",
            "http://localhost:4202",
            "http://localhost:61968",
            "http://localhost:61969"
        };

        public static readonly HashSet<string> Prod = new(StringComparer.OrdinalIgnoreCase)
        {
            "https://progrxpruebas.aseccss.com",
            "https://progrxweb.com"
        };
    }
}