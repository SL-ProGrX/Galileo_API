using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Microsoft.AspNetCore.Authorization;
using Galileo.Models.INV;

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

        [HttpGet("Autorizador_Obtener")]
        public ErrorDto<List<PermisosBodegasDto>> Autorizador_Obtener(int CodEmpresa, string CodBodega)
        {
            return _bl.Autorizador_ObtenerTodos(CodEmpresa, CodBodega);
        }


        [HttpGet("Bodegas_Obtener")]
        public ErrorDto<List<BodegasDto>> Bodegas_Obtener(int CodEmpresa)
        {
            return _bl.Bodegas_Obtener(CodEmpresa);
        }


        [HttpGet("ConsultaAscDesc")]
        public ErrorDto<BodegasDto> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            return _bl.ConsultaAscDesc(CodEmpresa, consecutivo, tipo);
        }


        [HttpGet("bodegaConsecutivo_Obtener")]
        public ErrorDto<BodegasDto> bodegaConsecutivo_Obtener(int CodEmpresa, string consecutivo)
        {
            return _bl.bodegaConsecutivo_Obtener(CodEmpresa, consecutivo);
        }


        [HttpPost("bodega_Insertar")]
        public ErrorDto bodega_Insertar(int CodEmpresa, BodegasDto data)
        {
            return _bl.bodega_Insertar(CodEmpresa, data);
        }

        [HttpPost("actualizar_Bodega")]
        public ErrorDto actualizar_Bodega(int CodEmpresa, BodegasDto request)
        {
            return _bl.bodega_Actualizar(CodEmpresa, request);
        }

        [HttpDelete("bodega_Eliminar")]
        public ErrorDto bodega_Eliminar(int CodEmpresa, string cod_bodega)
        {
            return _bl.bodega_Eliminar(CodEmpresa, cod_bodega);
        }

        [HttpPost("permisosBodega_Actualizar")]
        public ErrorDto permisosBodega_Actualizar(int CodEmpresa, PermisosBodegasDto request, string cod_bodega)
        {
            return _bl.permisosBodega_Actualizar(CodEmpresa, request, cod_bodega);
        }
    }
}