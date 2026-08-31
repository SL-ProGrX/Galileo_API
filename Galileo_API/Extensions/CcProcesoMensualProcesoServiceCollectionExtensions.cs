using Galileo_API.Services.ProGrX_Procesos;

namespace Galileo_API.Extensions
{
    public static class CcProcesoMensualProcesoServiceCollectionExtensions
    {
        public static IServiceCollection AddCcProcesoMensualProceso(
           this IServiceCollection services)
        {
            services.AddSingleton<CcProcesoMensualProcesoQueue>();
            services.AddHostedService<CcProcesoMensualProcesoWorker>();

            return services;
        }
    }
}
