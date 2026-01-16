using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Compras;
using Galileo.Models.CPR;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class FrmCprSolicitudCotizaValoraController : ControllerBase
    {
        private readonly FrmCprSolicitudCotizaValoraBL _bl;
        public FrmCprSolicitudCotizaValoraController(IConfiguration config)
        {
            _bl = new FrmCprSolicitudCotizaValoraBL(config);
        }

        [HttpGet("CprSolicitudProveedoresLista_Obtener")]
        public ErrorDto<List<CprValoracionLista>> CprSolicitudProveedoresLista_Obtener(int CodEmpresa, int consulta, int cpr_id)
        {
            return _bl.CprSolicitudProveedoresLista_Obtener(CodEmpresa, consulta, cpr_id);
        }

        [HttpPost("CprSolicitudProveedor_Invitar")]
        public ErrorDto CprSolicitudProveedor_Invitar(int CodEmpresa, CprSolicitudProvDto proveedor)
        {
            return _bl.CprSolicitudProveedor_Invitar(CodEmpresa, proveedor);
        }

        [HttpDelete("CprSolicitudProveedor_Eliminar")]
        public ErrorDto CprSolicitudProveedor_Eliminar(int CodEmpresa, int proveedor_codigo, int cpr_id)
        {
            return _bl.CprSolicitudProveedor_Eliminar(CodEmpresa, proveedor_codigo, cpr_id);
        }

        [HttpGet("CprSolicitudProvInvitados_Obtener")]
        public ErrorDto<List<CprSolicitudProvDto>> CprSolicitudProvInvitados_Obtener(int CodEmpresa, int cpr_id)
        {
            return _bl.CprSolicitudProvInvitados_Obtener(CodEmpresa, cpr_id);
        }

        [HttpGet("CprSolicitudProvContizacionLista_Obtener")]
        public ErrorDto<List<CprSolicitudPrvBs>> CprSolicitudProvContizacionLista_Obtener(int CodEmpresa, int cpr_id, string cod_proveedor)
        {
            return _bl.CprSolicitudProvContizacionLista_Obtener(CodEmpresa, cpr_id, cod_proveedor);
        }

        [HttpGet("CprSolicitudProvValItemData_Obtener")]
        public ErrorDto<List<CprSolicitudProvValItemData>> CprSolicitudProvValItemData_Obtener(
            int CodEmpresa, 
            string parametros)
        {
            return _bl.CprSolicitudProvValItemData_Obtener(CodEmpresa, parametros);
        }

        [HttpPost("CprSolicitudProvCotizacion_Enviar")]
        public Task<ErrorDto> CprSolicitudProvCotizacion_Enviar(int CodEmpresa, int cpr_id, string cod_proveedor)
        {
            return _bl.CprSolicitudProvCotizacion_Enviar(CodEmpresa, cpr_id, cod_proveedor);
        }

       
        [HttpPost("CprSolicitudValoracion_Guardar")]
        public ErrorDto CprSolicitudValoracion_Guardar(int CodEmpresa, CprSolicitusValoracionGuardar datos)
        {
            return _bl.CprSolicitudValoracion_Guardar(CodEmpresa, datos);
        }

    }
}