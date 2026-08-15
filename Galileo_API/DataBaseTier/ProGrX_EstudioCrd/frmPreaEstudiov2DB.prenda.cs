using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Collections.Generic;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta el tab Prendario del expediente (sbPrendario_Load en VB6).
        /// Fiel a frmPreaEstudiov2.frm ~línea 16545-16590:
        ///   - exec spCrd_Prea_Examenes_Log '&lt;expediente&gt;'  → lswP_Examenes
        ///     (columnas Id, Nota, Usuario, Fecha).
        ///   - exec spCrd_Prea_Prenda_Datos '&lt;expediente&gt;' → txtPolizaPrenda,
        ///     txtPrendaValor y ESTADO_EXAMENES.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2PrendarioConsultarResponse> Prea_frmPreaEstudiov2_Prendario_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2PrendarioConsultarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2PrendarioConsultarResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                var expediente = (cod_preanalisis ?? string.Empty).Trim();

                const string sqlExamenes = "exec spCrd_Prea_Examenes_Log @Expediente";
                var examenes = connection.Query<FrmPreaEstudiov2ExamenPrendaDto>(
                    sqlExamenes, new { Expediente = expediente }).ToList();

                const string sqlPrenda = "exec spCrd_Prea_Prenda_Datos @Expediente";
                var prendaRow = connection.QueryFirstOrDefault(sqlPrenda, new { Expediente = expediente })
                    as IDictionary<string, object>;

                result.Result.examenes = examenes;

                if (prendaRow is not null)
                {
                    var dic = new Dictionary<string, object>(prendaRow, System.StringComparer.OrdinalIgnoreCase);
                    result.Result.monto_poliza_prenda = GetDecimal(dic, "Prenda_Poliza");
                    result.Result.valor_prenda = GetDecimal(dic, "Prenda_Monto");
                    result.Result.estado_examenes = GetString(dic, "ESTADO_EXAMENES");
                }
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2PrendarioConsultarResponse();
            }

            return result;
        }

        /// <summary>
        /// Aplica un estado a los exámenes de prenda del expediente (btnP_Examenes_Click
        /// en VB6, frmPreaEstudiov2.frm ~línea 13584-13610):
        ///   exec spCRD_PreaAplicaEstadoExamenes '&lt;expediente&gt;', '&lt;estado&gt;', '&lt;usuario&gt;', '&lt;nota&gt;'
        /// El SP retorna una columna 'Resultado' con un mensaje cuando falla; vacío si fue ok.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2PrendarioEstadoResponse> Prea_frmPreaEstudiov2_Prendario_Estado(
            int codEmpresa,
            FrmPreaEstudiov2PrendarioEstadoRequest request)
        {
            var result = new ErrorDto<FrmPreaEstudiov2PrendarioEstadoResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2PrendarioEstadoResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var expediente = (request.cod_preanalisis ?? string.Empty).Trim();
                var estado = (request.estado ?? string.Empty).Trim();
                var usuario = (request.usuario ?? string.Empty).Trim();

                var nota = estado switch
                {
                    "E" => "Exámenes médicos enviados",
                    "R" => "Exámenes médicos recibidos",
                    "A" => "Exámenes médicos aprobados",
                    _ => string.Empty
                };

                const string sql = "exec spCRD_PreaAplicaEstadoExamenes @Expediente, @Estado, @Usuario, @Nota";
                var row = connection.QueryFirstOrDefault(sql, new
                {
                    Expediente = expediente,
                    Estado = estado,
                    Usuario = usuario,
                    Nota = nota
                }) as IDictionary<string, object>;

                if (row is not null)
                {
                    var dic = new Dictionary<string, object>(row, System.StringComparer.OrdinalIgnoreCase);
                    var mensaje = GetString(dic, "Resultado");
                    if (!string.IsNullOrWhiteSpace(mensaje))
                    {
                        result.Code = -1;
                        result.Description = mensaje;
                        return result;
                    }
                }

                result.Result.mensaje = "Estado de exámenes actualizado correctamente.";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2PrendarioEstadoResponse();
            }

            return result;
        }
    }
}