using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Credito;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrCatalogoGruposController : ControllerBase
    {
        private readonly FrmCrCatalogoGruposBl _bl;

        public FrmCrCatalogoGruposController(IConfiguration config)
        {
            _bl = new FrmCrCatalogoGruposBl(config);
        }

        [HttpGet("CrCatalogoGrupos_Obtener")]
        public ErrorDto<List<CrCatalogoGrupoData>> CrCatalogoGrupos_Obtener(
            int codEmpresa,
            bool? activos)
        {
            return _bl.CrCatalogoGrupos_Obtener(codEmpresa, activos);
        }

        [HttpPost("CrCatalogoGrupos_Consulta_Calcular")]
        public ErrorDto<List<CrCatalogoGrupoConsultaData>> CrCatalogoGrupos_Consulta_Calcular(
            int codEmpresa,
            CrCatalogoGrupoConsultaRequest request)
        {
            return _bl.CrCatalogoGrupos_Consulta_Calcular(codEmpresa, request);
        }

        [HttpPost("CrCatalogoGrupos_Guardar")]
        public ErrorDto CrCatalogoGrupos_Guardar(
            int codEmpresa,
            string usuario,
            CrCatalogoGrupoData request)
        {
            return _bl.CrCatalogoGrupos_Guardar(codEmpresa, usuario, request);
        }

        [HttpGet("CrCatalogoGrupos_AsignacionCatalogos_Obtener")]
        public ErrorDto<List<CrCatalogoGrupoAsignacionCatalogoData>> CrCatalogoGrupos_AsignacionCatalogos_Obtener(
            int codEmpresa,
            string codGrupo)
        {
            return _bl.CrCatalogoGrupos_AsignacionCatalogos_Obtener(codEmpresa, codGrupo);
        }

        [HttpPost("CrCatalogoGrupos_Asignacion_Guardar")]
        public ErrorDto CrCatalogoGrupos_Asignacion_Guardar(
            int codEmpresa,
            CrCatalogoGrupoAsignacionGuardarRequest request)
        {
            return _bl.CrCatalogoGrupos_Asignacion_Guardar(codEmpresa, request);
        }

        [HttpGet("CrCatalogoGrupos_Diario_Obtener")]
        public ErrorDto<List<CrCatalogoGrupoDiarioData>> CrCatalogoGrupos_Diario_Obtener(
            int codEmpresa,
            string codGrupo)
        {
            return _bl.CrCatalogoGrupos_Diario_Obtener(codEmpresa, codGrupo);
        }

        [HttpPost("CrCatalogoGrupos_Diario_Guardar")]
        public ErrorDto CrCatalogoGrupos_Diario_Guardar(
            int codEmpresa,
            CrCatalogoGrupoDiarioGuardarRequest request)
        {
            return _bl.CrCatalogoGrupos_Diario_Guardar(codEmpresa, request);
        }

        [HttpDelete("CrCatalogoGrupos_Eliminar")]
        public ErrorDto CrCatalogoGrupos_Eliminar(
            int codEmpresa, 
            string usuario,
            string codGrupo)
        {
            return _bl.CrCatalogoGrupos_Eliminar(codEmpresa, usuario, codGrupo);
        }
    }
}