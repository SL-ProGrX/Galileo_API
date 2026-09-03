using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta las fianzas del expediente. Fiel a VB6 sbFianzas_Load (frmPreaEstudiov2.frm
        /// línea ~17513): exec spCrdPreaConsultaFianzas '&lt;expediente&gt;'.
        /// Totales replican sbFianzas_Calcula: saldo/cuota se dividen entre fiadores y solo
        /// suman cuando Aplica=1; monto_mora se divide entre fiadores y solo suma a
        /// total_saldos cuando Cancela_Mora=1 (VB6 acumula saldo y monto_mora en el mismo total).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2FianzasResponse> Prea_frmPreaEstudiov2_Fianzas_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2FianzasResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2FianzasResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                result.Result = ConsultarFianzas(connection, cod_preanalisis);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2FianzasResponse();
            }

            return result;
        }

        private static FrmPreaEstudiov2FianzasResponse ConsultarFianzas(IDbConnection connection, string cod_preanalisis)
        {
            const string sql = "EXEC spCrdPreaConsultaFianzas @Expediente";
            var rawFianzas = connection.Query(
                sql,
                new { Expediente = cod_preanalisis.Trim() }
            );
            var fianzas = rawFianzas.Select(row =>
            {
                var item = new Dictionary<string, object>(
                    (IDictionary<string, object>)row,
                    StringComparer.OrdinalIgnoreCase);

                return new FrmPreaEstudiov2FianzaDto
                {
                    id_solicitud = GetInt(item, "id_solicitud"),
                    saldo = GetDecimal(item, "saldo"),
                    cuota = GetDecimal(item, "cuota"),
                    fiadores = GetInt(item, "nfiadores"),
                    mora_cuotas = GetInt(item, "Mora_Cuotas"),
                    monto_mora = GetDecimal(item, "Mora_Monto"),
                    aplica = GetBool(item, "aplica"),
                    cancela_mora = GetBool(item, "Cancela_Mora"),
                    montoApr = GetDecimal(item, "MontoApr"),
                    porcentaje = GetDecimal(item, "Porcentaje"),
                    clasificacion = GetString(item, "Clasificacion"),
                };
            }).ToList();

            decimal totalSaldos = 0;
            decimal totalCuotas = 0;

            foreach (var f in fianzas)
            {
                var numFia = f.fiadores <= 0 ? 1 : f.fiadores;

                if (f.aplica)
                {
                    totalSaldos += f.saldo / numFia;
                    totalCuotas += f.cuota / numFia;
                }

                if (f.cancela_mora)
                {
                    totalSaldos += f.monto_mora / numFia;
                }
            }

            return new FrmPreaEstudiov2FianzasResponse
            {
                fianzas = fianzas,
                total_saldos = totalSaldos,
                total_cuotas = totalCuotas
            };
        }

        /// <summary>
        /// Actualiza (recalcula) las fianzas del expediente. Fiel a VB6 btnFianzas_Actualiza_Click
        /// (frmPreaEstudiov2.frm línea ~13294): exec spCRDPreaFianzas '&lt;expediente&gt;', 'I'.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2FianzasResponse> Prea_frmPreaEstudiov2_Fianzas_Actualizar(
            int codEmpresa,
            FrmPreaEstudiov2FianzasActualizarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = "EXEC spCRDPreaFianzas @Expediente, 'I'";
                connection.Execute(sql, new { Expediente = request.cod_preanalisis.Trim() });

                return new ErrorDto<FrmPreaEstudiov2FianzasResponse>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = ConsultarFianzas(connection, request.cod_preanalisis)
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<FrmPreaEstudiov2FianzasResponse>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new FrmPreaEstudiov2FianzasResponse()
                };
            }
        }

        /// <summary>
        /// Actualiza los checkboxes Aplica / Cancela_Mora de una fila de fianza. Fiel a VB6
        /// gFianzas_ButtonClicked (frmPreaEstudiov2.frm línea ~16036):
        /// update CRD_PREA_DETALLE_FIANZAS set Aplica = &lt;0/1&gt;, Cancela_Mora = &lt;0/1&gt;
        /// where cod_PreAnalisis = '&lt;expediente&gt;' and id_solicitud = &lt;id&gt;.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2FianzasResponse> Prea_frmPreaEstudiov2_Fianzas_ToggleAplica(
            int codEmpresa,
            FrmPreaEstudiov2FianzaToggleRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = @"UPDATE CRD_PREA_DETALLE_FIANZAS
                    SET Aplica = @Aplica, Cancela_Mora = @CancelaMora
                    WHERE cod_PreAnalisis = @Expediente AND id_solicitud = @IdSolicitud";
                connection.Execute(sql, new
                {
                    Aplica = request.aplica ? 1 : 0,
                    CancelaMora = request.cancela_mora ? 1 : 0,
                    Expediente = request.cod_preanalisis.Trim(),
                    IdSolicitud = request.id_solicitud
                });

                return new ErrorDto<FrmPreaEstudiov2FianzasResponse>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = ConsultarFianzas(connection, request.cod_preanalisis)
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto<FrmPreaEstudiov2FianzasResponse>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new FrmPreaEstudiov2FianzasResponse()
                };
            }
        }
    }
}
