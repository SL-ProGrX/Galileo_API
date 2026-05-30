using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFProfesionesController : ControllerBase
    {
        private readonly FrmAFProfesionesBL _bl;

        public FrmAFProfesionesController(IConfiguration config)
        {
            _bl = new FrmAFProfesionesBL(config);
        }

        [Authorize]
        [HttpGet("AF_Profesiones_Obtener")]
        public ErrorDto<TablasListaGenericaModel> AF_Profesiones_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_Profesiones_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_Profesiones_Guardar")]
        public ErrorDto AF_Profesiones_Guardar(int CodEmpresa, string Usuario, string Codigo, string Descripcion)
        {
            return _bl.AF_Profesiones_Guardar(CodEmpresa, Usuario, Codigo, Descripcion);
        }

        [Authorize]
        [HttpDelete("AF_Profesiones_Eliminar")]
        public ErrorDto AF_Profesiones_Eliminar(int CodEmpresa, string Usuario, int Codigo, string Descripcion)
        {
            return _bl.AF_Profesiones_Eliminar(CodEmpresa, Usuario, Codigo, Descripcion);
        }
    }
}