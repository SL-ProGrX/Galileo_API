using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.TES;
using System.Data;
using System.Diagnostics;
using System.Xml.Linq;

namespace Galileo_API.DataBaseTier.ProGrX.Bancos.frmTES_EmisionDocumentos
{
    public partial class FrmTesEmisionDocumentosDb
    {
        /// <summary>
        /// Inicia una emisión TS persistente o devuelve el proceso activo del banco.
        /// </summary>
        public ErrorDto<TesEmisionDocumentosProcesoResult>
            TES_EmisionDocumentos_Sinpe_Proceso_Iniciar(
                int codEmpresa,
                string usuario,
                TesEmisionDocumentosProcesoIniciarRequest request)
        {
            try
            {
                var solicitudes = request.solicitudes
                    .Where(item => item > 0)
                    .Distinct()
                    .ToArray();
                if (request.banco <= 0 || solicitudes.Length == 0)
                {
                    return DbHelper.CreateErrorResponse<TesEmisionDocumentosProcesoResult>(
                        "Banco y solicitudes son requeridos.");
                }

                var solicitudesXml = new XElement(
                    "Solicitudes",
                    solicitudes.Select(item => new XElement(
                        "Solicitud",
                        new XAttribute("Id", item))))
                    .ToString(SaveOptions.DisableFormatting);

                using var connection = DbHelper.OpenConnection(
                    _portalDB,
                    codEmpresa);
                var resultado =
                    connection.QuerySingle<TesEmisionDocumentosProcesoResult>(
                        "spTES_W_EmisionProceso_Iniciar",
                        new
                        {
                            CodEmpresa = codEmpresa,
                            IdBanco = request.banco,
                            CodPlan = request.plan,
                            Usuario = usuario.Trim().ToUpperInvariant(),
                            SolicitudesXml = solicitudesXml
                        },
                        commandType: CommandType.StoredProcedure);

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "TES_EmisionDocumentos_Sinpe_Proceso_Iniciar: {0}",
                    ex);
                return DbHelper.CreateErrorResponse<TesEmisionDocumentosProcesoResult>(
                    "No fue posible iniciar la emisión TS.");
            }
        }

