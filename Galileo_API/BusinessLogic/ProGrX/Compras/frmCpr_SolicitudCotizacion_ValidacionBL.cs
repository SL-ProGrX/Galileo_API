using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic
{
    public class FrmCprSolicitudCotizacionValidacionBL
    {
        private readonly FrmCprSolicitudCotizacionValidacionDB _db;

        public FrmCprSolicitudCotizacionValidacionBL(IConfiguration config)
        {
            _db = new FrmCprSolicitudCotizacionValidacionDB(config);
        }

        public ErrorDto<CprSolicitudCotizacionPrvBsLista> CprValidarCotizacionBs_Obtener(int CodEmpresa, int? cpr_id, int? cod_unidad)
        {
            return _db.CprValidarCotizacionBs_Obtener(CodEmpresa, cpr_id, cod_unidad);
        }

        public ErrorDto CprValidarContizacionBs_Guardar(int CodEmpresa, CprSolicitusCotizacionGuardar datos)
        {
            return _db.CprValidarContizacionBs_Guardar(CodEmpresa, datos);
        }

        /// <summary>Desmarca como seleccionada la línea de cotización indicada.</summary>
        public ErrorDto CprValidacionCotizacionBs_Eliminar(
            int CodEmpresa,
            int id_cotizacion_linea,
            int? cpr_id,
            int? proveedor_codigo,
            string? codigo,
            string? cod_producto)
        {
            return _db.CprValidacionCotizacionBs_Eliminar(
                CodEmpresa,
                id_cotizacion_linea,
                cpr_id,
                proveedor_codigo,
                codigo,
                cod_producto);
        }
    }
}
