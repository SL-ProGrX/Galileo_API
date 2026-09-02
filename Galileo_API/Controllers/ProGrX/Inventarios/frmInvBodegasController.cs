using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvBodegasController : ControllerBase
    {
        private readonly FrmInvBodegasBl _bl;

        public FrmInvBodegasController(IConfiguration config)
        {
            _bl = new FrmInvBodegasBl(config);
        }

        [HttpGet("INV_Bodegas_Lista_Obtener")]
        public ErrorDto<List<BodegasDto>> INV_Bodegas_Lista_Obtener(
            int CodEmpresa)
        {
            return _bl.INV_Bodegas_Lista_Obtener(
                CodEmpresa);
        }

        [HttpGet("INV_Bodegas_Codigo_Obtener")]
        public ErrorDto<BodegasDto> INV_Bodegas_Codigo_Obtener(
            int CodEmpresa,
            string cod_bodega)
        {
            return _bl.INV_Bodegas_Codigo_Obtener(
                CodEmpresa,
                cod_bodega);
        }

        [HttpGet("INV_Bodegas_Navegacion_Obtener")]
        public ErrorDto<BodegasDto> INV_Bodegas_Navegacion_Obtener(
            int CodEmpresa,
            string consecutivo,
            string tipo)
        {
            return _bl.INV_Bodegas_Navegacion_Obtener(
                CodEmpresa,
                consecutivo,
                tipo);
        }

        [HttpGet("INV_Bodegas_Permisos_Obtener")]
        public ErrorDto<List<PermisosBodegasDto>> INV_Bodegas_Permisos_Obtener(
            int CodEmpresa,
            string cod_bodega,
            string tipo_transaccion)
        {
            return _bl.INV_Bodegas_Permisos_Obtener(
                CodEmpresa,
                cod_bodega,
                tipo_transaccion);
        }

        [HttpPost("INV_Bodegas_Registrar")]
        public ErrorDto INV_Bodegas_Registrar(
            int CodEmpresa,
            [FromBody] BodegasDto request)
        {
            return _bl.INV_Bodegas_Registrar(
                CodEmpresa,
                request);
        }

        [HttpPut("INV_Bodegas_Actualizar")]
        public ErrorDto INV_Bodegas_Actualizar(
            int CodEmpresa,
            [FromBody] BodegasDto request)
        {
            return _bl.INV_Bodegas_Actualizar(
                CodEmpresa,
                request);
        }

        [HttpDelete("INV_Bodegas_Eliminar")]
        public ErrorDto INV_Bodegas_Eliminar(
            int CodEmpresa,
            string cod_bodega)
        {
            return _bl.INV_Bodegas_Eliminar(
                CodEmpresa,
                cod_bodega);
        }

        [HttpPut("INV_Bodegas_Permiso_Actualizar")]
        public ErrorDto INV_Bodegas_Permiso_Actualizar(
            int CodEmpresa,
            [FromBody] InvBodegasPermisoActualizarRequest request)
        {
            return _bl.INV_Bodegas_Permiso_Actualizar(
                CodEmpresa,
                request);
        }
    }
}