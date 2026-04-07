using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_CxC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.CuentasxCobrar
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCxCReportesController : ControllerBase
    {
        private readonly FrmCxCReportesBl _bl;

        public FrmCxCReportesController(IConfiguration config)
        {
            _bl = new FrmCxCReportesBl(config);
        }

        [HttpGet("CxC_Clientes_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Clientes_Listar(int codEmpresa)
        {
            return _bl.CxC_Clientes_Listar(codEmpresa);
        }

        [HttpGet("CxC_Pagadores_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Pagadores_Listar(int codEmpresa)
        {
            return _bl.CxC_Pagadores_Listar(codEmpresa);
        }

        [HttpGet("CxC_Conceptos_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Conceptos_Listar(int codEmpresa)
        {
            return _bl.CxC_Conceptos_Listar(codEmpresa);
        }

        [HttpGet("CxC_Cargos_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_Cargos_Listar(int codEmpresa)
        {
            return _bl.CxC_Cargos_Listar(codEmpresa);
        }
    }
}