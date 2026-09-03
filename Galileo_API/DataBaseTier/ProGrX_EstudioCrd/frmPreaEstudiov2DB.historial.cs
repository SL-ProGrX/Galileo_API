using Dapper;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        /// <summary>
        /// Consulta el historial del expediente (ejecutivo y general).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HistorialResponse> Prea_frmPreaEstudiov2_Historial_Consultar(
            int codEmpresa,
            string cod_preanalisis)
        {
            var result = new ErrorDto<FrmPreaEstudiov2HistorialResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2HistorialResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@COD_PREANANLISIS", cod_preanalisis.Trim(), DbType.String);

                var historialEjecutivo = connection.Query(
                    "spCrdPreaGetHistorial",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).Select(MapHistorialEtiqueta).ToList();

                var historialGeneral = connection.Query(
                    "spCrdPreaGetHistorialGeneral",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).Select(MapHistorialEtiqueta).ToList();

                result.Result = new FrmPreaEstudiov2HistorialResponse
                {
                    ejecutivos = historialEjecutivo,
                    general = historialGeneral
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2HistorialResponse();
            }

            return result;
        }

        private static FrmPreaEstudiov2HistorialDto MapHistorialEtiqueta(dynamic row)
        {
            var values = (IDictionary<string, object>)row;
            var codigoEtiqueta = GetString(values, "cod_etiqueta", "COD_ETIQUETA", "codigo_etiqueta", "tag_codigo", "TAG_CODIGO", "Código Etiqueta", "Codigo Etiqueta");
            var etiqueta = GetString(values, "etiqueta", "Etiqueta", "ETIQUETA");
            var descripcion = GetString(values, "descripcion", "Descripción", "Descripcion", "DESCRIPCION", "notas", "NOTAS", "detalle");
            var usuarioRegistra1 = GetString(values, "usuario_registra_1", "USUARIO_REGISTRA_1", "Usuario Registra 1", "registro_usuario", "REGISTRO_USUARIO", "usuario");
            var usuarioRegistra2 = GetString(values, "usuario_registra_2", "USUARIO_REGISTRA_2", "Usuario Registra 2 (Opcional)", "usuario_registra_2_opcional");
            var usuarioRevision = GetString(values, "usuario_revision", "USUARIO_REVISION", "Usuario Revisión", "Usuario Revision", "autorizador");
            var fecha = GetDate(values, "fecha", "Fecha", "FECHA", "REGISTRO_FECHA", "registro_fecha");

            return new FrmPreaEstudiov2HistorialDto
            {
                fecha = fecha,
                usuario = usuarioRegistra1,
                accion = etiqueta,
                detalle = descripcion,
                cod_etiqueta = codigoEtiqueta,
                codigo_etiqueta = codigoEtiqueta,
                etiqueta = etiqueta,
                descripcion = descripcion,
                usuario_registra_1 = usuarioRegistra1,
                usuario_registra_2 = usuarioRegistra2,
                usuario_revision = usuarioRevision
            };
        }

        private static string GetString(IDictionary<string, object> values, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (values.TryGetValue(key, out var value) && value is not null && value is not DBNull)
                {
                    return Convert.ToString(value)?.Trim() ?? string.Empty;
                }

                var match = values.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(match.Key) && match.Value is not null && match.Value is not DBNull)
                {
                    return Convert.ToString(match.Value)?.Trim() ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static DateTime GetDate(IDictionary<string, object> values, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (values.TryGetValue(key, out var value) && value is not null && value is not DBNull)
                {
                    return Convert.ToDateTime(value);
                }

                var match = values.FirstOrDefault(item => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(match.Key) && match.Value is not null && match.Value is not DBNull)
                {
                    return Convert.ToDateTime(match.Value);
                }
            }

            return default;
        }

        /// <summary>
        /// Agrega una etiqueta de seguimiento con nota al expediente. Fiel a VB6
        /// btnEtiqueta_Click (frmPreaEstudiov2.frm línea ~13247): exec spCrdPreaAgregaEtiqueta
        /// '&lt;expediente&gt;', '&lt;etiqueta&gt;', '&lt;nota&gt;', '&lt;usuario&gt;', luego recarga
        /// el historial (sbHistorial_Load("E")).
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HistorialResponse> Prea_frmPreaEstudiov2_Etiqueta_Agregar(
            int codEmpresa,
            FrmPreaEstudiov2EtiquetaAgregarRequest request)
        {
            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = "EXEC spCrdPreaAgregaEtiqueta @Expediente, @Etiqueta, @Nota, @Usuario";
                connection.Execute(sql, new
                {
                    Expediente = request.cod_preanalisis.Trim(),
                    Etiqueta = (request.cod_etiqueta ?? string.Empty).Trim(),
                    Nota = (request.nota ?? string.Empty).Trim(),
                    Usuario = (request.usuario ?? string.Empty).Trim()
                });
            }
            catch (Exception ex)
            {
                return new ErrorDto<FrmPreaEstudiov2HistorialResponse>
                {
                    Code = -1,
                    Description = ex.Message,
                    Result = new FrmPreaEstudiov2HistorialResponse()
                };
            }

            return Prea_frmPreaEstudiov2_Historial_Consultar(codEmpresa, request.cod_preanalisis);
        }
    }
}
