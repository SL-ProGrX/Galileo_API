using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos.frmTES_EmisionDocumentos
{
    public partial class FrmTesEmisionDocumentosDb
    {
        /// <summary>
        /// Crea un proceso de emisión o recupera el proceso activo equivalente.
        /// </summary>
        public ErrorDto<TesEmisionDocumentosProcesoResult> TES_EmisionDocumentos_Proceso_Iniciar(
            int codEmpresa,
            string propietario,
            TesEmisionDocumentosProcesoIniciarRequest request)
        {
            SqlConnection? connection = null;
            SqlTransaction? transaction = null;

            try
            {
                ValidarInicioProceso(propietario, request);
                var propietarioNormalizado = propietario.Trim().ToUpperInvariant();
                var hash = TesEmisionDocumentosProcesoHash.Crear(
                    codEmpresa,
                    propietarioNormalizado,
                    request.filtros);

                connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
                connection.Open();
                transaction = connection.BeginTransaction();
                AdquirirBloqueoSolicitud(connection, transaction, codEmpresa, propietarioNormalizado, hash);

                var existente = ObtenerProcesoActivo(
                    connection,
                    transaction,
                    codEmpresa,
                    propietarioNormalizado,
                    hash);

                if (existente != null)
                {
                    transaction.Commit();
                    return DbHelper.CreateOkResponse(MapearProceso(existente));
                }

                var procesoId = Guid.NewGuid();
                InsertarProceso(
                    connection,
                    transaction,
                    procesoId,
                    codEmpresa,
                    propietarioNormalizado,
                    hash,
                    request);

                var creado = ObtenerProcesoPorId(connection, transaction, procesoId)
                    ?? throw new InvalidOperationException("No se pudo recuperar el proceso creado.");

                transaction.Commit();
                return DbHelper.CreateOkResponse(MapearProceso(creado));
            }
            catch (Exception ex)
            {
                RollbackSeguro(transaction);
                Trace.TraceError("TES_EmisionDocumentos_Proceso_Iniciar: {0}", ex);
                return DbHelper.CreateErrorResponse<TesEmisionDocumentosProcesoResult>(
                    "No fue posible iniciar la emisión de documentos.");
            }
            finally
            {
                transaction?.Dispose();
                connection?.Dispose();
            }
        }

        /// <summary>
        /// Consulta el estado de un proceso perteneciente al usuario autenticado.
        /// </summary>
        public ErrorDto<TesEmisionDocumentosProcesoResult> TES_EmisionDocumentos_Proceso_Estado_Obtener(
            int codEmpresa,
            Guid procesoId,
            string propietario)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
                var proceso = connection.QuerySingleOrDefault<TesEmisionDocumentosProcesoContexto>(
                    SqlProcesoPorPropietario,
                    new
                    {
                        procesoId,
                        codEmpresa,
                        propietario = propietario.Trim().ToUpperInvariant()
                    });

                return proceso == null
                    ? DbHelper.CreateErrorResponse<TesEmisionDocumentosProcesoResult>(
                        "No se encontró el proceso de emisión solicitado.")
                    : DbHelper.CreateOkResponse(MapearProceso(proceso));
            }
            catch (Exception ex)
            {
                Trace.TraceError("TES_EmisionDocumentos_Proceso_Estado_Obtener: {0}", ex);
                return DbHelper.CreateErrorResponse<TesEmisionDocumentosProcesoResult>(
                    "No fue posible consultar el proceso de emisión.");
            }
        }

        /// <summary>
        /// Adquiere de forma atómica un proceso pendiente para el trabajador.
        /// </summary>
        public TesEmisionDocumentosProcesoContexto? TES_EmisionDocumentos_Proceso_Adquirir(
            int codEmpresa,
            Guid procesoId)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
            const string sql = @"
update dbo.TES_EMISION_DOCUMENTOS_PROCESO
set ESTADO = 'Preparando',
    ETAPA = 'Preparando solicitudes',
    ULTIMA_ACTIVIDAD = SYSDATETIME()
