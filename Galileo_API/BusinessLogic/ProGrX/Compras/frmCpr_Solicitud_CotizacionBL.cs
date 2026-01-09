using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprSolicitudCotizacionBL
    {
        private readonly FrmCprSolicitudCotizacionDB _db;

        public FrmCprSolicitudCotizacionBL(IConfiguration config)
        {
            _db = new FrmCprSolicitudCotizacionDB(config);
        }

        public ErrorDto CprSolicitudContizacionBs_Guardar(int CodEmpresa, CprSolicitusCotizacionGuardar datos)
        {
            return _db.CprSolicitudContizacionBs_Guardar(CodEmpresa, datos);
        }

        public ErrorDto CprSolicitudCotizacionBs_Eliminar(int CodEmpresa, int id_cotizacion_linea)
        {
            return _db.CprSolicitudCotizacionBs_Eliminar(CodEmpresa, id_cotizacion_linea);
        }

        public ErrorDto<List<CprSolicitudProvCotiza>> CprSolicitudContizacionLista_Obtener(int CodEmpresa, int cpr_id, string cod_proveedor)
        {
            return _db.CprSolicitudContizacionLista_Obtener(CodEmpresa, cpr_id, cod_proveedor);
        }

    }
}