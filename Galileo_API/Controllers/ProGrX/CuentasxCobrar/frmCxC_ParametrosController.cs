
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.CuentasxCobrar;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc; 

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCxCParametrosController : ControllerBase
    {
        private readonly FrmCxCParametrosBL _bl;

        public FrmCxCParametrosController(IConfiguration config)
            => _bl = new FrmCxCParametrosBL(config);

        [Authorize]
        [HttpGet("CxCParametrosLista_Obtener")]
        public ErrorDto<CxCParametrosLista> CxCParametrosLista_Obtener(int CodEmpresa, int codContabilidad, string filtros, bool esExportar)
        {
            return _bl.CxCParametrosLista_Obtener(CodEmpresa, codContabilidad, filtros, esExportar);
        }

        [Authorize]
        [HttpPost("CxCParametros_Guardar")]
        public ErrorDto CxCParametros_Guardar(int CodEmpresa, string usuario, string valor, string codParametro)
        {
            return _bl.CxCParametros_Guardar(CodEmpresa, usuario, valor, codParametro);

        }
    }

}