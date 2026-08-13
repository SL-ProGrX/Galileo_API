using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta las refundiciones del expediente. Fiel a VB6 sbRefundiciones_Load
        /// (frmPreaEstudiov2.frm línea ~17313): exec spCrdPreaConsultaRefundicionesPreanalisis
        /// '&lt;expediente&gt;', '&lt;fechaFormaliza yyyy-mm-dd&gt;', '&lt;codGarantia&gt;'.
        /// dtpR_Formaliza no tiene evento Change/Validate en VB6 (solo default = fecha de servidor
        /// al abrir el formulario), por lo que aquí se usa siempre la fecha del servidor.
        /// Totales replican sbRefundiciones_Calcula: solo suman filas con Aplica=1.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_Consultar(
            int codEmpresa,
            string cod_preanalisis,
            string cod_garantia)
        {
            var result = new ErrorDto<FrmPreaEstudiov2RefundicionesResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2RefundicionesResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = "EXEC spCrdPreaConsultaRefundicionesPreanalisis @Expediente, @Fecha, @Garantia";
                var refundiciones = connection.Query<FrmPreaEstudiov2RefundicionDto>(
                    sql,
                    new
                    {
                        Expediente = cod_preanalisis.Trim(),
                        Fecha = DateTime.Now.Date,
                        Garantia = (cod_garantia ?? string.Empty).Trim()
                    }
                ).ToList();

                result.Result = new FrmPreaEstudiov2RefundicionesResponse
                {
                    refundiciones = refundiciones,
                    total_refunde = refundiciones.Where(r => r.aplica && !r.apl_mora).Sum(r => r.refunde),
                    total_cuotas = refundiciones.Where(r => r.aplica && !r.apl_mora).Sum(r => r.cuota),
                    total_mora = refundiciones.Where(r => r.aplica && r.apl_mora).Sum(r => r.mora)
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2RefundicionesResponse();
            }

            return result;
        }

        /// <summary>
        /// Actualiza (recalcula) las refundiciones del expediente. Fiel a VB6
        /// btnRefundiciones_Actualiza_Click (frmPreaEstudiov2.frm línea ~13669):
        /// exec spCrdPreaRefundicionesActualiza '&lt;expediente&gt;'.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_Actualizar(
            int codEmpresa,
            FrmPreaEstudiov2RefundicionesActualizarRequest request,
            string cod_garantia)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = "EXEC spCrdPreaRefundicionesActualiza @Expediente";
                connection.Execute(sql, new { Expediente = request.cod_preanalisis.Trim() });
            }
            catch (Exception ex)
            {
                return new ErrorDto<FrmPreaEstudiov2RefundicionesResponse>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new FrmPreaEstudiov2RefundicionesResponse()
                };
            }

            return Prea_frmPreaEstudiov2_Refundiciones_Consultar(codEmpresa, request.cod_preanalisis, cod_garantia);
        }

        /// <summary>
        /// Actualiza los checkboxes Aplica / Apl_Mora de una fila de refundición. Fiel a VB6
        /// gRefunde_ButtonClicked (frmPreaEstudiov2.frm línea ~16073):
        /// update CRD_PREA_REFUNDICIONES set Aplica = &lt;0/1&gt;, Apl_Mora = &lt;0/1&gt;
        /// where cod_PreAnalisis = '&lt;expediente&gt;' and id_solicitud = &lt;id&gt;.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2RefundicionesResponse> Prea_frmPreaEstudiov2_Refundiciones_ToggleAplica(
            int codEmpresa,
            FrmPreaEstudiov2RefundicionToggleRequest request,
            string cod_garantia)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = @"UPDATE CRD_PREA_REFUNDICIONES
                    SET Aplica = @Aplica, Apl_Mora = @AplMora
                    WHERE cod_PreAnalisis = @Expediente AND id_solicitud = @IdSolicitud";
                connection.Execute(sql, new
                {
                    Aplica = request.aplica ? 1 : 0,
                    AplMora = request.apl_mora ? 1 : 0,
                    Expediente = request.cod_preanalisis.Trim(),
                    IdSolicitud = request.id_solicitud
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto<FrmPreaEstudiov2RefundicionesResponse>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new FrmPreaEstudiov2RefundicionesResponse()
                };
            }

            return Prea_frmPreaEstudiov2_Refundiciones_Consultar(codEmpresa, request.cod_preanalisis, cod_garantia);
        }
    }
}
