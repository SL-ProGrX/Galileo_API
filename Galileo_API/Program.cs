using Galileo_API.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddExternalSettings(builder.Environment);
builder.Configuration.EnsureDevelopmentJwtSecret(builder.Environment);
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddApiDocumentation(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiCors(builder.Environment);
ApiHostServiceCollectionExtensions.ConfigureDefaultCulture();

var app = builder.Build();
app.UseApiPipeline();

await app.RunAsync();
