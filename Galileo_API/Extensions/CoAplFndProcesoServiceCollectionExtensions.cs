using Galileo_API.Services.ProGrX.Cobros;

namespace Galileo_API.Extensions
{
    public static class CoAplFndProcesoServiceCollectionExtensions
    {
        public static IServiceCollection AddCoAplFndProceso(
           this IServiceCollection services)
        {
            services.AddSingleton<CoAplFndProcesoQueue>();
            services.AddHostedService<CoAplFndProcesoWorker>();

            return services;
        }
    }
}
