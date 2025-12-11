using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmActivosReportesController : ControllerBase
    {

        private readonly FrmActivosReportesBL _bl;
        public FrmActivosReportesController(IConfiguration config)
        {
            _bl = new FrmActivosReportesBL(config);
        }

        [Authorize]
        [HttpGet("Activos_Reportes_Departamentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Departamentos_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Reportes_Departamentos_Obtener(CodEmpresa);
        }
       
        [Authorize]
        [HttpGet("Activos_Reportes_Secciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Secciones_Obtener(int CodEmpresa, string departamento)
        {
            return _bl.Activos_Reportes_Secciones_Obtener(CodEmpresa, departamento);
        }
        
        [Authorize]
        [HttpGet("Activos_Reportes_TipoActivo_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_TipoActivo_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Reportes_TipoActivo_Obtener(CodEmpresa);
        }
       
        [Authorize]
        [HttpGet("Activos_Reportes_Localizacion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Reportes_Localizacion_Obtener(int CodEmpresa)
        {
            return _bl.Activos_Reportes_Localizacion_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("Activos_Reportes_PeriodoEstado")]
        public ErrorDto<string> Activos_Reportes_PeriodoEstado(int CodEmpresa, DateTime fecha)
        {
            return _bl.Activos_Reportes_PeriodoEstado(CodEmpresa, fecha);
        }

        [Authorize]
        [HttpGet("Activos_Periodo_Consultar")]
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            return _bl.Activos_Periodo_Consultar(CodEmpresa, contabilidad);
        }

        [Authorize]
        [HttpGet("Activos_Reportes_Responsables_Consultart")]
        public ErrorDto<List<ActivosReportesResponsableData>> Activos_Reportes_Responsables_Consultart(int CodEmpresa)
        {
            return _bl.Activos_Reportes_Responsables_Consultart(CodEmpresa);
        }
    }
}