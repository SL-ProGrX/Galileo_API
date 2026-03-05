using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessTier.ProGrX.Cobros
{
    public class FrmCOReadecuacionCambioOperacionBL
    {
        private readonly FrmCOReadecuacionCambioOperacionDB Db;

        public FrmCOReadecuacionCambioOperacionBL(IConfiguration config)
        {
            Db = new FrmCOReadecuacionCambioOperacionDB(config);
        }

        public ErrorDto<CoReadecuacionCambioOperacionObtenerResponse> CO_ReadecuacionCambioOperacion_Obtener(int CodEmpresa, int idTramite)
        {
            return Db.CO_ReadecuacionCambioOperacion_Obtener(CodEmpresa, idTramite);
        }
        public ErrorDto<CoReadecuacionCambioOperacionAplicarResponse> CO_ReadecuacionCambioOperacion_Aplicar(int CodEmpresa, CoReadecuacionCambioOperacionAplicarRequest req)
        {
        return Db.CO_ReadecuacionCambioOperacion_Aplicar(CodEmpresa, req);
        }
        public ErrorDto<CoReadecuacionReporteOperacionNuevaDto> CO_Readecuacion_ReporteOperacionNueva_Obtener(int CodEmpresa, CoReadecuacionReporteOperacionNuevaRequest req)
        {
            return Db.CO_Readecuacion_ReporteOperacionNueva_Obtener(CodEmpresa, req);
        }
    }
}