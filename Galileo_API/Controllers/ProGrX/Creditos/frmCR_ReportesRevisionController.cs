using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrReportesRevisionController : ControllerBase
    {
        private readonly FrmCrReportesRevisionBL _bl;

        public FrmCrReportesRevisionController(IConfiguration config)
        {
            _bl = new FrmCrReportesRevisionBL(config);
        }

        [Authorize]
        [HttpGet("CR_ReportesRevision_UsuariosGrupos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_UsuariosGrupos_Obtener(int codEmpresa)
            => _bl.CR_ReportesRevision_UsuariosGrupos_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CR_ReportesRevision_Garantias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Garantias_Obtener(int codEmpresa)
            => _bl.CR_ReportesRevision_Garantias_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CR_ReportesRevision_Oficinas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Oficinas_Obtener(int codEmpresa)
            => _bl.CR_ReportesRevision_Oficinas_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CR_ReportesRevision_Comites_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Comites_Obtener(int codEmpresa)
            => _bl.CR_ReportesRevision_Comites_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CR_ReportesRevision_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Instituciones_Obtener(int codEmpresa)
            => _bl.CR_ReportesRevision_Instituciones_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CR_ReportesRevision_Etiquetas_Obtener")]
        public ErrorDto<List<CrAutorizacionTranferenciasTag>> CR_ReportesRevision_Etiquetas_Obtener(int codEmpresa)
            => _bl.CR_ReportesRevision_Etiquetas_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CR_ReportesRevision_Omisiones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Omisiones_Obtener(int codEmpresa)
            => _bl.CR_ReportesRevision_Omisiones_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CR_ReportesRevision_Catalogo_F4_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Catalogo_F4_Obtener(int codEmpresa)
            => _bl.CR_ReportesRevision_Catalogo_F4_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("CR_ReportesRevision_Catalogo_Descripcion_Obtener")]
        public ErrorDto<string?> CR_ReportesRevision_Catalogo_Descripcion_Obtener(int codEmpresa, string codigo)
            => _bl.CR_ReportesRevision_Catalogo_Descripcion_Obtener(codEmpresa, codigo);
    }
}
