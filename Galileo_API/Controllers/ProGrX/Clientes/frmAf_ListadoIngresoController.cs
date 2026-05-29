using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfListadoIngresoController : ControllerBase
    {
        private readonly FrmAfListadoIngresoBL _bl;
        public FrmAfListadoIngresoController(IConfiguration config)
        {
            _bl = new FrmAfListadoIngresoBL(config);
        }

        [Authorize]
        [HttpGet("AF_ListadoIngreso_Estados_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_ListadoIngreso_Estados_Obtener(int CodEmpresa)
        {
            return _bl.AF_ListadoIngreso_Estados_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_ListadoIngreso_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_ListadoIngreso_Instituciones_Obtener(int CodEmpresa)
        {
            return _bl.AF_ListadoIngreso_Instituciones_Obtener(CodEmpresa);
        }
    }
}