using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.BusinessLogic.ProGrX.Creditos
{
    public class FrmCrRetencionDeduccionesBl
    {
        private readonly FrmCrRetencionDeduccionesDb _db;

        public FrmCrRetencionDeduccionesBl(IConfiguration config)
        {
            _db = new FrmCrRetencionDeduccionesDb(config);
        }

        public ErrorDto<CrRetencionDeduccionesPantallaData> Cr_RetencionDeducciones_Pantalla_Obtener(int codEmpresa, string usuario)
            => _db.Cr_RetencionDeducciones_Pantalla_Obtener(codEmpresa, usuario);

        public ErrorDto<CrRetencionDeduccionesResultadoData> Cr_RetencionDeducciones_Obtener(
            int codEmpresa,
            CrRetencionDeduccionesObtenerRequest request)
            => _db.Cr_RetencionDeducciones_Obtener(codEmpresa, request);

        public ErrorDto<CrRetencionDeduccionesArchivoData> Cr_RetencionDeducciones_Archivo_Generar(
            int codEmpresa,
            CrRetencionDeduccionesArchivoRequest request)
            => _db.Cr_RetencionDeducciones_Archivo_Generar(codEmpresa, request);
    }
}