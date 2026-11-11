using Galileo_API.Services.ProGrX.Cobros;

namespace Galileo_API.Extensions
{
    public static class CoAplFndContrProcesoServiceCollectionExtensions
    {
        public static IServiceCollection AddCoAplFndContrProceso(
           this IServiceCollection services)
        {
            services.AddSingleton<CoAplFndContrProcesoQueue>();
            services.AddHostedService<CoAplFndContrProcesoWorker>();

            return services;
        }
    }
}
