using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosPolizasReportesController : ControllerBase
    {
        
        private readonly FrmActivosPolizasReportesBL _bl;

        public FrmActivosPolizasReportesController(IConfiguration config)
        {
            _bl = new FrmActivosPolizasReportesBL(config);
        }


        [HttpGet("Activos_PolizasReportesLista_Obtener")]
        [Authorize]
        public ErrorDto<ActivosPolizasReportesLista> Activos_PolizasReportesLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_PolizasReportesLista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Activos_PolizasReportes_Tipos_Lista_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_PolizasReportes_Tipos_Lista_Obtener(int CodEmpresa)
        {
            return _bl.Activos_PolizasReportes_Tipos_Lista_Obtener(CodEmpresa);
        }

        [HttpGet("Activos_PolizasReportes_Estados_Lista_Obtener")]
        [Authorize]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_PolizasReportes_Estados_Lista_Obtener(int CodEmpresa)
        {
            return _bl.Activos_PolizasReportes_Estados_Lista_Obtener(CodEmpresa);
        }
    }
}