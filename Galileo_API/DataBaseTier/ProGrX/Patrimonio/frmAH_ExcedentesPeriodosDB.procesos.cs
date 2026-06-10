using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesPeriodosDB
    {
        public const string validaSolicitud = "La solicitud es requerida.";
        public const string indicaPeriodo = "Debe indicar el período.";
        public const string indicaUsuario = "Debe indicar el usuario.";
        public const string periodoNoExiste = "El período indicado no existe.";

        /// <summary>
        /// Actualiza el modo de aplicación mensual del período.
        /// Equivale al botón Actualiza del VB6.
        /// </summary>
        public ErrorDto<bool> Ah_ExcedentesPeriodos_BaseAplicacion_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosBaseAplicacionRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(validaSolicitud, -2, false);
            }

            if (request.id_periodo <= 0)
            {
                return DbHelper.CreateErrorResponse(indicaPeriodo, -2, false);
            }

            var usuarioNormalizado = Ah_ExcedentesPeriodos_NormalizarTexto(request.usuario);
            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse(indicaUsuario, -2, false);
            }

            var tipoAplicacionNormalizado = Ah_ExcedentesPeriodos_NormalizarTipoAplicacion(request.tipo_apl_mensual);

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var periodo = Ah_ExcedentesPeriodos_ObtenerEstadoPeriodo(conn, request.id_periodo);
                if (periodo == null)
                {
                    return DbHelper.CreateErrorResponse(periodoNoExiste, -2, false);
                }

                if (periodo.estado == "C")
                {
                    return DbHelper.CreateErrorResponse("El período ya fue cerrado.", -2, false);
                }

                conn.Execute(
                    "spExc_Periodo_Modo_Aplicacion",
                    new
                    {
                        PeriodoId = request.id_periodo,
                        Tipo = tipoAplicacionNormalizado,
                        Usuario = usuarioNormalizado
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, false);
            }
        }

        /// <summary>
        /// Actualiza la nota del estado de excedentes del período.
        /// Equivale al botón de guardar nota del VB6.
        /// </summary>
        public ErrorDto<bool> Ah_ExcedentesPeriodos_EstadoNota_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosEstadoNotaRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(validaSolicitud, -2, false);
            }

            if (request.id_periodo <= 0)
            {
                return DbHelper.CreateErrorResponse(indicaPeriodo, -2, false);
            }

            var usuarioNormalizado = Ah_ExcedentesPeriodos_NormalizarTexto(request.usuario);
            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse(indicaUsuario, -2, false);
            }

            var notaNormalizada = Ah_ExcedentesPeriodos_NormalizarTexto(request.estado_notas);

            const string sql = @"
update EXC_PERIODOS
set ESTADO_NOTAS = @EstadoNotas
where ID_PERIODO = @PeriodoId;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                if (!Ah_ExcedentesPeriodos_Existe(conn, request.id_periodo))
                {
                    return DbHelper.CreateErrorResponse(periodoNoExiste, -2, false);
                }

                conn.Execute(
                    sql,
                    new
                    {
                        EstadoNotas = notaNormalizada,
                        PeriodoId = request.id_periodo
                    });

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, false);
            }
        }

        /// <summary>
        /// Recalcula la base de datos para cálculo de excedentes del período.
        /// Equivale al botón Recalcular la Base del VB6.
        /// </summary>
        public ErrorDto<bool> Ah_ExcedentesPeriodos_RecalcularBase(
            int codEmpresa,
            FrmAhExcedentesPeriodosRecalcularBaseRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(validaSolicitud, -2, false);
            }

            if (request.id_periodo <= 0)
            {
                return DbHelper.CreateErrorResponse(indicaPeriodo, -2, false);
            }

            var usuarioNormalizado = Ah_ExcedentesPeriodos_NormalizarTexto(request.usuario);
            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse(indicaUsuario, -2, false);
            }

            const string sqlCortes = @"
select dbo.fxSys_FechaAnioMesToDatetime(H.ANIO, H.MES) as corte
from EXC_PERIODOS E
inner join ASE_PER_HISTORICO H
    on dbo.fxSys_FechaAnioMesToDatetime(H.ANIO, H.MES) between E.INICIO and E.CORTE
where E.ID_PERIODO = @PeriodoId
  and E.ESTADO = 'A'
