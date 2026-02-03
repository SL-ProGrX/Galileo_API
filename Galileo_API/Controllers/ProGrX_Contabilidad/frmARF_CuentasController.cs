using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmArfCuentasController : ControllerBase
    {
        private readonly FrmArfCuentasBl _bl;

        public FrmArfCuentasController(IConfiguration config) => _bl = new FrmArfCuentasBl(config);
        
        [HttpGet("ArfCuentas_Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> ArfCuentas_Divisas_Obtener(int codEmpresa)
        {
            return _bl.ArfCuentas_Divisas_Obtener(codEmpresa);
        }

        [HttpGet("ArfCuentas_Unidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> ArfCuentas_Unidades_Obtener(int codEmpresa)
        {
            return _bl.ArfCuentas_Unidades_Obtener(codEmpresa);
        }

        [HttpGet("ArfCuentas_Obtener")]
        public ErrorDto<List<ArfCuentasDto>> ArfCuentas_Obtener(int codEmpresa, string codDivisa, string codUnidad)
        {
            return _bl.ArfCuentas_Obtener(codEmpresa, codDivisa, codUnidad);
        }

        [HttpPost("ArfCuentas_Registrar")]
        public ErrorDto ArfCuentas_Registrar(int codEmpresa, ArfCuentasRegistraRequest request)
        {
            return _bl.ArfCuentas_Registrar(codEmpresa, request);
        }
    }
}