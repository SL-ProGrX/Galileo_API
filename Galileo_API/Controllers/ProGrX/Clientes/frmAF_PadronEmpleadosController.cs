using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers.ProGrX.Clientes
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAfPadronEmpleadosController : ControllerBase
    {
        private readonly FrmAfPadronEmpleadosBL _db;

        public FrmAfPadronEmpleadosController(IConfiguration config)
        {
            _db = new FrmAfPadronEmpleadosBL(config);
        }

        [Authorize]
        [HttpGet("AF_PadronEmpleadosInstituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PadronEmpleadosInstituciones_Obtener(int CodEmpresa)
        {
            return _db.AF_PadronEmpleadosInstituciones_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("AF_PadronEmpleadosEstados_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> AF_PadronEmpleadosEstados_Obtener(int CodEmpresa)
        {
            return _db.AF_PadronEmpleadosEstados_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpDelete("AF_PadronEmpleados_Eliminar")]
        public ErrorDto AF_PadronEmpleados_Eliminar(int CodEmpresa, string cedula)
        {
            return _db.AF_PadronEmpleados_Eliminar(CodEmpresa, cedula);
        }

        [Authorize]
        [HttpGet("AF_PadronEmpleados_Obtener")]
        public ErrorDto<TablasListaGenericaModel> AF_PadronEmpleados_Obtener(int CodEmpresa, bool exporta, string filtros, string tblFiltros)
        {
            return _db.AF_PadronEmpleados_Obtener(CodEmpresa, exporta, filtros, tblFiltros);
        }
    }
}