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

        /// <summary>
        /// Cuenta del parámetro de créditos (VB6 fxCrdParametro). Por defecto parámetro "22".
        /// </summary>
        [Authorize]
        [HttpGet("CcAnomaliasCuentaOpcion_Obtener")]
        public ErrorDto<CcAnomaliaCuentaOpcionDto?> CcAnomaliasCuentaOpcion_Obtener(int codEmpresa, string parametro = "22")
        {
            return _bl.CcAnomaliasCuentaOpcion_Obtener(codEmpresa, parametro);
        }

        /// <summary>
        /// Corrige saldos menores (VB6 sbCorrigeSaldoMenor).
        /// </summary>
        [Authorize]
        [HttpPost("CcAnomaliasSaldosMenores_Corregir")]
        public ErrorDto<CcAnomaliaSaldosMenoresCorregirResultado> CcAnomaliasSaldosMenores_Corregir(
            int codEmpresa,
            CcAnomaliaSaldosMenoresCorregirRequest request)
        {
            return _bl.CcAnomaliasSaldosMenores_Corregir(codEmpresa, request);
        }

        /// <summary>
        /// Corrige saldos negativos (VB6 sbCorrigeSaldoNegativo).
        /// </summary>
        [Authorize]
        [HttpPost("CcAnomaliasSaldosNegativos_Corregir")]
        public ErrorDto<CcAnomaliaSaldosNegativosCorregirResultado> CcAnomaliasSaldosNegativos_Corregir(
            int codEmpresa,
            CcAnomaliaSaldosNegativosCorregirRequest request)
        {
            return _bl.CcAnomaliasSaldosNegativos_Corregir(codEmpresa, request);
        }

        /// <summary>
        /// Corrige mora menor (VB6 sbCorrigeMora). Solo aplica si SysPlanPagos = 0.
        /// </summary>
        [Authorize]
        [HttpPost("CcAnomaliasMoraMenor_Corregir")]
        public ErrorDto<CcAnomaliaMoraMenorCorregirResultado> CcAnomaliasMoraMenor_Corregir(
            int codEmpresa,
            CcAnomaliaMoraMenorCorregirRequest request)
        {
            return _bl.CcAnomaliasMoraMenor_Corregir(codEmpresa, request);
        }

        /// <summary>
        /// Corrige cta. derivada menor (VB6 sbCtaDerivada_Corrige → spSys_Creditos_Clean_Ctas_Menores).
        /// </summary>
        [Authorize]
        [HttpPost("CcAnomaliasCtaDerivadaMenor_Corregir")]
        public ErrorDto<CcAnomaliaCtaDerivadaCorregirResultado> CcAnomaliasCtaDerivadaMenor_Corregir(
            int codEmpresa,
            CcAnomaliaCtaDerivadaCorregirRequest request)
        {
            return _bl.CcAnomaliasCtaDerivadaMenor_Corregir(codEmpresa, request);
        }
    }
}
