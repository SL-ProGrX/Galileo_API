using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.BusinessTier.ProGrX.Cobros
{
    public class frmCO_ReadecuacionCambioOperacionBL
    {
        private readonly frmCO_ReadecuacionCambioOperacionDB _db;

        public frmCO_ReadecuacionCambioOperacionBL(frmCO_ReadecuacionCambioOperacionDB db)
        {
            _db = db;
        }

        public ErrorDto<CoReadecuacionCambioOperacionObtenerResponse> CO_ReadecuacionCambioOperacion_Obtener(int CodEmpresa, int idTramite)
        {
            return _db.CO_ReadecuacionCambioOperacion_Obtener(CodEmpresa, idTramite);
        }
        public ErrorDto<CoReadecuacionCambioOperacionAplicarResponse> CO_ReadecuacionCambioOperacion_Aplicar(int CodEmpresa, CoReadecuacionCambioOperacionAplicarRequest req)
        {
        return _db.CO_ReadecuacionCambioOperacion_Aplicar(CodEmpresa, req);
        }    
    }
}