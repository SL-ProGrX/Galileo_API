using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndLiquidacionPlanDB
    {
        private const int FndLiquidacionPlanTamanoLote = 200;
        private const string FndLiquidacionPlanEstadoPendiente = "P";
        private const string FndLiquidacionPlanEstadoCompletado = "C";

        /// <summary>
        /// Crea el manifiesto persistente de una liquidación o recupera el proceso pendiente
        /// del mismo usuario, operadora y plan.
        /// </summary>
        public ErrorDto<FndLiquidacionPlanProcesoResult> FND_LiquidacionPlan_Proceso_Iniciar(
            int codEmpresa,
            FndLiquidacionPlanProcesoIniciarRequest request)
        {
            SqlConnection? conn = null;
            SqlTransaction? tx = null;

            try
            {
                string? validacionBasica = ValidarInicioBasico(request);
                if (!string.IsNullOrWhiteSpace(validacionBasica))
                    return DbHelper.CreateErrorResponse<FndLiquidacionPlanProcesoResult>(validacionBasica);

                conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                var globales = mProGrx.sbSifParametrosInicializa(
                    codEmpresa,
                    request.usuario,
                    request.codContabilidad).Result;

                request.oficinaTitular = globales!.GOficinaTitular;
                request.oficinaUnidad = globales.GOficinaUnidad;
                request.oficinaCentroCosto = globales.GOficinaCentroCosto;

                string? validacion = ValidarLiquidacion(request);
                if (!string.IsNullOrWhiteSpace(validacion))
                    return DbHelper.CreateErrorResponse<FndLiquidacionPlanProcesoResult>(validacion);
                if (request.contratos.Count == 0)
                    return DbHelper.CreateErrorResponse<FndLiquidacionPlanProcesoResult>(
                        "Debe seleccionar al menos un contrato.");

                tx = conn.BeginTransaction();
                int codOperadora = int.Parse(request.cod_operadora);
                string solicitudHash = CalcularHashSolicitud(request);
                var procesoSolicitado = ObtenerProcesoPorId(
                    conn,
                    tx,
                    request.procesoId,
                    request.usuario);
                if (procesoSolicitado != null)
                {
                    ValidarContextoProcesoSolicitado(procesoSolicitado, codOperadora, request.cod_plan);
                    if (procesoSolicitado.estado == FndLiquidacionPlanEstadoPendiente ||
                        string.Equals(
                            procesoSolicitado.solicitud_hash,
                            solicitudHash,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        var resultadoSolicitado = ObtenerResultadoProceso(conn, tx, procesoSolicitado);
                        tx.Commit();
                        return DbHelper.CreateOkResponse(resultadoSolicitado);
                    }

                    request.procesoId = Guid.NewGuid();
                }

                var procesoActivo = ObtenerProcesoActivo(
                    conn,
                    tx,
                    codOperadora,
                    request.cod_plan,
                    request.usuario);

                if (procesoActivo != null)
                {
                    var resultadoActivo = ObtenerResultadoProceso(conn, tx, procesoActivo);
                    tx.Commit();
                    return DbHelper.CreateOkResponse(resultadoActivo);
                }

                var contexto = CrearContextoProceso(
                    conn,
                    tx,
                    request,
                    codOperadora,
                    solicitudHash);
                InsertarProceso(conn, tx, contexto);
                InsertarDetalleProceso(conn, tx, request, contexto.proceso_id);

                var resultado = ObtenerResultadoProceso(conn, tx, contexto);
                tx.Commit();
                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception ex)
            {
                RollbackSeguro(tx);
                Trace.TraceError("FND_LiquidacionPlan_Proceso_Iniciar: {0}", ex);
                return DbHelper.CreateErrorResponse<FndLiquidacionPlanProcesoResult>(
                    "No fue posible iniciar la liquidación del plan.");
            }
            finally
            {
                tx?.Dispose();
                conn?.Dispose();
            }
        }

        /// <summary>
        /// Procesa el siguiente lote pendiente del manifiesto. El bloqueo del encabezado
        /// serializa reintentos y evita que dos solicitudes ejecuten el mismo lote.
        /// </summary>
        public ErrorDto<FndLiquidacionPlanProcesoResult> FND_LiquidacionPlan_Proceso_Continuar(
            int codEmpresa,
            FndLiquidacionPlanProcesoContinuarRequest request)
        {
            SqlConnection? conn = null;
            SqlTransaction? tx = null;

            try
            {
                string? validacion = ValidarContinuacion(request);
                if (!string.IsNullOrWhiteSpace(validacion))
                    return DbHelper.CreateErrorResponse<FndLiquidacionPlanProcesoResult>(validacion);

                conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();
                tx = conn.BeginTransaction();

                var contexto = ObtenerProcesoParaActualizar(conn, tx, request.procesoId, request.usuario);
                if (contexto == null)
                    throw new InvalidOperationException("No se encontró el proceso de liquidación.");

                if (contexto.estado == FndLiquidacionPlanEstadoCompletado)
                {
                    var completado = ObtenerResultadoProceso(conn, tx, contexto);
                    tx.Commit();
                    return DbHelper.CreateOkResponse(completado);
                }

                var contratos = ObtenerSiguienteLote(conn, tx, contexto.proceso_id);
                ProcesarSiguienteLote(conn, tx, contexto, contratos);

                var resultado = ObtenerResultadoProceso(conn, tx, contexto);
                FinalizarProcesoSiCorresponde(conn, tx, contexto, resultado);
                ActualizarAvanceProceso(conn, tx, resultado);

                tx.Commit();
                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception ex)
            {
                RollbackSeguro(tx);
                Trace.TraceError("FND_LiquidacionPlan_Proceso_Continuar {0}: {1}", request.procesoId, ex);
                return DbHelper.CreateErrorResponse<FndLiquidacionPlanProcesoResult>(
                    "No fue posible procesar el siguiente lote de la liquidación.");
            }
            finally
            {
                tx?.Dispose();
                conn?.Dispose();
            }
        }

        private static string? ValidarInicioBasico(FndLiquidacionPlanProcesoIniciarRequest request)
        {
            if (request == null) return "La solicitud es requerida.";
            if (string.IsNullOrWhiteSpace(request.cod_operadora)) return "La operadora es requerida.";
            if (!int.TryParse(request.cod_operadora, out _)) return "La operadora no es válida.";
            if (string.IsNullOrWhiteSpace(request.cod_plan)) return "El plan es requerido.";
            if (string.IsNullOrWhiteSpace(request.usuario)) return "El usuario es requerido.";
            if (request.procesoId == Guid.Empty) return "El identificador del proceso es requerido.";
            return null;
        }

        private static string? ValidarContinuacion(FndLiquidacionPlanProcesoContinuarRequest request)
        {
            if (request == null) return "La solicitud es requerida.";
            if (request.procesoId == Guid.Empty) return "El identificador del proceso es requerido.";
            if (string.IsNullOrWhiteSpace(request.usuario)) return "El usuario es requerido.";
            return null;
        }

        private static FndLiquidacionPlanProcesoContexto? ObtenerProcesoActivo(
            SqlConnection conn,
            SqlTransaction tx,
            int codOperadora,
            string codPlan,
            string usuario)
        {
            const string sql = @"
                select top 1 *
                from dbo.FND_LIQUIDACION_PROCESO with (UPDLOCK, HOLDLOCK)
                where COD_OPERADORA = @codOperadora
                  and COD_PLAN = @codPlan
                  and USUARIO = @usuario
                  and ESTADO = 'P'
                order by REGISTRO_FECHA desc";

            return conn.QueryFirstOrDefault<FndLiquidacionPlanProcesoContexto>(sql, new
            {
                codOperadora,
                codPlan = codPlan.Trim(),
                usuario = usuario.Trim()
            }, tx);
        }

        private static FndLiquidacionPlanProcesoContexto? ObtenerProcesoPorId(
            SqlConnection conn,
            SqlTransaction tx,
            Guid procesoId,
            string usuario)
        {
            const string sql = @"
                select *
                from dbo.FND_LIQUIDACION_PROCESO with (UPDLOCK, HOLDLOCK)
                where PROCESO_ID = @procesoId
                  and USUARIO = @usuario";

            return conn.QuerySingleOrDefault<FndLiquidacionPlanProcesoContexto>(sql, new
            {
                procesoId,
                usuario = usuario.Trim()
            }, tx);
        }

        private static void ValidarContextoProcesoSolicitado(
            FndLiquidacionPlanProcesoContexto contexto,
            int codOperadora,
            string codPlan)
        {
            if (contexto.cod_operadora != codOperadora ||
                !string.Equals(contexto.cod_plan, codPlan.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "El proceso indicado no corresponde a la operadora y plan seleccionados.");
            }
        }

        private static FndLiquidacionPlanProcesoContexto CrearContextoProceso(
            SqlConnection conn,
            SqlTransaction tx,
            FndLiquidacionPlanProcesoIniciarRequest request,
            int codOperadora,
            string solicitudHash)
        {
            var plan = ObtenerPlanInfo(conn, tx, codOperadora, request.cod_plan);
            var operadora = ObtenerOperadoraInfo(conn, tx, codOperadora);
            if (string.IsNullOrWhiteSpace(plan.cod_moneda))
                throw new InvalidOperationException("No se encontró la configuración del plan.");

            string cuentaLiquidacion = EsRetener(request.proceso)
                ? ObtenerCuentaRetencion(conn, tx, request.retencionCodigo)
                : operadora.cta_retiros;
            if (string.IsNullOrWhiteSpace(cuentaLiquidacion))
                throw new InvalidOperationException("No se encontró la cuenta de liquidación.");

            var correlativo = ObtenerDocumentoReferencia(conn, tx, codOperadora, request.cod_plan);
            return new FndLiquidacionPlanProcesoContexto
            {
                proceso_id = request.procesoId,
                cod_operadora = codOperadora,
                cod_plan = request.cod_plan.Trim(),
                documento_referencia = $"{request.cod_plan.Trim()}.{correlativo.consecutivo:000}_{correlativo.fecha:yyyy.MM.dd}",
                fecha_proceso = correlativo.fecha,
                proceso_codigo = ObtenerCodigoProceso(request.proceso),
                tipo_documento = ObtenerCodigoTipoDocumentoBancario(request.tipoDocumento),
                tipo_liquidacion = ObtenerCodigoTipo(request.tipo),
                usuario = request.usuario.Trim(),
                oficina_titular = request.oficinaTitular.Trim(),
                oficina_unidad = request.oficinaUnidad.Trim(),
                oficina_centro_costo = request.oficinaCentroCosto.Trim(),
                enlace = request.enlace,
                multa = request.multa,
                notas = LimitarTexto(request.notas, 1000),
                retencion_codigo = request.retencionCodigo?.Trim() ?? string.Empty,
                cuenta_liquidacion = cuentaLiquidacion,
                fecha_vence = request.fechaVence,
                cod_contabilidad = request.codContabilidad,
                estado = FndLiquidacionPlanEstadoPendiente,
                solicitud_hash = solicitudHash,
                total_contratos = request.contratos.Select(item => item.cod_contrato).Distinct().Count()
            };
        }

        private static void InsertarProceso(
            SqlConnection conn,
            SqlTransaction tx,
            FndLiquidacionPlanProcesoContexto contexto)
        {
            const string sql = @"
                insert into dbo.FND_LIQUIDACION_PROCESO
                (
                    PROCESO_ID, COD_OPERADORA, COD_PLAN, DOCUMENTO_REFERENCIA, FECHA_PROCESO,
                    PROCESO_CODIGO, TIPO_DOCUMENTO, TIPO_LIQUIDACION, USUARIO,
                    OFICINA_TITULAR, OFICINA_UNIDAD, OFICINA_CENTRO_COSTO, ENLACE,
                    MULTA, NOTAS, RETENCION_CODIGO, CUENTA_LIQUIDACION, FECHA_VENCE,
                    COD_CONTABILIDAD, ESTADO, TOTAL_CONTRATOS, PROCESADOS,
                    SOLICITUD_HASH, REGISTRO_FECHA, ACTUALIZACION_FECHA
                )
                values
                (
                    @proceso_id, @cod_operadora, @cod_plan, @documento_referencia, @fecha_proceso,
                    @proceso_codigo, @tipo_documento, @tipo_liquidacion, @usuario,
                    @oficina_titular, @oficina_unidad, @oficina_centro_costo, @enlace,
                    @multa, @notas, @retencion_codigo, @cuenta_liquidacion, @fecha_vence,
                    @cod_contabilidad, 'P', @total_contratos, 0,
                    @solicitud_hash, dbo.MyGetdate(), dbo.MyGetdate()
                )";

            conn.Execute(sql, contexto, tx);
        }

        private static void InsertarDetalleProceso(
            SqlConnection conn,
            SqlTransaction tx,
            FndLiquidacionPlanLiquidarRequest request,
            Guid procesoId)
        {
            const string sql = @"
                insert into dbo.FND_LIQUIDACION_PROCESO_DET
                (PROCESO_ID, COD_CONTRATO, APORTES, RENDIMIENTO, BANCO_FINAL, CUENTA_FINAL, ESTADO)
                values
                (@procesoId, @cod_contrato, @aportes, @rendimiento, @bancofinal, @cuentafinal, 'P')";

            var contratos = request.contratos
                .GroupBy(item => item.cod_contrato)
                .Select(grupo => grupo.First())
                .Select(item => new
                {
                    procesoId,
                    item.cod_contrato,
                    item.aportes,
                    item.rendimiento,
                    item.bancofinal,
                    item.cuentafinal
                });

            conn.Execute(sql, contratos, tx);
        }

        private static FndLiquidacionPlanProcesoContexto? ObtenerProcesoParaActualizar(
            SqlConnection conn,
            SqlTransaction tx,
            Guid procesoId,
            string usuario)
        {
            const string sql = @"
                select *
                from dbo.FND_LIQUIDACION_PROCESO with (UPDLOCK, HOLDLOCK)
                where PROCESO_ID = @procesoId
                  and USUARIO = @usuario";

            return conn.QuerySingleOrDefault<FndLiquidacionPlanProcesoContexto>(sql, new
            {
                procesoId,
                usuario = usuario.Trim()
            }, tx);
        }

        private static List<FndLiquidacionPlanProcesoDetalle> ObtenerSiguienteLote(
            SqlConnection conn,
            SqlTransaction tx,
            Guid procesoId)
        {
            const string sql = @"
                select top (@tamanoLote)
                    COD_CONTRATO as cod_contrato,
                    APORTES as aportes,
                    RENDIMIENTO as rendimiento,
                    BANCO_FINAL as bancofinal,
                    CUENTA_FINAL as cuentafinal
                from dbo.FND_LIQUIDACION_PROCESO_DET with (UPDLOCK, ROWLOCK)
                where PROCESO_ID = @procesoId
                  and ESTADO = 'P'
                order by COD_CONTRATO";

            return conn.Query<FndLiquidacionPlanProcesoDetalle>(sql, new
            {
                procesoId,
                tamanoLote = FndLiquidacionPlanTamanoLote
            }, tx).ToList();
        }

        private static void ProcesarSiguienteLote(
            SqlConnection conn,
            SqlTransaction tx,
            FndLiquidacionPlanProcesoContexto contexto,
            IEnumerable<FndLiquidacionPlanProcesoDetalle> contratos)
        {
            foreach (var contrato in contratos)
            {
                EjecutarLiquidacionComplementaria(conn, tx, new
                {
                    Operadora = contexto.cod_operadora,
                    Plan = contexto.cod_plan,
                    Contrato = contrato.cod_contrato,
                    Tipo = "L",
                    TipoDoc = "FLIQ",
                    Concepto = "FND006",
                    DocRef = contexto.documento_referencia,
                    AporteLiq = contrato.aportes,
                    RendiLiq = contrato.rendimiento,
                    Multa = contexto.multa,
                    Notas = contexto.notas,
                    Usuario = contexto.usuario,
                    OficinaTitular = contexto.oficina_titular,
                    ProcesoCodigo = contexto.proceso_codigo,
                    RetencionCodigo = contexto.retencion_codigo,
                    CuentaLiquidacion = contexto.cuenta_liquidacion,
                    Banco = ParseBanco(contrato.bancofinal),
                    BancoTipo = contexto.tipo_documento,
                    CuentaAhorros = contrato.cuentafinal,
                    Origen = "ProGrX",
                    TipoLiquidacion = contexto.tipo_liquidacion,
                    FechaVence = contexto.fecha_vence?.Date ?? contexto.fecha_proceso.Date
                });

                MarcarContratoProcesado(conn, tx, contexto.proceso_id, contrato.cod_contrato);
            }
        }

        private static void MarcarContratoProcesado(
            SqlConnection conn,
            SqlTransaction tx,
            Guid procesoId,
            long codContrato)
        {
            const string sql = @"
                update dbo.FND_LIQUIDACION_PROCESO_DET
                set ESTADO = 'C', PROCESO_FECHA = dbo.MyGetdate()
                where PROCESO_ID = @procesoId
                  and COD_CONTRATO = @codContrato
                  and ESTADO = 'P'";

            conn.Execute(sql, new { procesoId, codContrato }, tx);
        }

        private static FndLiquidacionPlanProcesoResult ObtenerResultadoProceso(
            SqlConnection conn,
            SqlTransaction tx,
            FndLiquidacionPlanProcesoContexto contexto)
        {
            const string sql = @"
                select
                    count(1) as totalContratos,
                    sum(case when ESTADO = 'C' then 1 else 0 end) as contratosProcesados,
                    sum(case when ESTADO = 'P' then 1 else 0 end) as contratosPendientes,
                    isnull(sum(case when ESTADO = 'C' then APORTES else 0 end), 0) as totalAportes,
                    isnull(sum(case when ESTADO = 'C' then RENDIMIENTO else 0 end), 0) as totalRendimientos,
                    isnull(sum(case when ESTADO = 'C' then APORTES + RENDIMIENTO else 0 end), 0) as totalGeneral
                from dbo.FND_LIQUIDACION_PROCESO_DET
                where PROCESO_ID = @procesoId";

            var resultado = conn.QuerySingle<FndLiquidacionPlanProcesoResult>(
                sql,
                new { procesoId = contexto.proceso_id },
                tx);
            resultado.procesoId = contexto.proceso_id;
            resultado.documentoReferencia = contexto.documento_referencia;
            resultado.fecha = contexto.fecha_proceso;
            resultado.porcentaje = contexto.total_contratos == 0
                ? 0
                : Math.Round(resultado.contratosProcesados * 100m / contexto.total_contratos, 2);
            resultado.procesoFinalizado = contexto.estado == FndLiquidacionPlanEstadoCompletado;
            return resultado;
        }

        private static void FinalizarProcesoSiCorresponde(
            SqlConnection conn,
            SqlTransaction tx,
            FndLiquidacionPlanProcesoContexto contexto,
            FndLiquidacionPlanProcesoResult resultado)
        {
            if (resultado.contratosPendientes > 0) return;
            if (!DocumentoGeneralProcesoExiste(conn, tx, contexto.documento_referencia))
            {
                var request = CrearRequestDesdeContexto(contexto);
                CrearDocumentoGeneral(new CrearDocumentoGeneralParametros
                {
                    conn = conn,
                    tx = tx,
                    codOperador = contexto.cod_operadora,
                    request = request,
                    plan = ObtenerPlanInfo(conn, tx, contexto.cod_operadora, contexto.cod_plan),
                    operadora = ObtenerOperadoraInfo(conn, tx, contexto.cod_operadora),
                    docRef = contexto.documento_referencia,
                    tipoDoc = "FLIQ",
                    concepto = "FND006"
                });
            }

            resultado.procesoFinalizado = true;
            resultado.porcentaje = 100;
        }

        private static FndLiquidacionPlanLiquidarRequest CrearRequestDesdeContexto(
            FndLiquidacionPlanProcesoContexto contexto) => new()
            {
                cod_operadora = contexto.cod_operadora.ToString(),
                cod_plan = contexto.cod_plan,
                proceso = contexto.proceso_codigo,
                tipoDocumento = contexto.tipo_documento,
                tipo = contexto.tipo_liquidacion,
                usuario = contexto.usuario,
                oficinaTitular = contexto.oficina_titular,
                oficinaUnidad = contexto.oficina_unidad,
                oficinaCentroCosto = contexto.oficina_centro_costo,
                enlace = contexto.enlace,
                multa = contexto.multa,
                notas = contexto.notas,
                retencionCodigo = contexto.retencion_codigo,
                fechaVence = contexto.fecha_vence,
                codContabilidad = contexto.cod_contabilidad
            };

        private static bool DocumentoGeneralProcesoExiste(
            SqlConnection conn,
            SqlTransaction tx,
            string documentoReferencia)
        {
            const string sql = @"
                select count(1)
                from SIF_TRANSACCIONES
                where COD_TRANSACCION = @documentoReferencia
                  and TIPO_DOCUMENTO = 'FLIQ'";
            return conn.ExecuteScalar<int>(sql, new { documentoReferencia }, tx) > 0;
        }

        private static void ActualizarAvanceProceso(
            SqlConnection conn,
            SqlTransaction tx,
            FndLiquidacionPlanProcesoResult resultado)
        {
            const string sql = @"
                update dbo.FND_LIQUIDACION_PROCESO
                set PROCESADOS = @contratosProcesados,
                    ESTADO = @estado,
                    ERROR_MENSAJE = null,
                    ACTUALIZACION_FECHA = dbo.MyGetdate()
                where PROCESO_ID = @procesoId";

            conn.Execute(sql, new
            {
                resultado.procesoId,
                resultado.contratosProcesados,
                estado = resultado.procesoFinalizado
                    ? FndLiquidacionPlanEstadoCompletado
                    : FndLiquidacionPlanEstadoPendiente
            }, tx);
        }

        private static string CalcularHashSolicitud(FndLiquidacionPlanProcesoIniciarRequest request)
        {
            var contenido = new
            {
                cod_operadora = request.cod_operadora.Trim(),
                cod_plan = request.cod_plan.Trim(),
                proceso = ObtenerCodigoProceso(request.proceso),
                tipo_documento = ObtenerCodigoTipoDocumentoBancario(request.tipoDocumento),
                tipo_liquidacion = ObtenerCodigoTipo(request.tipo),
                request.enlace,
                request.codContabilidad,
                request.multa,
                notas = LimitarTexto(request.notas, 1000),
                retencion_codigo = request.retencionCodigo?.Trim() ?? string.Empty,
                fecha_vence = request.fechaVence?.Date,
                oficina_titular = request.oficinaTitular.Trim(),
                oficina_unidad = request.oficinaUnidad.Trim(),
                oficina_centro_costo = request.oficinaCentroCosto.Trim(),
                contratos = request.contratos
                    .GroupBy(item => item.cod_contrato)
                    .Select(grupo => grupo.First())
                    .OrderBy(item => item.cod_contrato)
                    .Select(item => new
                    {
                        item.cod_contrato,
                        item.aportes,
                        item.rendimiento,
                        bancofinal = item.bancofinal?.Trim() ?? string.Empty,
                        cuentafinal = item.cuentafinal?.Trim() ?? string.Empty
                    })
                    .ToArray()
            };

            string json = JsonSerializer.Serialize(contenido);
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
        }

        private sealed class FndLiquidacionPlanProcesoContexto
        {
            public Guid proceso_id { get; init; }
            public int cod_operadora { get; init; }
            public string cod_plan { get; init; } = string.Empty;
            public string documento_referencia { get; init; } = string.Empty;
            public DateTime fecha_proceso { get; init; }
            public string proceso_codigo { get; init; } = string.Empty;
            public string tipo_documento { get; init; } = string.Empty;
            public string tipo_liquidacion { get; init; } = string.Empty;
            public string usuario { get; init; } = string.Empty;
            public string oficina_titular { get; init; } = string.Empty;
            public string oficina_unidad { get; init; } = string.Empty;
            public string oficina_centro_costo { get; init; } = string.Empty;
            public int enlace { get; init; }
            public decimal multa { get; init; }
            public string notas { get; init; } = string.Empty;
            public string retencion_codigo { get; init; } = string.Empty;
            public string cuenta_liquidacion { get; init; } = string.Empty;
            public DateTime? fecha_vence { get; init; }
            public int cod_contabilidad { get; init; }
            public string estado { get; init; } = string.Empty;
            public string solicitud_hash { get; init; } = string.Empty;
            public int total_contratos { get; init; }
        }

        private sealed class FndLiquidacionPlanProcesoDetalle
        {
            public long cod_contrato { get; init; } = 0;
            public decimal aportes { get; init; } = 0;
            public decimal rendimiento { get; init; } = 0;
            public string bancofinal { get; init; } = string.Empty;
            public string cuentafinal { get; init; } = string.Empty;
        }
    }
}
