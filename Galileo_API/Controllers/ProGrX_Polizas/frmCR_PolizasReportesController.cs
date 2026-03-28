using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Polizas.FrmCRPolizasReportesModels;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCRPolizasReportesController : ControllerBase
    {

        private readonly FrmCRPolizasReportesBL _bl;

        public FrmCRPolizasReportesController(IConfiguration config)
        {
            _bl = new FrmCRPolizasReportesBL(config);
        }


        [Authorize]
        [HttpGet("Cr_PolizasReportes_Lineas_Obtener")]
        public ErrorDto<List<CrdPolizasLineaModel>> Cr_PolizasReportes_Lineas_Obtener(int codEmpresa)
             => _bl.Cr_PolizasReportes_Lineas_Obtener(codEmpresa);
        
        [Authorize]
        [HttpGet("Cr_PolizasReportes_Departamentos_Obtene")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolizasReportes_Departamentos_Obtene(int codEmpresa, string usuario, int codContabilidad)
              => _bl.Cr_PolizasReportes_Departamentos_Obtene(codEmpresa, usuario, codContabilidad);

        [Authorize]
        [HttpGet("Crd_PolizasReportes_Secciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasReportes_Secciones_Obtener(int codEmpresa, string usuario, int codContabilidad, string? departamentoCodigo="")
            => _bl.Crd_PolizasReportes_Secciones_Obtener(codEmpresa, usuario, codContabilidad, departamentoCodigo);
        
        [Authorize]
        [HttpGet("Cr_PolizasReportes_Socios_Obtener")]
        public ErrorDto<List<CrdPolizasReportesSocioModel>> Cr_PolizasReportes_Socios_Obtener(int codEmpresa)
               => _bl.Cr_PolizasReportes_Socios_Obtener(codEmpresa);

        [Authorize]
        [HttpGet("Crd_PolizasReportes_Cantones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasReportes_Cantones_Obtener(int codEmpresa, string provincia)
               => _bl.Crd_PolizasReportes_Cantones_Obtener(codEmpresa, provincia);

        [Authorize]
        [HttpGet("Crd_PolizasReportes_Distritos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasReportes_Distritos_Obtener(int codEmpresa, string provincia, string canton)
               => _bl.Crd_PolizasReportes_Distritos_Obtener(codEmpresa, provincia, canton);

        [Authorize]
        [HttpGet("Cr_PolizasReportes_Inicializar")]
        public ErrorDto<CrdPolizasReportesInicializarResponse> Cr_PolizasReportes_Inicializar(int codEmpresa, string usuario, int codContabilidad)
                => _bl.Cr_PolizasReportes_Inicializar(codEmpresa, usuario, codContabilidad);

        [Authorize]
        [HttpPost("Crd_PolizasReportes_ReporteConfig_Obtener")]
        public ErrorDto<CrdPolizasReporteConfigResponse> Crd_PolizasReportes_ReporteConfig_Obtener(int codEmpresa, [FromBody] CrdPolizasReportesRequest request, string usuario, string nombreEmpresa)
             => _bl.Crd_PolizasReportes_ReporteConfig_Obtener(codEmpresa, request, usuario, nombreEmpresa);

    }
}
