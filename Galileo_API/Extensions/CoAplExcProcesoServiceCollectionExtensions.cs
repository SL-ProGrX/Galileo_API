using Galileo_API.Services.ProGrX.Cobros;

namespace Galileo_API.Extensions
{
    public static class CoAplExcProcesoServiceCollectionExtensions
    {
        public static IServiceCollection AddCoAplExcProceso(
           this IServiceCollection services)
        {
            services.AddSingleton<CoAplExcProcesoQueue>();
            services.AddHostedService<CoAplExcProcesoWorker>();

            return services;
        }
    }
}
