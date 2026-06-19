using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FrmCrReporteDiferidosController : ControllerBase
    {
        private readonly FrmCrReporteDiferidosBl _bl;

        public FrmCrReporteDiferidosController(IConfiguration config)
        {
            _bl = new FrmCrReporteDiferidosBl(config);
        }

        [HttpGet("CrReporteDiferidos_Pantalla_Obtener")]
        public ErrorDto<CrReporteDiferidosPantallaData> CrReporteDiferidos_Pantalla_Obtener(
            int codEmpresa,
            string usuario)
            => _bl.CrReporteDiferidos_Pantalla_Obtener(codEmpresa, usuario);

        [HttpGet("CrReporteDiferidos_Catalogo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrReporteDiferidos_Catalogo_Obtener(
            int codEmpresa)
            => _bl.CrReporteDiferidos_Catalogo_Obtener(codEmpresa);

        [HttpGet("CrReporteDiferidos_Codigo_Descripcion_Obtener")]
        public ErrorDto<DropDownListaGenericaModel> CrReporteDiferidos_Codigo_Descripcion_Obtener(
            int codEmpresa,
            string codigo)
            => _bl.CrReporteDiferidos_Codigo_Descripcion_Obtener(codEmpresa, codigo);

        [HttpGet("CrReporteDiferidos_Consulta_Obtener")]
        public ErrorDto<List<CrReporteDiferidosItem>> CrReporteDiferidos_Consulta_Obtener(
            int codEmpresa,
            string request)
            => _bl.CrReporteDiferidos_Consulta_Obtener(codEmpresa, request);
    }
}