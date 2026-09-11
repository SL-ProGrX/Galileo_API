using Microsoft.AspNetCore.Builder;

namespace Galileo_API.Extensions;

public static class ApiHostApplicationBuilderExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        ConfigureOpenApi(app);
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors(ApiHostServiceCollectionExtensions.CorsPolicyName);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }

    private static void ConfigureOpenApi(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            return;
        }

        app.UseHsts();
    }
}
