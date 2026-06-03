using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using Galileo.BusinessLogic.ProGrX_Personas;

namespace Galileo.Controllers.ProGrX_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFBienesController : ControllerBase
    {
        private readonly FrmAFBienesBL _bl;

        public FrmAFBienesController(IConfiguration config)
        {
            _bl = new FrmAFBienesBL(config);
        }

        [Authorize]
        [HttpGet("AF_BienesTipos_Obtener")]
        public ErrorDto<BienesTipoLista> AF_BienesTipos_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_BienesTipos_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_BienesTipos_Guardar")]
        public ErrorDto AF_BienesTipos_Guardar(int CodEmpresa, string usuario, BienesTipoData bienTipo)
        {
            return _bl.AF_BienesTipos_Guardar(CodEmpresa, usuario, bienTipo);
        }

        [Authorize]
        [HttpDelete("AF_BienesTipos_Eliminar")]
        public ErrorDto AF_BienesTipos_Eliminar(int CodEmpresa, string usuario, string bienTipo)
        {
            return _bl.AF_BienesTipos_Eliminar(CodEmpresa, usuario, bienTipo);
        }

        [Authorize]
        [HttpGet("AF_BienesTipos_Valida")]
        public ErrorDto AF_BienesTipos_Valida(int CodEmpresa, string bienTipo)
        {
            return _bl.AF_BienesTipos_Valida(CodEmpresa, bienTipo);
        }

        [Authorize]
        [HttpGet("AF_BienesTipos_Exportar")]
        public ErrorDto<List<BienesTipoData>> AF_BienesTipos_Exportar(int CodEmpresa, string filtros)
        {
            return _bl.AF_BienesTipos_Exportar(CodEmpresa, filtros);
        }
    }
}
