using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrx_Personas;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;

namespace Galileo.Controllers.ProGrx_Personas
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmAFCanalesTiposController : ControllerBase
    {
        private readonly FrmAFCanalesTiposBL _bl;

        public FrmAFCanalesTiposController(IConfiguration config)
        {
            _bl = new FrmAFCanalesTiposBL(config);
        }

        [Authorize]
        [HttpGet("AF_CanalesTipos_Obtener")]
        public ErrorDto<CanalTipoLista> AF_CanalesTipos_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.AF_CanalesTipos_Obtener(CodEmpresa, filtros);
        }

        [Authorize]
        [HttpPost("AF_CanalesTipos_Guardar")]
        public ErrorDto AF_CanalesTipos_Guardar(int CodEmpresa, string usuario, CanalTipoData canalTipo)
        {
            return _bl.AF_CanalesTipos_Guardar(CodEmpresa, usuario, canalTipo);
        }

        [Authorize]
        [HttpDelete("AF_CanalesTipos_Eliminar")]
        public ErrorDto AF_CanalesTipos_Eliminar(int CodEmpresa, string usuario, string canalTipo)
        {
            return _bl.AF_CanalesTipos_Eliminar(CodEmpresa, usuario, canalTipo);
        }

        [Authorize]
        [HttpGet("AF_CanalesTipos_Valida")]
        public ErrorDto AF_CanalesTipos_Valida(int CodEmpresa, string canalTipo)
        {
            return _bl.AF_CanalesTipos_Valida(CodEmpresa, canalTipo);
        }

        [Authorize]
        [HttpGet("AF_CanalesTipos_Exportar")]
        public ErrorDto<List<CanalTipoData>> AF_CanalesTipos_Exportar(int CodEmpresa, string filtros)
        {
            return _bl.AF_CanalesTipos_Exportar(CodEmpresa, filtros);
        }
    }
}