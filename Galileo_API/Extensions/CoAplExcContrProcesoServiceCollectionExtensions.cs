using Galileo_API.Services.ProGrX.Cobros;

namespace Galileo_API.Extensions
{
    public static class CoAplExcContrProcesoServiceCollectionExtensions
    {
        public static IServiceCollection AddCoAplExcContrProceso(
           this IServiceCollection services)
        {
            services.AddSingleton<CoAplExcContrProcesoQueue>();
            services.AddHostedService<CoAplExcContrProcesoWorker>();

            return services;
        }
    }
}
