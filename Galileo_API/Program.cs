using System.Net;
using Galileo_API.Extensions;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddExternalSettings(builder.Environment);
builder.Configuration.EnsureDevelopmentJwtSecret(builder.Environment);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = builder.Configuration.GetValue<int?>("ForwardedHeaders:ForwardLimit") ?? 1;
    if (options.ForwardLimit <= 0)
        throw new InvalidOperationException("ForwardedHeaders:ForwardLimit debe ser mayor que cero.");

    var knownProxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
    foreach (var knownProxy in knownProxies)
    {
        if (!IPAddress.TryParse(knownProxy, out var proxyAddress))
            throw new InvalidOperationException($"La IP '{knownProxy}' configurada en ForwardedHeaders:KnownProxies no es válida.");
        options.KnownProxies.Add(proxyAddress);
    }
});
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddApiDocumentation(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiCors(builder.Environment);
ApiHostServiceCollectionExtensions.ConfigureDefaultCulture();

var app = builder.Build();
app.UseApiPipeline();

await app.RunAsync();
