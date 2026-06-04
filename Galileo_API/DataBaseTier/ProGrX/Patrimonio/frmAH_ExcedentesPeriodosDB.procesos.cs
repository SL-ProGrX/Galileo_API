using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesPeriodosDB
    {
        /// <summary>
        /// Actualiza el modo de aplicación mensual del período.
        /// Equivale al botón Actualiza del VB6.
        /// </summary>
        public ErrorDto<bool> Patrimonio_frmAH_ExcedentesPeriodos_BaseAplicacion_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosBaseAplicacionRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse("La solicitud es requerida.", -2, false);
            }

            if (request.id_periodo <= 0)
            {
                return DbHelper.CreateErrorResponse("Debe indicar el período.", -2, false);
            }

            var usuarioNormalizado = Patrimonio_frmAH_ExcedentesPeriodos_NormalizarTexto(request.usuario);
            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el usuario.", -2, false);
            }

            var tipoAplicacionNormalizado = Patrimonio_frmAH_ExcedentesPeriodos_NormalizarTipoAplicacion(request.tipo_apl_mensual);

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var periodo = Patrimonio_frmAH_ExcedentesPeriodos_ObtenerEstadoPeriodo(conn, request.id_periodo);
                if (periodo == null)
                {
                    return DbHelper.CreateErrorResponse("El período indicado no existe.", -2, false);
                }

                if (periodo.estado == "C")
                {
                    return DbHelper.CreateErrorResponse("El período ya fue cerrado.", -2, false);
                }

                conn.Execute(
                    "spExc_Periodo_Modo_Aplicacion",
                    new
                    {
                        pPeriodoId = request.id_periodo,
                        pTipoAplicacion = tipoAplicacionNormalizado,
                        pUsuario = usuarioNormalizado
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
        public ErrorDto<bool> Patrimonio_frmAH_ExcedentesPeriodos_EstadoNota_Actualizar(
            int codEmpresa,
            FrmAhExcedentesPeriodosEstadoNotaRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse("La solicitud es requerida.", -2, false);
            }

            if (request.id_periodo <= 0)
            {
                return DbHelper.CreateErrorResponse("Debe indicar el período.", -2, false);
            }

            var usuarioNormalizado = Patrimonio_frmAH_ExcedentesPeriodos_NormalizarTexto(request.usuario);
            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el usuario.", -2, false);
            }

            var notaNormalizada = Patrimonio_frmAH_ExcedentesPeriodos_NormalizarTexto(request.estado_notas);

            const string sql = @"
update EXC_PERIODOS
set ESTADO_NOTAS = @EstadoNotas
where ID_PERIODO = @PeriodoId;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                if (!Patrimonio_frmAH_ExcedentesPeriodos_Existe(conn, request.id_periodo))
                {
                    return DbHelper.CreateErrorResponse("El período indicado no existe.", -2, false);
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
        public ErrorDto<bool> Patrimonio_frmAH_ExcedentesPeriodos_RecalcularBase(
            int codEmpresa,
            FrmAhExcedentesPeriodosRecalcularBaseRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse("La solicitud es requerida.", -2, false);
            }

            if (request.id_periodo <= 0)
            {
                return DbHelper.CreateErrorResponse("Debe indicar el período.", -2, false);
            }

            var usuarioNormalizado = Patrimonio_frmAH_ExcedentesPeriodos_NormalizarTexto(request.usuario);
            if (string.IsNullOrWhiteSpace(usuarioNormalizado))
            {
                return DbHelper.CreateErrorResponse("Debe indicar el usuario.", -2, false);
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

                var periodo = Patrimonio_frmAH_ExcedentesPeriodos_ObtenerEstadoPeriodo(conn, request.id_periodo);
                if (periodo == null)
                {
                    return DbHelper.CreateErrorResponse("El período indicado no existe.", -2, false);
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
                            pAnio = corte.Year,
                            pMes = corte.Month
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

        private static FrmAhExcedentesPeriodosEstadoInternoDto? Patrimonio_frmAH_ExcedentesPeriodos_ObtenerEstadoPeriodo(
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

       
    }
}
