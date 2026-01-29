using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesParametrosController : ControllerBase
    {
        private readonly FrmTesParametrosBL _parametrosBL;

        public FrmTesParametrosController(IConfiguration config)
        {
            _parametrosBL = new FrmTesParametrosBL(config);
        }

        
        [HttpGet("TES_Parametros_Obtener")]
        public ErrorDto<TablasListaGenericaModel> TES_Parametros_Obtener(int CodEmpresa, string filtros)
        {
            return _parametrosBL.TES_Parametros_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("TES_Parametros_Guardar")]
        public ErrorDto TES_Parametros_Guardar(int CodEmpresa, string Usuario, string Parametros)
        {
            return _parametrosBL.TES_Parametros_Guardar(CodEmpresa, Usuario, Parametros);
        }
    }
}