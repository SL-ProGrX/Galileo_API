using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using System.Data;
using static Galileo_API.Models.ProGrX.Creditos.FrmCRCambioInfoEstadisticaModels;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCRCambioInfoEstadisticaDB
    {
        private readonly PortalDB _portalDb; 
        private const string TipoProcesoCredito = "C";
        private readonly MSecurityMainDb _Security_MainDB;
        private const int ModuloBitacora = 3;
        private const string TipoDeductora = "Deductora";
        private const string TipoDestino = "Destino o Plan de Inversión";
        private const string TipoRecursoPresupuestario = "Recurso Presupuestario";
        public FrmCRCambioInfoEstadisticaDB(IConfiguration config)
        { 
            _portalDb = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }


        /// <summary>
        /// Consulta el distado de datos segun el tipo seleccionado
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_CambioInfoEstadistica_DatosTipo_Obtener(int codEmpresa, string tipo)
        {
            var query = ObtenerConsultaPorTipo(tipo);

            if (string.IsNullOrWhiteSpace(query))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo seleccionado no es válido.",
                    -1,
                    new List<DropDownListaGenericaModel>());
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    connection
                        .Query<DropDownListaGenericaModel>(query)
                        .ToList());
        }
        private static string ObtenerConsultaPorTipo(string tipo)
        {
            return tipo switch
            {
                TipoDeductora => Consultas.Deductoras,
                TipoDestino => Consultas.Destinos,
                TipoRecursoPresupuestario =>
                    Consultas.RecursosPresupuestarios,
                _ => string.Empty
            };
        }

        /// <summary>
        /// Contiene las consultas SQL para obtener los datos según el tipo seleccionado.
        /// </summary>
        private static class Consultas
        {
            internal const string Deductoras = """
                SELECT
                    COD_INSTITUCION AS Item,
                    RTRIM(DESCRIPCION)
                        + SPACE(10)
                        + '['
                        + RTRIM(ISNULL(DESC_CORTA, ''))
                        + ']' AS Descripcion
                FROM INSTITUCIONES
                WHERE ACTIVA = 1
                  AND DEDUCCION_PLANILLA = 1
                ORDER BY RTRIM(DESC_CORTA);
                """;

            internal const string Destinos = """
                SELECT
                    RTRIM(COD_DESTINO) AS Item,
                    DESCRIPCION
                        + SPACE(10)
                        + '['
                        + RTRIM(COD_DESTINO)
                        + ']' AS Descripcion
                FROM CATALOGO_DESTINOS
                ORDER BY COD_DESTINO;
                """;

            internal const string RecursosPresupuestarios = """
                SELECT
                    RTRIM(COD_GRUPO) AS Item,
                    DESCRIPCION
                        + SPACE(10)
                        + '['
                        + RTRIM(COD_GRUPO)
                        + ']' AS Descripcion
                FROM CATALOGO_GRUPOS
                ORDER BY COD_GRUPO;
                """;
        }

        /// <summary>
        /// Procesa el cambio masivo de crédito según el listado cargado previamente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<bool> CR_CambioInfoEstadistica_Procesar(int codEmpresa, CrCambioInfoEstadisticaProcesarRequest request)
        {
            using var connection =
                DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                connection.Execute(
                    "spSys_Carga_Masiva_Credito_Procesa",
                    new
                    {
                        Tipo = TipoProcesoCredito,
                        request.ProcessId,
                        request.Usuario,
                        Codigo = request.CodigoDato
                    },
                    commandType: CommandType.StoredProcedure);

                RegistrarBitacora(
                   codEmpresa,
                    request.Usuario,
                   $"Cambio Masivo de {request.TipoDescripcion}, Listado de Excel: Líneas({request.CantidadLineas}) ",
                   "Aplica- WEB");

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse(
                    "Error al procesar el cambio masivo de crédito.",
                    -1,
                    false);
            }
        }

        /// <summary>
        /// Registra un movimiento en la bitácora del sistema.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="detalle"></param>
        /// <param name="movimiento"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario ?? string.Empty,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloBitacora
            });
        }

        private static ErrorDto<CrCambioInfoEstadisticaCargaListadoResponse> ValidarSolicitud(CrCambioInfoEstadisticaCargaListadoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TipoSeleccionado) ||
                string.IsNullOrWhiteSpace(request.CodigoDato) ||
                string.IsNullOrWhiteSpace(request.Usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo seleccionado, el código y el usuario son requeridos.",
                    -1,
                    new CrCambioInfoEstadisticaCargaListadoResponse());
            }

            if (request.Registros.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                     "Debe enviar al menos una operación para procesar.",
                    -1,
                    new CrCambioInfoEstadisticaCargaListadoResponse());
            }

            return null;
        }

        /// <summary>
        /// Obtiene las operaciones válidas de la lista de registros, eliminando duplicados y valores nulos o vacíos.
        /// </summary>
        /// <param name="registros"></param>
        /// <returns></returns>
        private static List<string> ObtenerOperacionesValidas(IEnumerable<CrCambioInfoEstadisticaCargaExcelData> registros)
        {
            return registros
                .Select(registro => registro.Operacion.Trim())
                .Where(operacion =>
                    !string.IsNullOrWhiteSpace(operacion))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Crea un identificador de proceso único basado en el tipo seleccionado y el código del dato.
        /// </summary>
        /// <param name="tipoSeleccionado"></param>
        /// <param name="codigoDato"></param>
        /// <returns></returns>
        private static string CrearProcessId(string tipoSeleccionado, string codigoDato)
        {
            var prefijo = tipoSeleccionado
                .Trim()
                    [
                    ..Math.Min(
                        4,
                        tipoSeleccionado.Trim().Length)];

            var processId = string.Concat(
                prefijo,
                ".",
                codigoDato.Trim());

            return processId[
                ..Math.Min(10, processId.Length)];
        }

        private const int TamanoLote = 500;

        /// <summary>
        /// Carga las operaciones en la tabla temporal para el proceso de cambio masivo de crédito
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="request"></param>
        /// <param name="processId"></param>
        private static void CargarOperaciones(IDbConnection connection, IDbTransaction transaction, CrCambioInfoEstadisticaCargaListadoRequest request, string processId)
        {
            const string procedure = "spSys_Carga_Masiva";

            var operaciones = ObtenerOperacionesValidas(request.Registros);

            var parametros = operaciones
                .Select((operacion, index) => new
                {
                    Tipo = TipoProcesoCredito,
                    ProcesoId = processId,
                    request.Usuario,
                    Llave01 = operacion,
                    Llave02 = string.Empty,
                    Clean = index == 0 ? 1 : 0
                })
                .ToList();

            foreach (var lote in parametros.Chunk(TamanoLote))
            {
                connection.Execute(
                    procedure,
                    lote,
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);
            }

        }

        /// <summary>
        /// Obtiene los registros vinculados al proceso de carga masiva de crédito
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="usuario"></param>
        /// <param name="processId"></param>
        /// <returns></returns>
        private static List<CrCambioInfoEstadisticaCargaResultadoData> ObtenerRegistrosVinculados(
        IDbConnection connection,
        IDbTransaction transaction,
        string usuario,
        string processId)
        {
            const string procedure =
                "spSys_Carga_Masiva_Credito_Ref";

            return connection.Query<CrCambioInfoEstadisticaCargaResultadoData>(
                    procedure,
                    new
                    {
                        Tipo = TipoProcesoCredito,
                        ProcesoId = processId,
                        Usuario = usuario
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure)
                .AsList();
        }

        /// <summary>
        /// Carga informacion del excel para ser procesada en el cambio masivo de credito
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrCambioInfoEstadisticaCargaListadoResponse> CR_CambioInfoEstadistica_CargarListado(int codEmpresa, CrCambioInfoEstadisticaCargaListadoRequest request)
        {
            var validacion = ValidarSolicitud(request);

            if (validacion is not null)
            {
                return validacion;
            }

            using var connection =
                DbHelper.OpenConnection(_portalDb, codEmpresa);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var processId = CrearProcessId(
                    request.TipoSeleccionado,
                    request.CodigoDato);

                CargarOperaciones(
                    connection,
                    transaction,
                    request,
                    processId);

                var registros = ObtenerRegistrosVinculados(
                    connection,
                    transaction,
                    request.Usuario,
                    processId);

                transaction.Commit();

                var response = new CrCambioInfoEstadisticaCargaListadoResponse
                {
                    ProcessId = processId,
                    CantidadRegistros = registros.Count,
                    Registros = registros
                };

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse(
                    "Error al cargar el listado de operaciones.",
                    -1,
                    new CrCambioInfoEstadisticaCargaListadoResponse());
            }
        }
    }
}
