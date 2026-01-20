using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Galileo_API.Models.ProGrX.Cajas;

namespace Galileo_API.BusinessLogic.ProGrX.Cajas
{
    /// <summary>
    /// BL de frmCajas_Crd_AbonosStP (migración VB6 -> .NET).
    /// Capa delgada: delega en DB para acceso a datos y lógica heredada.
    /// </summary>
    public sealed class FrmCajasCrdAbonosStpBL
    {
        private readonly FrmCajasCrdAbonosStpDB _db;

        public FrmCajasCrdAbonosStpBL(IConfiguration config)
        {
            _db = new FrmCajasCrdAbonosStpDB(config);
        }

        #region Consultas base

        public ErrorDto<int> CajasCrdAbonosSt_fxCrdParametro(int codEmpresa, string parametro)
            => _db.CajasCrdAbonosSt_fxCrdParametro(codEmpresa, parametro);

        public ErrorDto<List<DropDownListaGenericaModel>> CajasCrdAbonosSt_Documentos_Obtener(int codEmpresa, string codCaja)
            => _db.CajasCrdAbonosSt_Documentos_Obtener(codEmpresa, codCaja);

        public ErrorDto<List<CajasCrdAbonosStPDData>> CajasCrdAbonosSt_Operaciones_Obtener(int codEmpresa)
            => _db.CajasCrdAbonosSt_Operaciones_Obtener(codEmpresa);

        public ErrorDto<CajasCrdAbonosStPDData> CajasCrdAbonosSt_ConsultaOperacion_Obtener(int codEmpresa, string codCaja, int operacionId)
            => _db.CajasCrdAbonosSt_ConsultaOperacion_Obtener(codEmpresa, codCaja, operacionId);

        public ErrorDto<CajasCrdAbonoCargaOperacionData> CajasCrdAbonosSt_CargaOperacionCodCed(int codEmpresa, string cedula, string codigo)
            => _db.CajasCrdAbonosSt_CargaOperacionCodCed(codEmpresa, cedula, codigo);

        #endregion

        #region Mora

        public ErrorDto<List<CajasCrdAbonoMorosidadData>> CajasCrdAbonosSt_MoraConsulta(int codEmpresa, int operacion, DateTime fechaPago)
            => _db.CajasCrdAbonosSt_MoraConsulta(codEmpresa, operacion, fechaPago);

        public ErrorDto<MoraConsultaResponse> CajasCrdAbonosSt_MoraConsultaResumen(int codEmpresa, long operacion, DateTime fechaPago)
            => _db.CajasCrdAbonosSt_MoraConsultaResumen(codEmpresa, operacion, fechaPago);

        #endregion

        #region Simulación / Recalculo

        public ErrorDto<SimularCuotasResponse> CajasCrdAbonosSt_SimularCuotas(int codEmpresa, SimularCuotasRequest req)
            => _db.CajasCrdAbonosSt_SimularCuotas(codEmpresa, req);

        public ErrorDto<RecalculaCuotaResponse> CajasCrdAbonosSt_RecalcularCuota(int codEmpresa, RecalculaCuotaRequest req)
            => _db.CajasCrdAbonosSt_RecalcularCuota(codEmpresa, req);

        #endregion

        #region Aplicación de abono + bitácora / documentos

        public ErrorDto CajasCrdAbonosSt_Abono_Aplica(int codEmpresa, CajasCrdAbonoRequest request)
            => _db.CajasCrdAbonosSt_Abono_Aplica(codEmpresa, request);

        public ErrorDto Bitacora(int codEmpresa, string usuario, string detalle)
            => _db.Bitacora(codEmpresa, usuario, detalle);

        public ErrorDto sbDocumentoAbono(int codEmpresa, CajasCrdAbonosStPDData solicitud, CajasCrdAbonosStpVariables variable)
            => _db.sbDocumentoAbono(codEmpresa, solicitud, variable);

        #endregion

        #region Helpers expuestos

        public ErrorDto<decimal> fxFechaProcesoSiguiente(int codEmpresa, decimal pProceso)
            => _db.fxFechaProcesoSiguiente(codEmpresa, pProceso);

        public ErrorDto<decimal> fxCalcula_Cuota(int CodEmpresa, decimal monto, int plazo, object interes, string? frecuencia = "M")
            => _db.fxCalcula_Cuota(CodEmpresa, monto, plazo, interes, frecuencia);

        #endregion
    }
}
