using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Fondos;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmFndUsuariosAutorizadosController : ControllerBase
    {
        private readonly FrmFndUsuariosAutorizadosBl _bl;

        public FrmFndUsuariosAutorizadosController(IConfiguration config)
        {
            _bl = new FrmFndUsuariosAutorizadosBl(config);
        }

        [Authorize]
        [HttpGet("FndColaboradoresCc_Obtener")]
        public ErrorDto<List<FndColaboradoresCcData>> FndColaboradoresCc_Obtener(int CodEmpresa)
        {
            return _bl.FndColaboradoresCc_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("FndColaboradoresCc_Valida")]
        public ErrorDto FndColaboradoresCc_Valida(int CodEmpresa, string usuario)
        {
            return _bl.FndColaboradoresCc_Valida(CodEmpresa, usuario);
        }

        [Authorize]
        [HttpPost("FndColaboradoresCc_Guardar")]
        public ErrorDto FndColaboradoresCc_Guardar(int CodEmpresa, string usuarioLogueado, [FromBody] FndColaboradoresCcData colaborador)
        {
            return _bl.FndColaboradoresCc_Guardar(CodEmpresa, usuarioLogueado, colaborador);
        }

        [Authorize]
        [HttpDelete("FndColaboradoresCc_Eliminar")]
        public ErrorDto FndColaboradoresCc_Eliminar(int CodEmpresa, string usuario)
        {
            return _bl.FndColaboradoresCc_Eliminar(CodEmpresa, usuario);
        }
    }
}