output inserted.PROCESO_ID as proceso_id,
       inserted.COD_EMPRESA as cod_empresa,
       inserted.PROPIETARIO as propietario,
       inserted.SOLICITUD_HASH as solicitud_hash,
       inserted.FILTROS as filtros,
       inserted.ESTADO as estado,
       inserted.ETAPA as etapa,
       inserted.TOTAL as total,
       inserted.PROCESADAS as procesadas,
       inserted.EXITOSAS as exitosas,
       inserted.ERRORES as errores,
       inserted.CONSULTAS_REALIZADAS as consultas_realizadas,
       inserted.MENSAJE as mensaje,
       inserted.FECHA_INICIO as fecha_inicio,
       inserted.ULTIMA_ACTIVIDAD as ultima_actividad
where PROCESO_ID = @procesoId
  and COD_EMPRESA = @codEmpresa
  and ESTADO = 'Pendiente';";

            return connection.QuerySingleOrDefault<TesEmisionDocumentosProcesoContexto>(
                sql,
                new { procesoId, codEmpresa });
        }

        /// <summary>
        /// Cambia una etapa solamente cuando la transición es válida.
        /// </summary>
        public bool TES_EmisionDocumentos_Proceso_Estado_Actualizar(
            int codEmpresa,
            Guid procesoId,
            string estadoActual,
            string estadoSiguiente,
            string etapa)
        {
            if (!TesEmisionDocumentosEstado.PuedeCambiar(estadoActual, estadoSiguiente))
            {
                return false;
            }

            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
            const string sql = @"
update dbo.TES_EMISION_DOCUMENTOS_PROCESO
set ESTADO = @estadoSiguiente,
    ETAPA = @etapa,
    ULTIMA_ACTIVIDAD = SYSDATETIME()
where PROCESO_ID = @procesoId
  and COD_EMPRESA = @codEmpresa
  and ESTADO = @estadoActual;";

            return connection.Execute(sql, new
            {
                procesoId,
                codEmpresa,
                estadoActual,
                estadoSiguiente,
                etapa
            }) == 1;
        }

        /// <summary>
        /// Registra avance real del proceso en ejecución.
        /// </summary>
        public void TES_EmisionDocumentos_Proceso_Avance_Actualizar(
            int codEmpresa,
            TesEmisionDocumentosAvancePersistir avance)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
            const string sql = @"
update dbo.TES_EMISION_DOCUMENTOS_PROCESO
set TOTAL = @Total,
    PROCESADAS = @Procesadas,
    EXITOSAS = @Exitosas,
    ERRORES = @Errores,
    CONSULTAS_REALIZADAS = @ConsultasRealizadas,
    ETAPA = @Etapa,
    ULTIMA_ACTIVIDAD = SYSDATETIME()
where PROCESO_ID = @ProcesoId
  and COD_EMPRESA = @codEmpresa
  and ESTADO in ('Preparando', 'Generando', 'Validando');";

            connection.Execute(sql, new
            {
                codEmpresa,
                avance.ProcesoId,
                avance.Total,
                avance.Procesadas,
                avance.Exitosas,
                avance.Errores,
                avance.ConsultasRealizadas,
                avance.Etapa
            });
        }

        /// <summary>
        /// Mantiene actualizada la actividad de una generación larga sin alterar su avance.
        /// </summary>
        public void TES_EmisionDocumentos_Proceso_Actividad_Actualizar(
            int codEmpresa,
            Guid procesoId)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
            const string sql = @"
update dbo.TES_EMISION_DOCUMENTOS_PROCESO
set ULTIMA_ACTIVIDAD = SYSDATETIME()
where PROCESO_ID = @procesoId
  and COD_EMPRESA = @codEmpresa
  and ESTADO in ('Preparando', 'Generando', 'Validando');";
            connection.Execute(sql, new { procesoId, codEmpresa });
        }

        /// <summary>
        /// Registra un archivo ya publicado y validado.
        /// </summary>
        public void TES_EmisionDocumentos_Proceso_Archivo_Registrar(
            int codEmpresa,
            TesEmisionDocumentosArchivoPersistir archivo)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
            const string sql = @"
insert into dbo.TES_EMISION_DOCUMENTOS_ARCHIVO
    (ARCHIVO_ID, PROCESO_ID, ORDEN, NOMBRE, EXTENSION, CONTENT_TYPE,
     RUTA_INTERNA, TAMANO, SHA256, PAGINAS)
