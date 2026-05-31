using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFParametrosController : ControllerBase
    {
        private readonly FrmAFParametrosBL BL_AF_Parametros;

        public FrmAFParametrosController(IConfiguration config)
        {
            BL_AF_Parametros = new FrmAFParametrosBL(config);
        }

        [Authorize]
        [HttpGet("AF_Parametros_Obtener")]
        public ErrorDto<AfParametrosLista> AF_Parametros_Obtener(int CodEmpresa, string filtros)
        {
            return BL_AF_Parametros.AF_Parametros_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_Parametros_Actualizar")]
        public ErrorDto AF_Parametros_Actualizar(int CodEmpresa, string Usuario, string Codigo, string Valor)
        {
            return BL_AF_Parametros.AF_Parametros_Actualizar(CodEmpresa, Usuario, Codigo, Valor);
        }
    }
}