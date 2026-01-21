using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cajas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class FrmCajasCrdAbonosStPController : ControllerBase
    {
        private readonly FrmCajasCrdAbonosStpBL _bl;

        public FrmCajasCrdAbonosStPController(IConfiguration config)
        {
            _bl = new FrmCajasCrdAbonosStpBL(config);
        }

        #region Consultas base

        [HttpGet("CajasCrdAbonosSt_fxCrdParametro")]
        public ErrorDto<int> CajasCrdAbonosSt_fxCrdParametro(int CodEmpresa, string parametro)
        {
            return _bl.CajasCrdAbonosSt_fxCrdParametro(CodEmpresa, parametro);
        }

        [HttpGet("CajasCrdAbonosSt_Documentos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CajasCrdAbonosSt_Documentos_Obtener(int CodEmpresa, string codCaja)
        {
            return _bl.CajasCrdAbonosSt_Documentos_Obtener(CodEmpresa, codCaja);
        }

        [HttpGet("CajasCrdAbonosSt_Operaciones_Obtener")]
        public ErrorDto<List<CajasCrdAbonosStPDData>> CajasCrdAbonosSt_Operaciones_Obtener(int CodEmpresa)
        {
            return _bl.CajasCrdAbonosSt_Operaciones_Obtener(CodEmpresa);
        }

        [HttpGet("CajasCrdAbonosSt_ConsultaOperacion_Obtener")]
        public ErrorDto<CajasCrdAbonosStPDData> CajasCrdAbonosSt_ConsultaOperacion_Obtener(int CodEmpresa, string CodCaja, int OperacionId)
        {
            return _bl.CajasCrdAbonosSt_ConsultaOperacion_Obtener(CodEmpresa, CodCaja, OperacionId);
        }

        [HttpGet("CajasCrdAbonosSt_CargaOperacionCodCed")]
        public ErrorDto<CajasCrdAbonoCargaOperacionData> CajasCrdAbonosSt_CargaOperacionCodCed(int CodEmpresa, string cedula, string codigo)
        {
            return _bl.CajasCrdAbonosSt_CargaOperacionCodCed(CodEmpresa, cedula, codigo);
        }

        #endregion

        #region Mora

        [HttpGet("CajasCrdAbonosSt_MoraConsulta")]
        public ErrorDto<List<CajasCrdAbonoMorosidadData>> CajasCrdAbonosSt_MoraConsulta(int CodEmpresa, int Operacion, DateTime FechaPago)
        {
            return _bl.CajasCrdAbonosSt_MoraConsulta(CodEmpresa, Operacion, FechaPago);
        }

        [HttpGet("CajasCrdAbonosSt_MoraConsultaResumen")]
        public ErrorDto<MoraConsultaResponse> CajasCrdAbonosSt_MoraConsultaResumen(int CodEmpresa, long Operacion, DateTime FechaPago)
        {
            return _bl.CajasCrdAbonosSt_MoraConsultaResumen(CodEmpresa, Operacion, FechaPago);
        }

        #endregion

        #region Simulación / Recalculo

        /// <summary>
        /// Simula cuotas/proyección (equivalente a txtCuotas_Change VB6), recomendado como POST por el tamaño del request.
        /// </summary>
        [HttpPost("CajasCrdAbonosSt_SimularCuotas")]
        public ErrorDto<SimularCuotasResponse> CajasCrdAbonosSt_SimularCuotas(int CodEmpresa,SimularCuotasRequest req)
        {
            if (req == null)
                return DbHelper.CreateErrorResponse<SimularCuotasResponse>("Request inválido.");

            const int MAX_CUOTAS = 480;
            if (req.CantidadCuotas < 0 || req.CantidadCuotas > MAX_CUOTAS)
                return DbHelper.CreateErrorResponse<SimularCuotasResponse>($"CantidadCuotas fuera de rango (0..{MAX_CUOTAS}).");

            return _bl.CajasCrdAbonosSt_SimularCuotas(CodEmpresa, req);
        }

        /// <summary>
        /// Recalcula cuota (equivalente a txtCompromiso_Change cuando chkRecalculaCuota=True).
        /// </summary>
        [HttpPost("CajasCrdAbonosSt_RecalcularCuota")]
        public ErrorDto<RecalculaCuotaResponse> CajasCrdAbonosSt_RecalcularCuota( int CodEmpresa,  RecalculaCuotaRequest req)
        {
            return _bl.CajasCrdAbonosSt_RecalcularCuota(CodEmpresa, req);
        }

        #endregion

        #region Aplicar abono

        [HttpPost("CajasCrdAbonosSt_Abono_Aplica")]
        public ErrorDto CajasCrdAbonosSt_Abono_Aplica(int CodEmpresa,CajasCrdAbonoRequest request)
        {
            return _bl.CajasCrdAbonosSt_Abono_Aplica(CodEmpresa, request);
        }

        #endregion

        #region Bitácora + Documento (si se usan desde UI)

        [HttpPost("Bitacora")]
        public ErrorDto Bitacora(int CodEmpresa, string usuario,string detalle)
        {
            return _bl.Bitacora(CodEmpresa, usuario, detalle);
        }

        [HttpPost("sbDocumentoAbono")]
        public ErrorDto sbDocumentoAbono( int CodEmpresa, DocumentoAbonoRequest req)
        {
            return _bl.sbDocumentoAbono(CodEmpresa, req.Solicitud, req.Variables);
        }

        public sealed class DocumentoAbonoRequest
        {
            public CajasCrdAbonosStPDData Solicitud { get; set; } = new CajasCrdAbonosStPDData();
            public CajasCrdAbonosStpVariables Variables { get; set; } = new CajasCrdAbonosStpVariables();
        }

        #endregion

        #region Helpers expuestos

        [HttpGet("fxFechaProcesoSiguiente")]
        public ErrorDto<decimal> fxFechaProcesoSiguiente(int CodEmpresa, decimal pProceso)
        {
            return _bl.fxFechaProcesoSiguiente(CodEmpresa, pProceso);
        }

        [HttpGet("fxCalcula_Cuota")]
        public ErrorDto<decimal> fxCalcula_Cuota(int CodEmpresa, decimal monto, int plazo, object interes, string? frecuencia = "M")
        {
            return _bl.fxCalcula_Cuota(CodEmpresa, monto, plazo, interes, frecuencia);
        }

        #endregion
    }
}
