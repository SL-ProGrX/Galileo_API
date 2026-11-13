using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrEnCobroCuotasDB
    {
        /// <summary>
        /// Obtiene la lista de resumen comparativo por operación y el total completo enviado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa sobre la que se realiza la consulta.</param>
        /// <param name="request">Institución, proceso, cédula y parámetros de paginación.</param>
        /// <returns>Resumen paginado con los totales completos del proceso.</returns>
        public ErrorDto<CrEnCobroCuotasListaResult<CrEnCobroCuotasResumenData>> CR_EnCobroCuotas_Resumen_Lista_Obtener(
            int CodEmpresa,
            CrEnCobroCuotasConsultaRequest request)
        {
            var ctxResult = BuildContext(request);
            if (ctxResult.errorResumen != null) return ctxResult.errorResumen;

            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sqlResumen = "exec spPrm_Compara_Persona @Institucion, @Proceso, @Cedula;";
                const string sqlTotalEnviado = @"
                    SELECT ISNULL(SUM(C.cuota), 0)
                    FROM PRM_ENVIADO_DETALLE C
                    WHERE C.fecpro = @Proceso
                      AND C.cedula = @Cedula
                      AND C.cod_Institucion = @Institucion;";

                var parametros = new
                {
                    Institucion = ctxResult.ctx.codInstitucion,
                    Proceso = ctxResult.ctx.proceso,
                    Cedula = ctxResult.ctx.cedula
                };

                var lista = conn.Query<CrEnCobroCuotasResumenRow>(sqlResumen, parametros)
                    .Select(x => new CrEnCobroCuotasResumenData
                    {
                        operacion = x.Operacion,
                        linea = (x.Linea ?? string.Empty).Trim(),
                        envio = x.Envio,
                        recibido = x.Recibido,
                        diferencia = x.Recibido - x.Envio,
                        tipo_desc = (x.TipoDesc ?? string.Empty).Trim(),
                        linea_desc = (x.LineaDesc ?? string.Empty).Trim()
                    })
                    .ToList();

                // La pestaña Envío del ProGrX original totaliza directamente PRM_ENVIADO_DETALLE.
                // El resumen debe usar esa misma fuente para incluir cuotas ordinarias y de morosidad.
                var totalEnviadoCompleto = conn.ExecuteScalar<decimal>(sqlTotalEnviado, parametros);

                return BuildResumenResult(
                    lista,
                    ctxResult.ctx.filtros,
                    totalEnviadoCompleto);
            }
            catch (SqlException ex)
            {
                return ErrorLista<CrEnCobroCuotasResumenData>(ex.Message);
            }
        }

        private sealed class CrEnCobroCuotasResumenRow
        {
            public int Operacion { get; set; }
            public string Linea { get; set; } = string.Empty;
            public decimal Envio { get; set; }
            public decimal Recibido { get; set; }
            public string TipoDesc { get; set; } = string.Empty;
            public string LineaDesc { get; set; } = string.Empty;
        }
    }
}
