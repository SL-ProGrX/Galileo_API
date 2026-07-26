
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using System.Globalization;
using static Galileo_API.Models.ProGrX.Creditos.FrmCrAbonoMasivoManualModels;
using static Galileo_API.Models.ProGrX.Creditos.FrmCRCambioInfoEstadisticaModels;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrAbonoMasivoManualDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrAbonoMasivoManualDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la lista de operadoras para el abono masivo manual.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_AbonoMasivo_Manual_Operadoras_Obtener(int codEmpresa)
        {
            var query = """
                SELECT
                 cod_operadora AS Item,
                 descripcion AS Descripcion
                from FND_Operadoras
                """;

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    connection
                        .Query<DropDownListaGenericaModel>(query)
                        .ToList());
        }

        /// <summary>
        ///  Obtiene la lista de planes para el abono masivo manual según la operadora seleccionada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operadora"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_AbonoMasivo_Manual_Planes_Obtener(int codEmpresa, string operadora)
        {
            var query = """
                SELECT
                    rtrim(cod_plan) AS Item,
                   rtrim(descripcion) + space(10) + '[' + rtrim(cod_Plan) + ']' AS Descripcion
                from fnd_planes where deduce_independiente = 1 and cod_operadora =@operadora
                """;

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    connection
                        .Query<DropDownListaGenericaModel>(query, new { operadora })
                        .ToList());
        }

        /// <summary>
        /// Procesa la carga de deducciones para el abono masivo manual.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrAplicacionAbonoMasivoResponse> CR_AbonoMasivo_Manual_CargaDeducciones_Procesar(int codEmpresa, CrAplicacionAbonoMasivoRequest request)
        {

            using var connection =
                DbHelper.OpenConnection(_portalDb, codEmpresa);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var resultado = CargarAplicacionesAbono(
                    connection,
                    transaction,
                    request);

                transaction.Commit();


                return DbHelper.CreateOkResponse(resultado);
            }
            catch
            {
                transaction.Rollback();
                return DbHelper.CreateErrorResponse(
                           "Error al cargar el listado de operaciones.",
                           -1,
                           new CrAplicacionAbonoMasivoResponse());
            }
        }

        /// <summary>
        /// Carga las aplicaciones de abono masivo.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        private static CrAplicacionAbonoMasivoResponse CargarAplicacionesAbono(IDbConnection connection, IDbTransaction transaction, CrAplicacionAbonoMasivoRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.Registros.Count == 0)
            {
                return new CrAplicacionAbonoMasivoResponse();
            }

            CargarRegistros(
                connection,
                transaction,
                request);

            var registros = RevisarRegistros(
                connection,
                transaction,
                request.Usuario);

            return new CrAplicacionAbonoMasivoResponse
            {
                CantidadCasos = registros.Count,
                MontoTotal = registros.Sum(x => x.Abono),
                Registros = registros
            };
        }

        private const int TamanoBloque = 500;

        /// <summary>
        /// Carga los registros de abono masivo en bloques.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="request"></param>
        private static void CargarRegistros(IDbConnection connection, IDbTransaction transaction, CrAplicacionAbonoMasivoRequest request)
        {
            var registros = request.Registros
                .Where(x => x.Operacion > 0)
                .ToList();

            for (var inicio = 0; inicio < registros.Count; inicio += TamanoBloque)
            {
                var bloque = registros
                    .Skip(inicio)
                    .Take(TamanoBloque)
                    .Select((registro, indice) => CrearParametrosCarga(
                        registro,
                        request.Usuario,
                        limpiar: inicio == 0 && indice == 0))
                    .ToList();

                connection.Execute(
                    sql: SqlCargaMasiva,
                    param: bloque,
                    transaction: transaction,
                    commandType: CommandType.Text);
            }
        }

        /// <summary>
        /// Consulta SQL para la carga masiva de registros.
        /// </summary>
        private const string SqlCargaMasiva = """
    EXEC spSys_Carga_Masiva_Sube
         @Tipo,
         @Proceso,
         @Llave01,
         @Llave02,
         @Usuario,
         @Llave03,
         @Llave04,
         @Llave05,
         @Monto01,
         @Monto02,
         @Monto03,
         @Texto01,
         @Fecha01,
         @Fecha02,
         @Fecha03,
         @Clean;
    """;

        /// <summary>
        ///     Crea los parámetros para la carga masiva de registros.
        /// </summary>
        /// <param name="registro"></param>
        /// <param name="usuario"></param>
        /// <param name="limpiar"></param>
        /// <returns></returns>
        private static object CrearParametrosCarga(CrAplicacionAbonoMasivoRegistroRequest registro, string usuario, bool limpiar)
        {
            return new
            {
                Tipo = "C",
                Proceso = "CrdAplAbo",
                Llave01 = registro.Operacion.ToString(
                    CultureInfo.InvariantCulture),

                Llave02 = string.Empty,
                Usuario = usuario,
                Llave03 = string.Empty,
                Llave04 = string.Empty,
                Llave05 = string.Empty,

                Monto01 = registro.Abono,
                Monto02 = 0m,
                Monto03 = 0m,

                Texto01 = string.Empty,

                Fecha01 = (DateTime?)null,
                Fecha02 = (DateTime?)null,
                Fecha03 = (DateTime?)null,

                Clean = limpiar ? 1 : 0
            };
        }

        /// <summary>
        /// Revisa los registros cargados para el abono masivo.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static List<CrAplicacionAbonoMasivoDetalle> RevisarRegistros(IDbConnection connection, IDbTransaction transaction, string usuario)
        {
            const string sql = """
        EXEC spSys_Carga_Masiva_Revisa_AplMasCrd_Abono
             @Tipo,
             @Proceso,
             @Llave01,
             @Llave02,
             @Usuario;
        """;

            return [.. connection.Query<CrAplicacionAbonoMasivoDetalle>(
                    sql,
                    new
                    {
                        Tipo = "C",
                        Proceso = "CrdAplAbo",
                        Llave01 = string.Empty,
                        Llave02 = string.Empty,
                        Usuario = usuario
                    },
                    transaction,
                    commandType: CommandType.Text)];
        }

        /// <summary>
        /// Aplica los abonos masivos procesados.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="transaction"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static CrAplicacionAbonoMasivoProcesarResponse AplicarAbonosMasivos(IDbConnection connection, IDbTransaction transaction, CrAplicacionAbonoMasivoProcesarRequest request)
        {
            const string procedure =
                "spSys_Carga_Masiva_Aplica_AplMasCrd_Abono";

            var resultado =
                connection.QuerySingleOrDefault<CrAplicacionAbonoMasivoProcesarResponse>(
                    procedure,
                    new
                    {
                        Tipo = "C",
                        ProcesoId = "CrdAplAbo",
                        Llave_01 = string.Empty,
                        Llave_02 = string.Empty,
                        request.Usuario,
                        request.Operadora,
                        request.Plan,
                        request.Cuenta,
                        FondoComun = request.FondoGeneral ? 1 : 0,
                        TipoApl = request.Tipo
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure);

            return resultado
                ?? throw new InvalidOperationException(
                    "El procedimiento no devolvió el documento generado.");
        }
        /// <summary>
        /// Procesa los abonos masivos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrAplicacionAbonoMasivoProcesarResponse> ProcesarAbonosMasivos(int codEmpresa, CrAplicacionAbonoMasivoProcesarRequest request)
        {

            using var connection =
                DbHelper.OpenConnection(_portalDb, codEmpresa);

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                var resultado = AplicarAbonosMasivos(
                    connection,
                    transaction,
                    request);

                transaction.Commit();

                return DbHelper.CreateOkResponse(resultado);
            }
            catch
            {
                transaction.Rollback();
                return DbHelper.CreateErrorResponse(
                       "Error al procesar los abonos masivos.",
                       -1,
                       new CrAplicacionAbonoMasivoProcesarResponse());
            }
        }
    }
}
