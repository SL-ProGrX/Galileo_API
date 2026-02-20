using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Nucleo;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Nucleo;

namespace Galileo.Controllers.ProGrX_Nucleo
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmSifEmisoresController : ControllerBase
    {
        private readonly FrmSifEmisoresBL _bl;

        public FrmSifEmisoresController(IConfiguration config)
        {
            _bl = new FrmSifEmisoresBL(config);
        }

        [Authorize]
        [HttpGet("SIF_EmisoresLista_Obtener")]
        public ActionResult<ErrorDto<SifEmisoresLista>> SIF_EmisoresLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.SIF_EmisoresLista_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpGet("SIF_Emisores_Obtener")]
        public ActionResult<ErrorDto<List<SifEmisoresData>>> SIF_Emisores_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.SIF_Emisores_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("SIF_Emisores_Guardar")]
        public ErrorDto SIF_Emisores_Guardar(int CodEmpresa, string usuario, [FromBody] SifEmisoresData emisor)
        {
            return _bl.SIF_Emisores_Guardar(CodEmpresa, usuario, emisor);
        }

        [Authorize]
        [HttpPost("SIF_Emisores_Valida")]
        public ErrorDto SIF_Emisores_Valida(int CodEmpresa, [FromBody] SifEmisoresData emisor)
        {
            return _bl.SIF_Emisores_Valida(CodEmpresa, emisor);
        }

        [Authorize]
        [HttpDelete("SIF_Emisores_Eliminar")]
        public ErrorDto SIF_Emisores_Eliminar(int CodEmpresa, string usuario, string cod_emisor)
        {
            return _bl.SIF_Emisores_Eliminar(CodEmpresa, usuario, cod_emisor);
        }

        [Authorize]
        [HttpGet("SIF_EmisoresTarjetas_Obtener")]
        public ActionResult<ErrorDto<List<SifTarjetasAsignadasData>>> SIF_EmisoresTarjetas_Obtener(int CodEmpresa, string cod_emisor)
        {
            return _bl.SIF_EmisoresTarjetas_Obtener(CodEmpresa, cod_emisor);
        }

        [Authorize]
        [HttpPost("SIF_EmisorTarjeta_Asignar")]
        public ErrorDto SIF_EmisorTarjeta_Asignar(int CodEmpresa, string usuario, [FromBody] SifEmisorTarjetaData asignacion)
        {
            return _bl.SIF_EmisorTarjeta_Asignar(CodEmpresa, usuario, asignacion);
        }

        [Authorize]
        [HttpPost("SIF_EmisorTarjeta_Desasignar")]
        public ErrorDto SIF_EmisorTarjeta_Desasignar(int CodEmpresa, string usuario, [FromBody] SifEmisorTarjetaData asignacion)
        {
            return _bl.SIF_EmisorTarjeta_Desasignar(CodEmpresa, usuario, asignacion);
        }
    }
}