values
    (@ArchivoId, @ProcesoId, @Orden, @Nombre, @Extension, @ContentType,
     @RutaInterna, @Tamano, @Sha256, @Paginas);";

            connection.Execute(sql, archivo);
        }

        /// <summary>
        /// Obtiene el manifiesto de archivos únicamente para un proceso completado del propietario.
        /// </summary>
        public ErrorDto<TesEmisionDocumentosProcesoManifiestoResult> TES_EmisionDocumentos_Proceso_Resultado_Obtener(
            int codEmpresa,
            Guid procesoId,
            string propietario)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
                var proceso = connection.QuerySingleOrDefault<TesEmisionDocumentosProcesoContexto>(
                    SqlProcesoPorPropietario,
                    new
                    {
                        procesoId,
                        codEmpresa,
                        propietario = propietario.Trim().ToUpperInvariant()
                    });

                if (proceso == null || proceso.estado != TesEmisionDocumentosEstado.Completado)
                {
                    return DbHelper.CreateErrorResponse<TesEmisionDocumentosProcesoManifiestoResult>(
                        "El proceso no está completado o no pertenece al usuario.");
                }

                const string sql = @"
select ARCHIVO_ID as archivoId,
       NOMBRE as nombre,
       EXTENSION as extension,
       CONTENT_TYPE as contentType,
       TAMANO as tamano,
       SHA256 as sha256
from dbo.TES_EMISION_DOCUMENTOS_ARCHIVO
where PROCESO_ID = @procesoId
order by ORDEN;";

                var archivos = connection.Query<TesEmisionDocumentosArchivoResult>(
                    sql,
                    new { procesoId }).ToList();
                var contexto = connection.QuerySingleOrDefault<string>(
                    @"select RESULTADO_CONTEXTO
                      from dbo.TES_EMISION_DOCUMENTOS_PROCESO
                      where PROCESO_ID = @procesoId;",
                    new { procesoId }) ?? string.Empty;

                return DbHelper.CreateOkResponse(new TesEmisionDocumentosProcesoManifiestoResult
                {
                    procesoId = procesoId,
                    contexto = contexto,
                    archivos = archivos
                });
            }
            catch (Exception ex)
            {
                Trace.TraceError("TES_EmisionDocumentos_Proceso_Resultado_Obtener: {0}", ex);
                return DbHelper.CreateErrorResponse<TesEmisionDocumentosProcesoManifiestoResult>(
                    "No fue posible obtener los archivos del proceso.");
            }
        }

        /// <summary>
        /// Obtiene la ruta interna de un archivo validando empresa, propietario y proceso completado.
        /// </summary>
        public TesEmisionDocumentosArchivoContexto? TES_EmisionDocumentos_Proceso_Archivo_Obtener(
            int codEmpresa,
            Guid procesoId,
            Guid archivoId,
            string propietario)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
            const string sql = @"
select a.ARCHIVO_ID as archivo_id,
       a.PROCESO_ID as proceso_id,
       a.NOMBRE as nombre,
       a.EXTENSION as extension,
       a.CONTENT_TYPE as content_type,
       a.RUTA_INTERNA as ruta_interna,
       a.TAMANO as tamano,
       a.SHA256 as sha256
from dbo.TES_EMISION_DOCUMENTOS_ARCHIVO a
inner join dbo.TES_EMISION_DOCUMENTOS_PROCESO p
    on p.PROCESO_ID = a.PROCESO_ID
where a.ARCHIVO_ID = @archivoId
  and a.PROCESO_ID = @procesoId
  and p.COD_EMPRESA = @codEmpresa
  and p.PROPIETARIO = @propietario
  and p.ESTADO = 'Completado';";

            return connection.QuerySingleOrDefault<TesEmisionDocumentosArchivoContexto>(
                sql,
                new
                {
                    archivoId,
                    procesoId,
                    codEmpresa,
                    propietario = propietario.Trim().ToUpperInvariant()
                });
        }

        /// <summary>
        /// Marca el proceso completado después de publicar todos los archivos.
        /// </summary>
        public void TES_EmisionDocumentos_Proceso_Finalizar(
            int codEmpresa,
            Guid procesoId,
            string resultadoContexto)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
            const string sql = @"
