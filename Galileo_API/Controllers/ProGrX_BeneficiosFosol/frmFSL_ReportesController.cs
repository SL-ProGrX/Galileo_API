using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_BeneficiosFosol;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_BeneficiosFosol
{
    [Route("api/frmFSL_Reportes")]
    [Authorize]
    [ApiController]
    public sealed class FrmFslReportesController
        : ControllerBase
    {
        private readonly FrmFslReportesBL _bl;

        public FrmFslReportesController(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _bl = new FrmFslReportesBL(config);
        }

        [HttpGet(
            "FSL_Reportes_Oficinas_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            FSL_Reportes_Oficinas_Obtener(
                int CodEmpresa)
        {
            return _bl
                .FSL_Reportes_Oficinas_Obtener(
                    CodEmpresa);
        }

        [HttpGet(
            "FSL_Reportes_Planes_Obtener")]
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            FSL_Reportes_Planes_Obtener(
                int CodEmpresa)
        {
            return _bl
                .FSL_Reportes_Planes_Obtener(
                    CodEmpresa);
        }
    }
}