using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Galileo_API.BusinessLogic.TES;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace PgxAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesBitacoraEspecialController : ControllerBase
    {
        private readonly FrmTesBitacoraEspecialBL _bl;

        public FrmTesBitacoraEspecialController(IConfiguration config)
        {
            _bl = new FrmTesBitacoraEspecialBL(config);
        }

        
        [HttpGet("TES_Bancos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Bancos_Obtener(int CodEmpresa)
        {
            return _bl.TES_Bancos_Obtener(CodEmpresa);
        }

        [HttpGet("TES_Tipos_Doc_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Tipos_Doc_Obtener(int CodEmpresa)
        {
            return _bl.TES_Tipos_Doc_Obtener(CodEmpresa);
        }

        [HttpGet("TES_Tipos_Movimientos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TES_Tipos_Movimientos_Obtener(int CodEmpresa)
        {
            return _bl.TES_Tipos_Movimientos_Obtener(CodEmpresa);
        }

        [HttpPost("BitacoraEspecial_Buscar")]
        public ErrorDto<List<BitacoraEspecialDto>> BitacoraEspecial_Buscar(int codEmpresa, [FromBody] FiltrosBitacoraEspecial filtros)
        {
            return _bl.BitacoraEspecial_Buscar(codEmpresa, filtros);
        }

        [HttpPost("TES_Historial_Actualizar")]
        public ErrorDto TES_Historial_Actualizar(int codEmpresa, string id, string usuario, string nsolicitud)
        {
            return _bl.TES_Historial_Actualizar(codEmpresa, id, usuario, nsolicitud);
        }
    }
}