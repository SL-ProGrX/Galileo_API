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
            var exp = cod_preanalisis.Trim().Replace("'", "''");

            var fianzas = connection.Query<FrmPreaEstudiov2FianzaDto>(
                $"exec spCrdPreaConsultaFianzas '{exp}'",
                commandType: CommandType.Text
            ).ToList();

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

                var exp = request.cod_preanalisis.Trim().Replace("'", "''");
                var strSQL = $"exec spCRDPreaFianzas '{exp}', 'I'";
                connection.Execute(strSQL, commandType: CommandType.Text);

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

                var exp = request.cod_preanalisis.Trim().Replace("'", "''");
                var aplica = request.aplica ? 1 : 0;
                var cancelaMora = request.cancela_mora ? 1 : 0;

                var strSQL = "update CRD_PREA_DETALLE_FIANZAS set Aplica = " + aplica +
                    ", Cancela_Mora = " + cancelaMora +
                    " where cod_PreAnalisis = '" + exp + "' and id_solicitud = " + request.id_solicitud;

                connection.Execute(strSQL, commandType: CommandType.Text);

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
