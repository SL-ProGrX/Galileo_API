using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta los adjuntos del expediente.
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2AdjuntoDto>> Prea_frmPreaEstudiov2_Adjuntos_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<List<FrmPreaEstudiov2AdjuntoDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = []
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = @"SELECT ID_ADJUNTO AS id_adjunto, NOM_ADJUNTO AS nombre_archivo, 
                                     FECHA_REG AS fecha, USUARIO_REG AS usuario 
                                     FROM CRD_PREA_V2_ADJUNTOS 
                                     WHERE ID_EXPEDIENTE = @Expediente 
                                     ORDER BY FECHA_REG DESC";

                result.Result = connection.Query<FrmPreaEstudiov2AdjuntoDto>(
                    sql,
                    new { Expediente = cod_preanalisis.Trim() },
                    commandType: CommandType.Text
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = [];
            }

            return result;
        }

        /// <summary>
        /// VB6: btnAdjunto_Guardar_Click. Guarda el archivo (contenido binario) adjunto al expediente.
        /// INSERT INTO CRD_PREA_V2_ADJUNTOS (ID_EXPEDIENTE, DOC_ADJUNTO, NOM_ADJUNTO, USUARIO_REG, FECHA_REG)
        /// Validación defensiva replicada del VB6: no permite modificar adjuntos si el estado
        /// es "A" (Aprobado), "D" (Descartado) o "B" (Abandonado).
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Adjunto_Guardar(
            int codEmpresa,
            string usuario,
            string cod_preanalisis,
            string nombre_archivo,
            byte[] contenido)
        {
            return EjecutarCambioAdjunto(codEmpresa, cod_preanalisis, (connection, codPreanalisis) =>
            {
                const string sql = @"INSERT INTO CRD_PREA_V2_ADJUNTOS (ID_EXPEDIENTE, DOC_ADJUNTO, NOM_ADJUNTO, USUARIO_REG, FECHA_REG)
                                     VALUES (@Expediente, @Contenido, @NombreArchivo, @Usuario, @FechaReg)";

                connection.Execute(sql, new
                {
                    Expediente = codPreanalisis,
                    Contenido = contenido,
                    NombreArchivo = nombre_archivo,
                    Usuario = usuario,
                    FechaReg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }, commandType: CommandType.Text);
            });
        }

        /// <summary>
        /// VB6: btnAdjunto_Elimina_Click.
        /// delete CRD_PREA_V2_ADJUNTOS Where ID_EXPEDIENTE = '&lt;exp&gt;' and ID_ADJUNTO in(&lt;ids&gt;)
        /// Validación defensiva replicada del VB6: no permite eliminar adjuntos si el estado
        /// es "A" (Aprobado), "D" (Descartado) o "B" (Abandonado).
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Adjunto_Eliminar(
            int codEmpresa,
            string cod_preanalisis,
            int id_adjunto)
        {
            return EjecutarCambioAdjunto(codEmpresa, cod_preanalisis, (connection, codPreanalisis) =>
            {
                const string sql = @"DELETE CRD_PREA_V2_ADJUNTOS WHERE ID_EXPEDIENTE = @Expediente AND ID_ADJUNTO = @IdAdjunto";

                connection.Execute(sql, new
                {
                    Expediente = codPreanalisis,
                    IdAdjunto = id_adjunto
                }, commandType: CommandType.Text);
            });
        }

        private ErrorDto<string> EjecutarCambioAdjunto(
            int codEmpresa,
            string cod_preanalisis,
            Action<IDbConnection, string> ejecutar)
        {
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = "Ok"
            };

            var codPreanalisis = (cod_preanalisis ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(codPreanalisis))
            {
                result.Code = -1;
                result.Description = "Debe indicar el expediente.";
                return result;
            }

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var estadoActual = ObtenerEstadoExpediente(connection, codPreanalisis);
                if (!string.IsNullOrEmpty(estadoActual))
                {
                    var estadoUpper = estadoActual.ToUpperInvariant();
                    if (estadoUpper == "A" || estadoUpper == "D" || estadoUpper == "B")
                    {
                        result.Code = -1;
                        result.Description = "Este Expediente no puede ser modificado!";
                        return result;
                    }
                }

                ejecutar(connection, codPreanalisis);
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = string.Empty;
            }

            return result;
        }
    }
}
