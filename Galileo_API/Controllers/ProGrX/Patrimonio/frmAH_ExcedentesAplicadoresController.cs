using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Procesos;
using Galileo_API.BusinessLogic.ProGrX.Patrimonio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Patrimonio
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAHExcedentesAplicadoresController : ControllerBase
    {
        private readonly FrmAHExcedentesAplicadoresBL _bl;

        public FrmAHExcedentesAplicadoresController(IConfiguration config)
        {
            _bl = new FrmAHExcedentesAplicadoresBL(config);
        }

        [Authorize]
        [HttpGet("AH_Excedentes_Aplicadores_Lista_Obtener")]
        public ErrorDto<List<ExcedenteAplicadorDto>>AH_Excedentes_Aplicadores_Lista_Obtener(int CodEmpresa)
        {
            return _bl.AH_Excedentes_Aplicadores_Lista_Obtener(CodEmpresa);
        }
        [Authorize]
        [HttpPost("AH_Excedentes_Aplicadores_Guardar")]
        public ErrorDto AH_Excedentes_Aplicadores_Guardar(int CodEmpresa,string usuario, [FromBody] ExcedenteAplicadorDto aplicador)
        {
            return _bl.AH_Excedentes_Aplicadores_Guardar(CodEmpresa,usuario,aplicador);
        }
        [Authorize]
        [HttpDelete("AH_Excedentes_Aplicadores_Eliminar")]
        public ErrorDto AH_Excedentes_Aplicadores_Eliminar(int CodEmpresa,string usuarioAplicador,string usuario)
        {
            return _bl.AH_Excedentes_Aplicadores_Eliminar(CodEmpresa,usuarioAplicador,usuario);
        }
    }
}