update dbo.TES_EMISION_DOCUMENTOS_PROCESO
set ESTADO = 'Completado',
    ETAPA = 'Archivos disponibles',
    RESULTADO_CONTEXTO = @resultadoContexto,
    PROCESADAS = TOTAL,
    EXITOSAS = TOTAL - ERRORES,
    ULTIMA_ACTIVIDAD = SYSDATETIME()
where PROCESO_ID = @procesoId
  and COD_EMPRESA = @codEmpresa
  and ESTADO = 'Validando';";
            connection.Execute(sql, new { procesoId, codEmpresa, resultadoContexto });
        }

        /// <summary>
        /// Registra un error terminal sin exponer detalles técnicos.
        /// </summary>
        public void TES_EmisionDocumentos_Proceso_Error_Registrar(
            int codEmpresa,
            Guid procesoId,
            string mensaje)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
            const string sql = @"
update dbo.TES_EMISION_DOCUMENTOS_PROCESO
set ESTADO = 'Error',
    ETAPA = 'Proceso detenido',
    ERRORES = ERRORES + 1,
    MENSAJE = @mensaje,
    ULTIMA_ACTIVIDAD = SYSDATETIME()
where PROCESO_ID = @procesoId
  and COD_EMPRESA = @codEmpresa
  and ESTADO in ('Pendiente', 'Preparando', 'Generando', 'Validando');";
            connection.Execute(sql, new
            {
                procesoId,
                codEmpresa,
                mensaje = LimitarMensaje(mensaje)
            });
        }

        private const string SqlProcesoPorPropietario = @"
select PROCESO_ID as proceso_id,
       COD_EMPRESA as cod_empresa,
       PROPIETARIO as propietario,
       SOLICITUD_HASH as solicitud_hash,
       FILTROS as filtros,
       ESTADO as estado,
       ETAPA as etapa,
       TOTAL as total,
       PROCESADAS as procesadas,
       EXITOSAS as exitosas,
       ERRORES as errores,
       CONSULTAS_REALIZADAS as consultas_realizadas,
       MENSAJE as mensaje,
       FECHA_INICIO as fecha_inicio,
       ULTIMA_ACTIVIDAD as ultima_actividad
