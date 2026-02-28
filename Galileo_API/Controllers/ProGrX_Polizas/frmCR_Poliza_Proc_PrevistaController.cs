using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPolizaProcPrevistaController : ControllerBase
    {
        private readonly FrmCrPolizaProcPrevistaBL _bl;

        public FrmCrPolizaProcPrevistaController(IConfiguration config)
        {
            _bl = new FrmCrPolizaProcPrevistaBL(config);
        }

        [HttpGet("Cr_PolProcPrevista_PolizaFacturables_Lista")]
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolProcPrevista_PolizaFacturables_Lista(int CodEmpresa)
        {
            return _bl.Cr_PolProcPrevista_PolizaFacturables_Lista(CodEmpresa);
        }

        [HttpPost("Cr_PolProcPrevista_Corte_Detalle_Cargar")]
        public ErrorDto Cr_PolProcPrevista_Corte_Detalle_Cargar(int CodEmpresa, string usuario, CrPolProcPrevistaDetalleAddRequest request)
        {
            return _bl.Cr_PolProcPrevista_Corte_Detalle_Cargar(CodEmpresa, usuario, request);
        }

        [HttpPost("Cr_PolProcPrevista_Corte_Detalle_Eliminar")]
        public ErrorDto Cr_PolProcPrevista_Corte_Detalle_Eliminar(int CodEmpresa, string usuario, CrPolProcPrevistaDetalleEliminarRequest request)
        {
            return _bl.Cr_PolProcPrevista_Corte_Detalle_Eliminar(CodEmpresa, usuario, request);
        }

        [HttpGet("Cr_PolProcPrevista_Corte_Detalle_Consulta")]
        public ErrorDto<List<CrPolProcprevistaDetalleDto>> Cr_PolProcPrevista_Corte_Detalle_Consulta(
            int CodEmpresa,
            string codPoliza,
            DateTime corte)
        {
            return _bl.Cr_PolProcPrevista_Corte_Detalle_Consulta(CodEmpresa, codPoliza, corte);
        }

        [HttpPost("Cr_PolProcPrevista_Corte_Concilia_Consulta")]
        public ErrorDto<List<CrPolProcPrevistaConciliaDto>> Cr_PolProcPrevista_Corte_Concilia_Consulta(
         int CodEmpresa,
         CrPolProcPrevistaConciliaConsultaRequest request)
        {
            return _bl.Cr_PolProcPrevista_Corte_Concilia_Consulta(CodEmpresa, request);
        }
    }
}
