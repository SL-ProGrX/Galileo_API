using Galileo.DataBaseTier;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Compras
{
    public class FrmCprSolicitudCotizaValoraBL
    {
        private readonly FrmCprSolicitudCotizaValoraDB _db;

        public FrmCprSolicitudCotizaValoraBL(IConfiguration config)
        {
            _db = new FrmCprSolicitudCotizaValoraDB(config);
        }

        public ErrorDto<List<CprValoracionLista>> CprSolicitudProveedoresLista_Obtener(int CodEmpresa, int consulta, int cpr_id)
        {
            return _db.CprSolicitudProveedoresLista_Obtener(CodEmpresa, consulta, cpr_id);
        }

        public ErrorDto CprSolicitudProveedor_Invitar(int CodEmpresa, CprSolicitudProvDto proveedor)
        {
            return _db.CprSolicitudProveedor_Invitar(CodEmpresa, proveedor);
        }

        public ErrorDto CprSolicitudProveedor_Eliminar(int CodEmpresa, int proveedor_codigo, int cpr_id)
        {
            return _db.CprSolicitudProveedor_Eliminar(CodEmpresa, proveedor_codigo, cpr_id);
        }

        public ErrorDto<List<CprSolicitudProvDto>> CprSolicitudProvInvitados_Obtener(int CodEmpresa, int cpr_id)
        {
            return _db.CprSolicitudProvInvitados_Obtener(CodEmpresa, cpr_id);
        }

        public ErrorDto<List<CprSolicitudPrvBs>> CprSolicitudProvContizacionLista_Obtener(int CodEmpresa, int cpr_id, string cod_proveedor)
        {
            return _db.CprSolicitudProvContizacionLista_Obtener(CodEmpresa, cpr_id, cod_proveedor);
        }
        public ErrorDto<List<CprSolicitudProvValItemData>> CprSolicitudProvValItemData_Obtener(int CodEmpresa, string parametros)
        {
            return _db.CprSolicitudProvValItemData_Obtener(CodEmpresa, parametros);
        }

        public Task<ErrorDto> CprSolicitudProvCotizacion_Enviar(int CodEmpresa, int cpr_id, string cod_proveedor)
        {
            return _db.CprSolicitudProvCotizacion_Enviar(CodEmpresa, cpr_id, cod_proveedor);
        }

        public ErrorDto CprSolicitudValoracion_Guardar(int CodEmpresa, CprSolicitusValoracionGuardar datos)
        {
            return _db.CprSolicitudValoracion_Guardar(CodEmpresa, datos);
        }

      
    }
}