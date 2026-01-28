using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Galileo_API.BusinessLogic.ProGrX.Bancos;
using Galileo.Models.ERROR;
using Galileo.Models;

namespace Galileo_API.Controllers.ProGrX.Bancos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmTesReportesController : ControllerBase
    {
        private readonly FrmTesReportesBL _reportesBL;

        public FrmTesReportesController(IConfiguration config)
        {
            _reportesBL = new FrmTesReportesBL(config);
        }

        [HttpGet("sbTesBancoCargaCboAccesoGeneral")]
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesBancoCargaCboAccesoGeneral(int CodEmpresa)
        {
            return _reportesBL.sbTesBancoCargaCboAccesoGeneral(CodEmpresa);
        }

        [HttpGet("sbTesTiposDocsCargaCbo")]
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTiposDocsCargaCbo(int CodEmpresa, int id_banco)
        {
            return _reportesBL.sbTesTiposDocsCargaCbo(CodEmpresa, id_banco);
        }

        [HttpGet("sbTESCombos")]
        public ErrorDto<List<DropDownListaGenericaModel>> sbTESCombos(string tipo)
        {
            return _reportesBL.sbTESCombos(tipo);
        }

        [HttpGet("sbTesUnidadesCargaCboGeneral")]
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesUnidadesCargaCboGeneral(int CodEmpresa, int contabilidad)
        {
            return _reportesBL.sbTesUnidadesCargaCboGeneral(CodEmpresa, contabilidad);
        }

        [HttpGet("sbTesConceptosCargaCboGeneral")]
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesConceptosCargaCboGeneral(int CodEmpresa)
        {
            return _reportesBL.sbTesConceptosCargaCboGeneral(CodEmpresa);
        }

        [HttpGet("Tes_AnalisisCubo_Obtener")]
        public ErrorDto<string> Tes_AnalisisCubo_Obtener(int CodEmpresa, string tipo, DateTime fechaInicio, DateTime fechaCorte)
        {
            return _reportesBL.Tes_AnalisisCubo_Obtener(CodEmpresa, tipo, fechaInicio, fechaCorte);
        }

        [HttpGet("sbTesTokens")]
        public ErrorDto<List<DropDownListaGenericaModel>> sbTesTokens(int CodEmpresa)
        {
            return _reportesBL.sbTesTokens(CodEmpresa);
        }
    }
}
