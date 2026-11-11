using Galileo_API.Services.ProGrX.Cobros;

namespace Galileo_API.Extensions
{
    public static class CoCobroFiadoresProcesoServiceCollectionExtensions
    {
        public static IServiceCollection AddCoCobroFiadoresProceso(
           this IServiceCollection services)
        {
            services.AddSingleton<CoCobroFiadoresProcesoQueue>();
            services.AddHostedService<CoCobroFiadoresProcesoWorker>();

            return services;
        }
    }
}
