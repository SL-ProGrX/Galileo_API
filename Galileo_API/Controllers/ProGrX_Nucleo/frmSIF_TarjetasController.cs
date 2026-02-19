using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;
using Galileo.Models;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifTarjetasController : ControllerBase
    {
        private readonly FrmSifTarjetasBL _bl;

        public FrmSifTarjetasController(IConfiguration config)
        {
            _bl = new FrmSifTarjetasBL(config);
        }

        [Authorize]
        [HttpGet("SIF_TarjetasLista_Obtener")]
        public ActionResult<ErrorDto<SifTarjetasLista>> SIF_TarjetasLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.SIF_TarjetasLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("SIF_Tarjetas_Obtener")]
        public ActionResult<ErrorDto<List<SifTarjetasData>>> SIF_Tarjetas_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.SIF_Tarjetas_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("SIF_Tarjetas_Guardar")]
        public ErrorDto SIF_Tarjetas_Guardar(int CodEmpresa, string usuario, [FromBody] SifTarjetasData tarjeta)
        {
            return _bl.SIF_Tarjetas_Guardar(CodEmpresa, usuario, tarjeta);
        }

        [Authorize]
        [HttpDelete("SIF_Tarjetas_Eliminar")]
        public ErrorDto SIF_Tarjetas_Eliminar(int CodEmpresa, string usuario, string cod_tarjeta)
        {
            return _bl.SIF_Tarjetas_Eliminar(CodEmpresa, usuario, cod_tarjeta);
        }

        [Authorize]
        [HttpPost("SIF_Tarjetas_Valida")]
        public ErrorDto SIF_Tarjetas_Valida(int CodEmpresa, [FromBody] SifTarjetasData tarjeta)
        {
            return _bl.SIF_Tarjetas_Valida(CodEmpresa, tarjeta);
        }

        [Authorize]
        [HttpGet("SIF_TarjetasEmisores_Obtener")]
        public ActionResult<ErrorDto<List<SifEmisoresAsignadosData>>> SIF_TarjetasEmisores_Obtener(int CodEmpresa, string cod_tarjeta)
        {
            var result = _bl.SIF_TarjetasEmisores_Obtener(CodEmpresa, cod_tarjeta);
            return Ok(result);
        }
    }
}