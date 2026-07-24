using Galileo.Models.TES;
using Galileo_API.Services.ProGrX.Bancos;

namespace Galileo_API.Extensions
{
    public static class TesEmisionDocumentosServiceCollectionExtensions
    {
        public static IServiceCollection AddTesEmisionDocumentosProceso(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<TesEmisionDocumentosProcesoOptions>(
                configuration.GetSection("TES_EmisionDocumentos"));
            services.AddSingleton<TesEmisionDocumentosProcesoQueue>();
            services.AddHostedService<TesEmisionDocumentosProcesoWorker>();
            return services;
        }
    }
}