from dbo.TES_EMISION_DOCUMENTOS_PROCESO
where PROCESO_ID = @procesoId
  and COD_EMPRESA = @codEmpresa
  and PROPIETARIO = @propietario;";

        private static void ValidarInicioProceso(
            string propietario,
            TesEmisionDocumentosProcesoIniciarRequest request)
        {
            if (string.IsNullOrWhiteSpace(propietario))
                throw new InvalidOperationException("El propietario autenticado es requerido.");
            if (request?.filtros == null)
                throw new InvalidOperationException("Los filtros de emisión son requeridos.");
            if (request.filtros.cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor que cero.");
        }

        private static void AdquirirBloqueoSolicitud(
            SqlConnection connection,
            SqlTransaction transaction,
            int codEmpresa,
            string propietario,
            string hash)
        {
            const string sql = @"
declare @resultado int;
exec @resultado = sys.sp_getapplock
    @Resource = @recurso,
    @LockMode = 'Exclusive',
    @LockOwner = 'Transaction',
    @LockTimeout = 10000;
select @resultado;";
            var recurso = $"TES_EMISION:{codEmpresa}:{propietario}:{hash}";
            var resultado = connection.ExecuteScalar<int>(sql, new { recurso }, transaction);
            if (resultado < 0)
                throw new InvalidOperationException("No se pudo bloquear la solicitud de emisión.");
        }

        private static TesEmisionDocumentosProcesoContexto? ObtenerProcesoActivo(
            SqlConnection connection,
            SqlTransaction transaction,
            int codEmpresa,
            string propietario,
            string hash)
        {
            const string sql = @"
select top (1)
       PROCESO_ID as proceso_id,
       COD_EMPRESA as cod_empresa,
       PROPIETARIO as propietario,
       SOLICITUD_HASH as solicitud_hash,
       FILTROS as filtros,
       ESTADO as estado,
       ETAPA as etapa,
       TOTAL as total,
       PROCESADAS as procesadas,
       EXITOSAS as exitosas,
       ERRORES as errores,
       CONSULTAS_REALIZADAS as consultas_realizadas,
       MENSAJE as mensaje,
       FECHA_INICIO as fecha_inicio,
       ULTIMA_ACTIVIDAD as ultima_actividad
from dbo.TES_EMISION_DOCUMENTOS_PROCESO with (updlock, holdlock)
where COD_EMPRESA = @codEmpresa
  and PROPIETARIO = @propietario
  and SOLICITUD_HASH = @hash
  and ESTADO in ('Pendiente', 'Preparando', 'Generando', 'Validando')
order by FECHA_INICIO desc;";
            return connection.QuerySingleOrDefault<TesEmisionDocumentosProcesoContexto>(
                sql,
                new { codEmpresa, propietario, hash },
                transaction);
        }

        private static void InsertarProceso(
            SqlConnection connection,
            SqlTransaction transaction,
            Guid procesoId,
            int codEmpresa,
            string propietario,
            string hash,
            TesEmisionDocumentosProcesoIniciarRequest request)
        {
            const string sql = @"
insert into dbo.TES_EMISION_DOCUMENTOS_PROCESO
    (PROCESO_ID, COD_EMPRESA, PROPIETARIO, SOLICITUD_HASH, FILTROS,
     ESTADO, ETAPA, TOTAL)
values
    (@procesoId, @codEmpresa, @propietario, @hash, @filtros,
     'Pendiente', 'En espera', @total);";
            connection.Execute(sql, new
            {
                procesoId,
                codEmpresa,
                propietario,
                hash,
                filtros = JsonConvert.SerializeObject(request.filtros),
                total = request.filtros.cantidad
            }, transaction);
        }

        private static TesEmisionDocumentosProcesoContexto? ObtenerProcesoPorId(
            SqlConnection connection,
            SqlTransaction transaction,
            Guid procesoId)
        {
            const string sql = @"
select PROCESO_ID as proceso_id,
       COD_EMPRESA as cod_empresa,
       PROPIETARIO as propietario,
       SOLICITUD_HASH as solicitud_hash,
       FILTROS as filtros,
       ESTADO as estado,
       ETAPA as etapa,
       TOTAL as total,
       PROCESADAS as procesadas,
       EXITOSAS as exitosas,
       ERRORES as errores,
       CONSULTAS_REALIZADAS as consultas_realizadas,
       MENSAJE as mensaje,
       FECHA_INICIO as fecha_inicio,
       ULTIMA_ACTIVIDAD as ultima_actividad
from dbo.TES_EMISION_DOCUMENTOS_PROCESO
where PROCESO_ID = @procesoId;";
            return connection.QuerySingleOrDefault<TesEmisionDocumentosProcesoContexto>(
                sql,
                new { procesoId },
                transaction);
        }

        private static TesEmisionDocumentosProcesoResult MapearProceso(
            TesEmisionDocumentosProcesoContexto proceso)
        {
            var porcentaje = proceso.total == 0
                ? 0
                : Math.Round(proceso.procesadas * 100m / proceso.total, 2);
            return new TesEmisionDocumentosProcesoResult
            {
                procesoId = proceso.proceso_id,
                estado = proceso.estado,
                etapa = proceso.etapa,
                total = proceso.total,
                procesadas = proceso.procesadas,
                exitosas = proceso.exitosas,
                errores = proceso.errores,
                consultasRealizadas = proceso.consultas_realizadas,
                porcentaje = porcentaje,
                fechaInicio = proceso.fecha_inicio,
                ultimaActividad = proceso.ultima_actividad,
                mensaje = proceso.mensaje ?? string.Empty
            };
        }

        private static string LimitarMensaje(string mensaje)
        {
            const int maximo = 1000;
            var limpio = string.IsNullOrWhiteSpace(mensaje)
                ? "La emisión no pudo completarse."
                : mensaje.Trim();
            return limpio.Length <= maximo ? limpio : limpio[..maximo];
        }

        private static void RollbackSeguro(SqlTransaction? transaction)
        {
            try
            {
                transaction?.Rollback();
            }
            catch (InvalidOperationException)
            {
                // La transacción ya finalizó.
            }
            catch (SqlException)
            {
                // La conexión se perdió y no es posible revertir explícitamente.
            }
        }
    }
}
