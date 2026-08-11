using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.General;
using Galileo_API.Models.ProGrX.General;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.General
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCcAnomaliasController : ControllerBase
    {
        private readonly FrmCcAnomaliasBL _bl;

        public FrmCcAnomaliasController(IConfiguration config)
        {
            _bl = new FrmCcAnomaliasBL(config);
        }

        [Authorize]
        [HttpPost("CcAnomaliasSaldosMenores_Obtener")]
        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasSaldosMenores_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            return _bl.CcAnomaliasSaldosMenores_Obtener(codEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("CcAnomaliasSaldosNegativos_Obtener")]
        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasSaldosNegativos_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            return _bl.CcAnomaliasSaldosNegativos_Obtener(codEmpresa, filtro);
        }

        [Authorize]
        [HttpGet("CcAnomaliasCreditos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasCreditos_Obtener(int codEmpresa)
        {
            return _bl.CcAnomaliasCreditos_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CcAnomaliasDestinos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasDestinos_Obtener(int codEmpresa)
        {
            return _bl.CcAnomaliasDestinos_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpGet("CcAnomaliasInstituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CcAnomaliasInstituciones_Obtener(int codEmpresa)
        {
            return _bl.CcAnomaliasInstituciones_Obtener(codEmpresa);
        }

        [Authorize]
        [HttpPost("CcAnomaliasMoraMenor_Obtener")]
        public ErrorDto<List<CcAnomaliaCreditoItemDto>> CcAnomaliasMoraMenor_Obtener(int codEmpresa, CcAnomaliaFiltroDto filtro)
        {
            return _bl.CcAnomaliasMoraMenor_Obtener(codEmpresa, filtro);
        }

        [Authorize]
        [HttpPost("CcAnomaliasCtaDerivadaMenor_Obtener")]
        public ErrorDto<List<CcAnomaliaCtaDerivadaItemDto>> CcAnomaliasCtaDerivadaMenor_Obtener(int codEmpresa, CcAnomaliaCtaDerivadaFiltroDto filtro)
        {
            return _bl.CcAnomaliasCtaDerivadaMenor_Obtener(codEmpresa, filtro);
        }
    }
}
