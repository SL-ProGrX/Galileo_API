using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCrCreditosCargaLoteController : ControllerBase
    {
        private readonly FrmCrCreditosCargaLoteBL _bl;

        public FrmCrCreditosCargaLoteController(IConfiguration config)
        {
            _bl = new FrmCrCreditosCargaLoteBL(config);
        }

        [Authorize]
        [HttpGet("CrCreditosCargaLote_Cliente_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Cliente_Obtener(int CodEmpresa)
        {
            return _bl.CrCreditosCargaLote_Cliente_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CrCreditosCargaLote_Destinos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_Destinos_Obtener(int CodEmpresa, string codigo)
        {
            return _bl.CrCreditosCargaLote_Destinos_Obtener(CodEmpresa, codigo);
        }

        [Authorize]
        [HttpGet("CrCreditosCargaLote_ConceptosDesembolso_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CrCreditosCargaLote_ConceptosDesembolso_Obtener(int CodEmpresa)
        {
            return _bl.CrCreditosCargaLote_ConceptosDesembolso_Obtener(CodEmpresa);
        }
    }
}
