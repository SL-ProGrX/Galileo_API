using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Galileo.BusinessLogic.Auth;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo_API;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

namespace Galileo_API.Extensions;

public static class ApiHostServiceCollectionExtensions
{
    public const string CorsPolicyName = "_myAllowSpecificOrigins";

    public static IConfigurationManager AddExternalSettings(
        this IConfigurationManager configuration,
        IWebHostEnvironment environment)
    {
        var externalConfigPath = Environment.GetEnvironmentVariable("APP_CONFIG_PATH");
        if (string.IsNullOrWhiteSpace(externalConfigPath))
        {
            return configuration;
        }

        var fileName = Path.GetFileName($"appsettings.{environment.EnvironmentName}.json");
        configuration.AddJsonFile(
            Path.Combine(externalConfigPath, fileName),
            optional: false,
            reloadOnChange: true);

        return configuration;
    }

    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCcProcesoMensualArchivos();
        services.AddCcProcesoMensualProceso();
        services.AddTesEmisionDocumentosProceso(configuration);
        services.Configure<ArchivosGeneradosOptions>(
            configuration.GetSection("ArchivosGenerados"));
        services.AddMemoryCache();
        services.AddSingleton<AuthSessionStore>();
        services.AddSingleton<AccessTokenService>();
        services.AddScoped<AuthBL>();
        services.AddScoped<EmpresaAccessFilter>();
        services.AddControllers(options => options.Filters.AddService<EmpresaAccessFilter>());
        services.AddAuthorization();
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 20;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueLimit = 0;
            });
        });

        return services;
    }

    public static void EnsureDevelopmentJwtSecret(
        this IConfigurationManager configuration,
        IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment() ||
            !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("Jwt__Secret")))
        {
            return;
        }

        configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);
        var configuredSecret = configuration["Jwt:Secret"];
        if (!string.IsNullOrWhiteSpace(configuredSecret))
        {
            Environment.SetEnvironmentVariable("Jwt__Secret", configuredSecret);
        }
    }

    public static IServiceCollection AddApiDocumentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSwaggerGen(options =>
        {
            var xmlDocFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var safeXmlDocFileName = Path.GetFileName(xmlDocFileName);
            if (Path.IsPathRooted(safeXmlDocFileName))
            {
                throw new InvalidOperationException("XML documentation file name must be relative.");
            }

            options.IncludeXmlComments(Path.Join(AppContext.BaseDirectory, safeXmlDocFileName));
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Galileo API",
                Version = "v1",
                Description = "API para gestión de Galileo"
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Ingresa: Bearer {tu_token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer")] = new List<string>()
            });
            options.CustomSchemaIds(type => type.FullName);
        });
        services.AddHsts(options =>
        {
            options.Preload = true;
            options.IncludeSubDomains = true;
            options.MaxAge = TimeSpan.FromDays(365);
        });

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var keyString = Environment.GetEnvironmentVariable("Jwt__Secret");
        if (string.IsNullOrWhiteSpace(keyString))
        {
            throw new InvalidOperationException(
                "Jwt:Secret no está configurada. Define la variable de entorno Jwt__Secret.");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        services
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
                    OnAuthenticationFailed = _ => Task.CompletedTask,
                    OnTokenValidated = context =>
                    {
                        var sessionId = context.Principal?.FindFirst("sid")?.Value;
                        var sessions = context.HttpContext.RequestServices.GetRequiredService<AuthSessionStore>();
                        if (string.IsNullOrWhiteSpace(sessionId) || !sessions.IsSessionActive(sessionId))
                        {
                            context.Fail("La sesión ya no está activa.");
                        }

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

        return services;
    }

    public static IServiceCollection AddApiCors(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, policy =>
            {
                policy.SetIsOriginAllowed(origin => IsAllowedOrigin(environment, origin))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static void ConfigureDefaultCulture()
    {
        var cultureInfo = new CultureInfo("en-US");
        cultureInfo.DateTimeFormat.ShortDatePattern = "MM/dd/yyyy";
        cultureInfo.DateTimeFormat.LongTimePattern = "HH:mm:ss";
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
    }

    private static bool IsAllowedOrigin(IWebHostEnvironment environment, string origin)
    {
        return environment.IsDevelopment()
            ? CorsOrigins.Dev.Contains(origin)
            : CorsOrigins.Prod.Contains(origin);
    }
}
