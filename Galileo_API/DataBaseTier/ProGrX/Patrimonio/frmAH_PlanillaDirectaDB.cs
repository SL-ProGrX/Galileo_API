using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAhPlanillaDirectaDB
    {

        private readonly PortalDB _portalDb;

        public FrmAhPlanillaDirectaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las instituciones activas para el proceso de planilla directa.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Ah_PlanillaDirecta_Instituciones_Obtener(
            int codEmpresa)
        {
            const string sql = @"
                select
                    cast(COD_INSTITUCION as varchar(20)) as item,
                    '[' + rtrim(isnull(COD_DIVISA, '')) + ']  ' + rtrim(isnull(DESCRIPCION, '')) as descripcion
                from INSTITUCIONES
                where ACTIVA = 1
                order by COD_INSTITUCION;";

            var result = DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);

            if (result.Code == 0 && result.Result != null)
            {
                result.Result.Insert(0, new DropDownListaGenericaModel
                {
                    item = "0",
                    descripcion = "TODOS"
                });
            }

            return result;
        }

        /// <summary>
        /// Obtiene los periodos de proceso disponibles para planilla directa.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Ah_PlanillaDirecta_Periodos_Obtener(
            int codEmpresa)
        {
            const string sql = @"
;WITH Periodos AS (
    SELECT dbo.fxSIFPrmProcesoAnt(
               dbo.fxSIFPrmProcesoAnt(YEAR(dbo.MyGetdate()) * 100 + MONTH(dbo.MyGetdate()))
           ) AS item,
           0 AS Orden
    UNION ALL
    SELECT dbo.fxSIFPrmProcesoSig(item), Orden + 1
    FROM Periodos
    WHERE Orden < 12
)
SELECT CAST(item AS varchar(20)) AS item,
       CAST(item AS varchar(20)) AS descripcion
FROM Periodos
ORDER BY item;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene el número de comprobante sugerido para planilla directa.
        /// </summary>
        public ErrorDto<string> Ah_PlanillaDirecta_Comprobante_Obtener(
            int codEmpresa,
            int codInstitucion,
            int proceso,
            string tipoAporte)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            const string sql = @"
SELECT dbo.fxPat_Planillas_Comprobante(
    @Institucion,
    @Proceso,
    @Plan
) AS Result;";

            var comprobante = conn.QueryFirstOrDefault<string>(
                sql,
                new
                {
                    Institucion = codInstitucion,
                    Proceso = proceso,
                    Plan = Ah_PlanillaDirecta_NormalizarTipoAporteComprobante(tipoAporte)
                });

            return DbHelper.CreateOkResponse(comprobante ?? string.Empty);
        }

        /// <summary>
        /// Carga el lote de planilla directa y devuelve los registros consultados.
        /// </summary>
        public ErrorDto<List<FrmAhPlanillaDirectaCargadoDto>> Ah_PlanillaDirecta_Cargado(
            int codEmpresa,
            FrmAhPlanillaDirectaCargadoRequest? request)
        {
            var response = new List<FrmAhPlanillaDirectaCargadoDto>();
            var validacion = Ah_PlanillaDirecta_ValidarCargadoRequest(request, response);

            if (validacion != null)
            {
                return validacion;
            }

            const string sqlSube = @"
exec spPAT_PlanillaDirecta_Sube
    @Institucion,
    @Documento,
    @Proceso,
    @Tipo,
    @Cedula,
    @Monto,
    @Usuario,
    @Linea,
    @Inicializa,
    @Nombre;";

            const string sqlConsulta = @"
exec spPAT_PlanillaDirecta_Consulta
    @Institucion,
    @Documento,
    @Proceso,
    @Tipo,
    @Usuario,
    1,
    @Nombre;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var transaction = conn.BeginTransaction();

                try
                {
                    var tipoAporte = Ah_PlanillaDirecta_NormalizarTipoAporteProceso(request!.tipo_aporte);

                    for (var i = 0; i < request.registros.Count; i++)
                    {
                        var fila = request.registros[i];

                        conn.Execute(
                            sqlSube,
                            new
                            {
                                Institucion = request.cod_institucion,
                                Documento = request.num_doc.Trim(),
                                Proceso = request.proceso,
                                Tipo = tipoAporte,
                                Cedula = fila.llave_01.Trim(),
                                Monto = fila.monto_01,
                                Usuario = request.registro_usuario.Trim(),
                                Linea = i + 1,
                                Inicializa = i == 0 ? 1 : 0,
                                Nombre = fila.ref_01.Trim()
                            },
                            transaction: transaction,
                            commandType: CommandType.Text);
                    }

                    response = conn.Query<FrmAhPlanillaDirectaCargadoDto>(
                        sqlConsulta,
                        new
                        {
                            Institucion = request.cod_institucion,
                            Documento = request.num_doc.Trim(),
                            Proceso = request.proceso,
                            Tipo = tipoAporte,
                            Usuario = request.registro_usuario.Trim(),
                            Nombre = request.archivo.Trim()
                        },
                        transaction: transaction,
                        commandType: CommandType.Text).ToList();

                    transaction.Commit();
                    return DbHelper.CreateOkResponse(response);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Obtiene las inconsistencias del comprobante cargado.
        /// </summary>
        public ErrorDto<List<FrmAhPlanillaDirectaInconsistenciaDto>> Ah_PlanillaDirecta_Inconsistencias_Obtener(
            int codEmpresa,
            string numDoc)
        {
            var response = new List<FrmAhPlanillaDirectaInconsistenciaDto>();

            if (string.IsNullOrWhiteSpace(numDoc))
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar el número de comprobante.",
                    -1,
                    response);
            }

            const string sql = @"
exec spPAT_PlanillaDirecta_Inconsistencias
    @Documento;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                response = conn.Query<FrmAhPlanillaDirectaInconsistenciaDto>(
                    sql,
                    new { Documento = numDoc.Trim() },
                    commandType: CommandType.Text).ToList();

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        /// <summary>
        /// Procesa la planilla directa cargada ejecutando el SP final del proceso.
        /// </summary>
        public ErrorDto<FrmAhPlanillaDirectaProcesarResponse> Ah_PlanillaDirecta_Procesar(
            int codEmpresa,
            FrmAhPlanillaDirectaProcesarRequest? request)
        {
            var response = new FrmAhPlanillaDirectaProcesarResponse();
            var validacion = Ah_PlanillaDirecta_ValidarProcesarRequest(request, response);

            if (validacion != null)
            {
                return validacion;
            }

            const string sqlConsecutivo = @"
update SIF_DOCUMENTOS
set CONSECUTIVO = isnull(CONSECUTIVO, 0) + 1
where TIPO_DOCUMENTO = 'PLA';";

            const string sqlProcesar = @"
exec spPAT_PlanillaDirecta_Procesa
    @Institucion,
    @Proceso,
    @Tipo,
    @Documento,
    @Usuario;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                conn.Open();

                using var transaction = conn.BeginTransaction();

                try
                {
                    conn.Execute(
                        sqlConsecutivo,
                        transaction: transaction,
                        commandType: CommandType.Text);

                    conn.Execute(
                        sqlProcesar,
                        new
                        {
                            Institucion = request!.cod_institucion,
                            Proceso = request.proceso,
                            Tipo = Ah_PlanillaDirecta_NormalizarTipoAporteProceso(request.tipo_aporte),
                            Documento = request.num_doc.Trim(),
                            Usuario = request.registro_usuario.Trim()
                        },
                        transaction: transaction,
                        commandType: CommandType.Text);

                    transaction.Commit();

                    response.accion = "PROCESAR";
                    response.mensaje = "Información actualizada satisfactoriamente.";
                    response.num_doc = request.num_doc.Trim();

                    return DbHelper.CreateOkResponse(response);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        private static ErrorDto<List<FrmAhPlanillaDirectaCargadoDto>>? Ah_PlanillaDirecta_ValidarCargadoRequest(
    FrmAhPlanillaDirectaCargadoRequest? request,
    List<FrmAhPlanillaDirectaCargadoDto> response)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse("La solicitud es requerida.", -1, response);
            }

            var mensajeBase = Ah_PlanillaDirecta_ValidarRequestBase(
                request.cod_institucion,
                request.proceso,
                request.num_doc,
                request.registro_usuario);

            if (!string.IsNullOrWhiteSpace(mensajeBase))
            {
                return DbHelper.CreateErrorResponse(mensajeBase, -1, response);
            }

            if (request.registros == null || request.registros.Count == 0)
            {
                return DbHelper.CreateErrorResponse("No existen registros para cargar.", -1, response);
            }

            for (var i = 0; i < request.registros.Count; i++)
            {
                var fila = request.registros[i];

                if (string.IsNullOrWhiteSpace(fila.llave_01))
                {
                    return DbHelper.CreateErrorResponse($"La cédula es requerida en la fila {i + 1}.", -1, response);
                }

                if (string.IsNullOrWhiteSpace(fila.ref_01))
                {
                    return DbHelper.CreateErrorResponse($"El nombre es requerido en la fila {i + 1}.", -1, response);
                }

                if (fila.monto_01 <= 0)
                {
                    return DbHelper.CreateErrorResponse($"El monto debe ser mayor a cero en la fila {i + 1}.", -1, response);
                }
            }

            return null;
        }

        private static string? Ah_PlanillaDirecta_ValidarRequestBase(
            int codInstitucion,
            int proceso,
            string? numDoc,
            string? registroUsuario)
        {
            if (codInstitucion <= 0)
            {
                return "La institución es requerida.";
            }

            if (proceso <= 0)
            {
                return "El período es requerido.";
            }

            if (string.IsNullOrWhiteSpace(numDoc))
            {
                return "El comprobante es requerido.";
            }

            if (string.IsNullOrWhiteSpace(registroUsuario))
            {
                return "El usuario es requerido.";
            }

            return null;
        }

        private static ErrorDto<FrmAhPlanillaDirectaProcesarResponse>? Ah_PlanillaDirecta_ValidarProcesarRequest(
    FrmAhPlanillaDirectaProcesarRequest? request,
    FrmAhPlanillaDirectaProcesarResponse response)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse("La solicitud es requerida.", -1, response);
            }

            var mensajeBase = Ah_PlanillaDirecta_ValidarRequestBase(
                request.cod_institucion,
                request.proceso,
                request.num_doc,
                request.registro_usuario);

            if (!string.IsNullOrWhiteSpace(mensajeBase))
            {
                return DbHelper.CreateErrorResponse(mensajeBase, -1, response);
            }

            return null;
        }

        private static string Ah_PlanillaDirecta_NormalizarTipoAporteComprobante(string? tipoAporte)
        {
            var tipo = (tipoAporte ?? string.Empty).Trim().ToUpperInvariant();

            return tipo switch
            {
                "OBR" or "OBRERO" or "O" => "OBR",
                "PAT" or "PATRONAL" or "P" => "PAT",
                "CAP" or "CAPITALIZACION" or "CAPITALIZACIÓN" or "C" => "CAP",
                _ => "OBR"
            };
        }

        private static string Ah_PlanillaDirecta_NormalizarTipoAporteProceso(string? tipoAporte)
        {
            var tipo = (tipoAporte ?? string.Empty).Trim().ToUpperInvariant();

            return tipo switch
            {
                "OBR" or "OBRERO" or "O" => "O",
                "PAT" or "PATRONAL" or "P" => "P",
                "CAP" or "CAPITALIZACION" or "CAPITALIZACIÓN" or "C" => "C",
                _ => "O"
            };
        }

    }
}
