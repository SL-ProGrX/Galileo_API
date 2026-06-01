
using Galileo_API.BusinessLogic.ProGrX_Procesos.frmCC_ProcesoMensualBL;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos;

namespace Galileo_API.Extensions
{
    public static class CcProcesoMensualArchivoServiceCollectionExtensions
    {
        public static IServiceCollection AddCcProcesoMensualArchivos(
           this IServiceCollection services)
        {
            services.AddScoped<CcProcesoMensualArchivosBL>();

            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF00ExcelGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF01CcssGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF02IntegraGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF03AseccssGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF03SifGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF04IceAcostelGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF05CoopeCajaGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF05CoopeCajaOldGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF06IceCentralGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF07IceProyectosGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF08AyaGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF09SpaGenerator>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF10SifIndefinidosGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF12ImasGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF13InaGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF14MsjGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF15PjGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF16StarHGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF17UcrGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF18ConaviGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF19CgrGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF20CenCenaiGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF21UnateprotGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF22PaniGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF23CorreosGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF24ServiCoopGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF25HolcimGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF26JupemaGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF27RecopeGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF28TekExpertsGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF29PygGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF31ExcelPorzaCashGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF32DxCGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF34AsoeCorrGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF35ProGrXRrhhGenerar>();
            services.AddScoped<ICcProcesoMensualArchivoGenerator, CcProcesoMensualArchivoF36AsoInsvaGenerar>(); 
            return services;
        }
    }
}
