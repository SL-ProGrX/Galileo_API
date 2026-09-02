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
    public sealed class FrmInvUnidadesController : ControllerBase
    {
        private readonly FrmInvUnidadesBl _bl;

        public FrmInvUnidadesController(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _bl = new FrmInvUnidadesBl(config);
        }

        [HttpGet("INV_Unidades_Lista_Obtener")]
        public ErrorDto<UnidadesDataLista>
            INV_Unidades_Lista_Obtener(
                int CodEmpresa,
                string? filtros)
        {
            return _bl.INV_Unidades_Lista_Obtener(
                CodEmpresa,
                filtros);
        }

        [HttpGet("INV_Unidades_Detalle_Obtener")]
        [HttpGet("UnidadMedicion_ObtenerTodosDetalle")]
        public ErrorDto<List<UnidadMedicionDto>>
            INV_Unidades_Detalle_Obtener(int CodEmpresa)
        {
            return _bl.INV_Unidades_Detalle_Obtener(
                CodEmpresa);
        }

        [HttpGet("INV_Unidades_Catalogo_Obtener")]
        public ErrorDto<List<UnidadMedicion>>
            INV_Unidades_Catalogo_Obtener(int CodEmpresa)
        {
            return _bl.INV_Unidades_Catalogo_Obtener(CodEmpresa);
        }

        [HttpPost("INV_Unidades_Registrar")]
        public ErrorDto INV_Unidades_Registrar(
            int CodEmpresa,
            UnidadMedicionDto request)
        {
            return _bl.INV_Unidades_Registrar(
                CodEmpresa,
                request);
        }

        [HttpPut("INV_Unidades_Actualizar")]
        public ErrorDto INV_Unidades_Actualizar(
            int CodEmpresa,
            UnidadMedicionDto request)
        {
            return _bl.INV_Unidades_Actualizar(
                CodEmpresa,
                request);
        }

        [HttpDelete("INV_Unidades_Eliminar")]
        public ErrorDto INV_Unidades_Eliminar(
            int CodEmpresa,
            string? unidad,
            string? usuario)
        {
            return _bl.INV_Unidades_Eliminar(
                CodEmpresa,
                unidad,
                usuario);
        }
    }
}