order by corte;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var periodo = Ah_ExcedentesPeriodos_ObtenerEstadoPeriodo(conn, request.id_periodo);
                if (periodo == null)
                {
                    return DbHelper.CreateErrorResponse(periodoNoExiste, -2, false);
                }

                if (periodo.estado == "C")
                {
                    return DbHelper.CreateErrorResponse("El período ya fue cerrado.", -2, false);
                }

                var cortes = conn.Query<DateTime>(
                    sqlCortes,
                    new { PeriodoId = request.id_periodo }).ToList();

                if (cortes.Count == 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "El período no tiene cortes abiertos para recalcular.",
                        -2,
                        false);
                }

                foreach (var corte in cortes)
                {
                    conn.Execute(
                        "spSIFAuxExcedentes_WLog",
                        new
                        {
                            Anio = corte.Year,
                            Mes = corte.Month
                        },
                        commandType: System.Data.CommandType.StoredProcedure);
                }

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, false);
            }
        }

        private static FrmAhExcedentesPeriodosEstadoInternoDto? Ah_ExcedentesPeriodos_ObtenerEstadoPeriodo(
            SqlConnection conn,
            int periodoId)
        {
            const string sql = @"
select
    isnull(ID_PERIODO, 0) as id_periodo,
    rtrim(isnull(ESTADO, '')) as estado
from EXC_PERIODOS
where ID_PERIODO = @PeriodoId;";

            return conn.QueryFirstOrDefault<FrmAhExcedentesPeriodosEstadoInternoDto>(
                sql,
                new { PeriodoId = periodoId });
        }


        /// <summary>
        /// Actualiza una bandera de visibilidad del estado de excedentes.
        /// Equivale a marcar/desmarcar los checks del tab Estado Excedentes del VB6.
        /// </summary>
        public ErrorDto<bool> Ah_ExcedentesPeriodos_Visibilidad_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosVisibilidadRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse(validaSolicitud, -2, false);
            }

            if (request.id_periodo <= 0)
            {
                return DbHelper.CreateErrorResponse(indicaPeriodo, -2, false);
            }

            var usuarioNormalizado = Ah_ExcedentesPeriodos_NormalizarTexto(request.usuario);
            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse(indicaUsuario, -2, false);
            }

            var columna = Ah_ExcedentesPeriodos_ObtenerColumnaVisibilidad(request.campo);
            if (string.IsNullOrWhiteSpace(columna))
            {
                return DbHelper.CreateErrorResponse("La opción de visibilidad indicada no es válida.", -2, false);
            }

            // La columna proviene de una lista blanca cerrada.
            // No existe entrada libre del usuario en la consulta SQL.
            var sql = $@"
update EXC_PERIODOS
set {columna} = @Valor
where ID_PERIODO = @PeriodoId;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var periodo = Ah_ExcedentesPeriodos_ObtenerEstadoPeriodo(conn, request.id_periodo);
                if (periodo == null)
                {
                    return DbHelper.CreateErrorResponse(periodoNoExiste, -2, false);
                }

                if (periodo.estado != "C")
                {
                    return DbHelper.CreateErrorResponse(
                        "Solo se permite modificar la visibilidad de períodos cerrados.",
                        -2,
                        false);
                }

                conn.Execute(
                    sql,
                    new
                    {
                        Valor = request.valor,
                        PeriodoId = request.id_periodo
                    });

                Ah_ExcedentesPeriodos_RegistrarBitacoraSeguridad(
                    codEmpresa,
                    usuarioNormalizado,
                    "Actualiza visibilidad de excedentes",
                    $"Período: {request.id_periodo}. Campo: {columna}. Valor: {request.valor}");

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, false);
            }
        }

        private static string Ah_ExcedentesPeriodos_ObtenerColumnaVisibilidad(string? campo)
        {
            return Ah_ExcedentesPeriodos_NormalizarTexto(campo).ToLowerInvariant() switch
            {
                "visible_webapp" => "VISIBLE_WEBAPP",
                "visible_sys" => "VISIBLE_SYS",
                "mostrar_en_historial" => "MOSTRAR_EN_HISTORIAL",
                "mostrar_tabla_renta" => "MOSTRAR_TABLA_RENTA",
                _ => string.Empty
            };
        }


    }
}
