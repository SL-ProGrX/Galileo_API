using System.Text;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Galileo_API;
using System.Text.Json;
using System.Globalization;
using Galileo.DataBaseTier;
using Microsoft.OpenApi;
using Galileo_API.Extensions ;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;


// ✅ Asegúrate que este using apunte al namespace real donde está tu filtro
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCcProcesoMensualArchivos();
builder.Services.AddCcProcesoMensualProceso();
builder.Services.AddTesEmisionDocumentosProceso(builder.Configuration);
builder.Services.Configure<ArchivosGeneradosOptions>(
builder.Configuration.GetSection("ArchivosGenerados"));

// ✅ Cargar configuración externa desde APP_CONFIG_PATH (definida en web.config/IIS)
var externalConfigPath = Environment.GetEnvironmentVariable("APP_CONFIG_PATH");

if (!string.IsNullOrWhiteSpace(externalConfigPath))
{
    var envName = builder.Environment.EnvironmentName; // Production, Staging, etc.
    var fileName = $"appsettings.{envName}.json";
    var safeFileName = Path.GetFileName(fileName);
    var externalSettingsFile = Path.Combine(externalConfigPath, safeFileName);

    builder.Configuration.AddJsonFile(externalSettingsFile, optional: false, reloadOnChange: true);
}

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

const string bearerScheme = "Bearer";

builder.Services.AddSwaggerGen(c =>
{
    var xmlDocFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var safeXmlDocFileName = Path.GetFileName(xmlDocFileName);
    if (Path.IsPathRooted(safeXmlDocFileName))
        throw new InvalidOperationException("XML documentation file name must be relative.");

    c.IncludeXmlComments(Path.Join(
        AppContext.BaseDirectory,
        safeXmlDocFileName));

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Galileo API",
        Version = "v1",
        Description = "API para gestión de Galileo"
    });

    c.AddSecurityDefinition(bearerScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Ingresa: Bearer {tu_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(bearerScheme)] = new List<string>()
    });

    c.CustomSchemaIds(type => type.FullName);
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
var keyString = Environment.GetEnvironmentVariable("Jwt__Secret");
if (string.IsNullOrWhiteSpace(keyString))
{
    // Compatibilidad con User Secrets y con la configuración externa de producción.
    var configuredSecret = builder.Configuration["Jwt:Secret"];
    if (!string.IsNullOrWhiteSpace(configuredSecret))
    {
        Environment.SetEnvironmentVariable("Jwt__Secret", configuredSecret);
        keyString = Environment.GetEnvironmentVariable("Jwt__Secret");
    }

    if (string.IsNullOrWhiteSpace(keyString))
        throw new InvalidOperationException("Jwt:Secret no está configurada. Define la variable de entorno Jwt__Secret.");
}

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
            "http://localhost:4300",
            "http://localhost:4301",
            "http://localhost:61968",
            "http://localhost:61969",
            "https://progrxpruebas.aseccss.com",
            "https://progrxweb.com"
        };

        public static readonly HashSet<string> Prod = new(StringComparer.OrdinalIgnoreCase)
        {
            "https://progrxpruebas.aseccss.com",
            "https://progrxweb.com"
        };
    }
}