        /// <summary>
        /// Obtiene el proceso TS activo para una empresa y banco.
        /// </summary>
        public ErrorDto<TesEmisionDocumentosProcesoResult?>
            TES_EmisionDocumentos_Sinpe_Proceso_Activo_Banco_Obtener(
                int codEmpresa,
                int banco)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDB,
                    codEmpresa);
                var resultado =
                    connection.QuerySingleOrDefault<TesEmisionDocumentosProcesoResult>(
                        "spTES_W_EmisionProceso_Activo_Obtener",
                        new { CodEmpresa = codEmpresa, IdBanco = banco },
                        commandType: CommandType.StoredProcedure);
                return DbHelper.CreateOkResponse<TesEmisionDocumentosProcesoResult?>(
                    resultado);
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "TES_EmisionDocumentos_Sinpe_Proceso_Activo_Banco_Obtener: {0}",
                    ex);
                return DbHelper.CreateErrorResponse<TesEmisionDocumentosProcesoResult?>(
                    "No fue posible consultar el proceso activo.");
            }
        }

        /// <summary>
        /// Obtiene el estado persistido de una emisión TS.
        /// </summary>
        public ErrorDto<TesEmisionDocumentosProcesoResult>
            TES_EmisionDocumentos_Sinpe_Proceso_Estado_Obtener(
                int codEmpresa,
                Guid procesoId)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDB,
                    codEmpresa);
                var resultado =
                    connection.QuerySingleOrDefault<TesEmisionDocumentosProcesoResult>(
                        "spTES_W_EmisionProceso_Estado_Obtener",
                        new { CodEmpresa = codEmpresa, ProcesoId = procesoId },
                        commandType: CommandType.StoredProcedure);
                return resultado == null
                    ? DbHelper.CreateErrorResponse<TesEmisionDocumentosProcesoResult>(
                        "No se encontró el proceso TS.")
                    : DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "TES_EmisionDocumentos_Sinpe_Proceso_Estado_Obtener: {0}",
                    ex);
                return DbHelper.CreateErrorResponse<TesEmisionDocumentosProcesoResult>(
                    "No fue posible consultar el proceso TS.");
            }
        }

        /// <summary>
        /// Obtiene los errores persistidos para generar el archivo de resultados.
        /// </summary>
        public ErrorDto<IReadOnlyList<TesEmisionProcesoError>>
            TES_EmisionDocumentos_Sinpe_Proceso_Errores_Obtener(
                int codEmpresa,
                Guid procesoId)
        {
            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDB,
                    codEmpresa);
                var resultado = connection
                    .Query<TesEmisionProcesoError>(
                        "spTES_W_EmisionProceso_Errores_Obtener",
                        new { ProcesoId = procesoId },
                        commandType: CommandType.StoredProcedure)
                    .ToList();
                return DbHelper.CreateOkResponse<IReadOnlyList<TesEmisionProcesoError>>(
                    resultado);
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "TES_EmisionDocumentos_Sinpe_Proceso_Errores_Obtener: {0}",
                    ex);
                return DbHelper.CreateErrorResponse<IReadOnlyList<TesEmisionProcesoError>>(
                    "No fue posible consultar los errores de emisión.");
            }
        }

        /// <summary>
        /// Obtiene el contexto interno utilizado por el trabajador.
        /// </summary>
        public TesEmisionDocumentosProcesoTrabajoContexto?
            TES_EmisionDocumentos_Sinpe_Proceso_Trabajo_Obtener(
                int codEmpresa,
                Guid procesoId)
        {
            using var connection = DbHelper.OpenConnection(
                _portalDB,
                codEmpresa);
            const string sql = @"
select PROCESO_ID as ProcesoId,
       COD_EMPRESA as CodEmpresa,
       ID_BANCO as Banco,
       COD_PLAN as COD_PLAN,
       USUARIO as Usuario,
       TOTAL as Total
from dbo.TES_EMISION_PROCESO
where PROCESO_ID = @procesoId
  and COD_EMPRESA = @codEmpresa
  and ACTIVO = 1;";
            return connection.QuerySingleOrDefault<
                TesEmisionDocumentosProcesoTrabajoContexto>(
                    sql,
                    new { procesoId, codEmpresa });
        }

        /// <summary>
        /// Toma atómicamente el siguiente grupo de solicitudes pendientes.
        /// </summary>
        public IReadOnlyList<int>
            TES_EmisionDocumentos_Sinpe_Proceso_Detalle_Tomar(
                int codEmpresa,
                Guid procesoId,
                int cantidad)
        {
            using var connection = DbHelper.OpenConnection(
                _portalDB,
                codEmpresa);
            return connection.Query<int>(
                "spTES_W_EmisionProceso_Detalle_Tomar",
                new { ProcesoId = procesoId, Cantidad = cantidad },
                commandType: CommandType.StoredProcedure)
                .ToList();
        }

        /// <summary>
        /// Obtiene confirmaciones recuperadas que no deben reenviarse a SINPE.
        /// </summary>
        public IReadOnlyList<int>
            TES_EmisionDocumentos_Sinpe_Proceso_Confirmaciones_Obtener(
                int codEmpresa,
                Guid procesoId)
        {
            using var connection = DbHelper.OpenConnection(
                _portalDB,
                codEmpresa);
            const string sql = @"
select NSOLICITUD
from dbo.TES_EMISION_PROCESO_DET
where PROCESO_ID = @procesoId
  and ESTADO = 'PendienteConfirmacion'
order by NSOLICITUD;";
            return connection.Query<int>(sql, new { procesoId }).ToList();
        }

        /// <summary>
        /// Registra un resultado SINPE y asigna numeración cuando fue exitoso.
        /// </summary>
        public void TES_EmisionDocumentos_Sinpe_Proceso_Resultado_Registrar(
            int codEmpresa,
            Guid procesoId,
            int nSolicitud,
            int codigo,
            string descripcion)
        {
            using var connection = DbHelper.OpenConnection(
                _portalDB,
                codEmpresa);
            connection.Execute(
                "spTES_W_EmisionProceso_Resultado_Registrar",
                new
                {
                    ProcesoId = procesoId,
                    NSolicitud = nSolicitud,
                    Codigo = codigo,
                    Descripcion = descripcion
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        /// <summary>
        /// Reconcilia detalles que quedaron enviándose antes de continuar.
        /// </summary>
        public void TES_EmisionDocumentos_Sinpe_Proceso_Recuperar(
            int codEmpresa,
            Guid procesoId)
        {
            using var connection = DbHelper.OpenConnection(
                _portalDB,
                codEmpresa);
            connection.Execute(
                "spTES_W_EmisionProceso_Recuperar",
                new { ProcesoId = procesoId },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Finaliza el proceso cuando todos sus detalles tienen resultado.
        /// </summary>
        public void TES_EmisionDocumentos_Sinpe_Proceso_Finalizar(
            int codEmpresa,
            Guid procesoId,
            string resultadoContexto)
        {
            using var connection = DbHelper.OpenConnection(
                _portalDB,
                codEmpresa);
            connection.Execute(
                "spTES_W_EmisionProceso_Finalizar",
                new
                {
                    ProcesoId = procesoId,
                    ResultadoContexto = resultadoContexto
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Finaliza un proceso con error y libera el banco.
        /// </summary>
        public void TES_EmisionDocumentos_Sinpe_Proceso_Error_Finalizar(
            int codEmpresa,
            Guid procesoId,
            string mensaje)
        {
            using var connection = DbHelper.OpenConnection(
                _portalDB,
                codEmpresa);
            connection.Execute(
                "spTES_W_EmisionProceso_Error_Finalizar",
                new
                {
                    ProcesoId = procesoId,
                    Mensaje = string.IsNullOrWhiteSpace(mensaje)
                        ? "La emisión TS no pudo completarse."
                        : mensaje
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}
