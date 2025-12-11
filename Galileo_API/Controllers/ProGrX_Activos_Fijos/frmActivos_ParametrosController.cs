using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosParametrosController : ControllerBase
    {
        private readonly FrmActivosParametrosBL _bl;
        public FrmActivosParametrosController(IConfiguration config)
        {
            _bl = new FrmActivosParametrosBL(config);
        }

        [Authorize]
        [HttpGet("Activos_Parametros_Contabilidad_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Parametros_Contabilidad_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Parametros_Contabilidad_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Activos_Parametros_EstablecerMes")]
        public ErrorDto Activos_Parametros_EstablecerMes(int CodEmpresa, DateTime periodo)
        {
            return _bl.Activos_Parametros_EstablecerMes(CodEmpresa, periodo);
        }

        [Authorize]
        [HttpGet("Activos_Parametros_Consultar")]
        public ErrorDto<ActivosParametrosData?> Activos_Parametros_Consultar(int CodEmpresa)
        {
            return _bl.Activos_Parametros_Consultar(CodEmpresa);
        }

        [Authorize]
        [HttpPost("Activos_Parametros_Guardar")]
        public ErrorDto Activos_Parametros_Guardar(int CodEmpresa, string usuario, ActivosParametrosData datos)
        {
            return _bl.Activos_Parametros_Guardar(CodEmpresa, datos);
        }
    }
}