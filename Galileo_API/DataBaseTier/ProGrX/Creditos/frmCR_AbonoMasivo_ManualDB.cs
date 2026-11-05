using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;
using System.Globalization;
using static Galileo_API.Models.ProGrX.Creditos.FrmCrAbonoMasivoManualModels;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrAbonoMasivoManualDb
    {
        private readonly PortalDB _portalDb;

        private const string TipoCarga = "C";
        private const string ProcesoCarga = "CrdAplAbo";

        public FrmCrAbonoMasivoManualDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las operadoras disponibles para el abono masivo manual.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CR_AbonoMasivo_Manual_Operadoras_Obtener(int codEmpresa)
        {
            const string query = """
                SELECT
                    cod_operadora AS Item,
                    descripcion AS Descripcion
                FROM FND_Operadoras
                """;

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection
                    .Query<DropDownListaGenericaModel>(query)
                    .ToList());
        }

        /// <summary>
        /// Obtiene los planes de la operadora seleccionada.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CR_AbonoMasivo_Manual_Planes_Obtener(
                int codEmpresa,
                string operadora)
        {
            const string query = """
                SELECT
                    RTRIM(cod_plan) AS Item,
                    RTRIM(descripcion) + SPACE(10) +
                        '[' + RTRIM(cod_plan) + ']' AS Descripcion
                FROM fnd_planes
                WHERE deduce_independiente = 1
                  AND cod_operadora = @operadora
                """;

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection => connection
                    .Query<DropDownListaGenericaModel>(
                        query,
                        new { operadora })
                    .ToList());
        }

        /// <summary>
        /// Carga los registros mediante spSys_Carga_Masiva_Sube y luego
        /// consulta los registros reconocidos por el procedimiento de revisión.
        /// Cada registro se ejecuta directamente, sin transacción del DB.
        /// </summary>
        public ErrorDto<CrAplicacionAbonoMasivoResponse>
            CR_AbonoMasivo_Manual_CargaDeducciones_Procesar(
                int codEmpresa,
                CrAplicacionAbonoMasivoRequest request)
        {
            var respuestaVacia = new CrAplicacionAbonoMasivoResponse();

            if (request is null ||
                string.IsNullOrWhiteSpace(request.Usuario) ||
                request.Usuario.Length > 30 ||
                request.Registros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "La solicitud de carga está incompleta.",
                    -1,
                    respuestaVacia);
            }

            if (request.Registros.Count == 0)
            {
                return DbHelper.CreateOkResponse(respuestaVacia);
            }

            // El SP convierte LLAVE_01 a int y recibe MONTO_01 como decimal(16,2).
            // Se valida toda la solicitud antes de iniciar cualquier escritura.
            if (request.Registros.Any(registro =>
                    registro is null ||
                    registro.Operacion <= 0 ||
                    registro.Operacion > int.MaxValue ||
                    registro.Abono <= 0 ||
                    registro.Abono > 99999999999999.99m ||
                    decimal.Round(registro.Abono, 2) != registro.Abono))
            {
                return DbHelper.CreateErrorResponse(
                    "Hay operaciones o montos de abono inválidos. No se inició la carga.",
                    -1,
                    respuestaVacia);
            }

            var cargados = 0;
            var cargaIniciada = false;
            var cargaTerminada = false;

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);

                connection.Open();

                cargaIniciada = true;
                CargarRegistros(
                    connection,
                    request.Registros,
                    request.Usuario,
                    ref cargados);

                cargaTerminada = true;

                var registros = RevisarRegistros(
                    connection,
                    request.Usuario);

                if (registros.Count == 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "La carga terminó, pero la revisión no encontró operaciones válidas. " +
                        "Revise los registros cargados antes de intentar aplicar los abonos.",
                        -1,
                        respuestaVacia);
                }

                var resultado = new CrAplicacionAbonoMasivoResponse
                {
                    CantidadCasos = registros.Count,
                    MontoTotal = registros.Sum(x => x.Abono),
                    Registros = registros
                };

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception)
            {
                string mensaje;

                if (!cargaIniciada)
                {
                    mensaje =
                        "No se pudo abrir la conexión para cargar los abonos. " +
                        "No se inició la carga.";
                }
                else if (!cargaTerminada)
                {
                    mensaje =
                        $"Falló la carga después de completar {cargados} registro(s). " +
                        "Los datos pueden haber quedado parciales; el primer intento " +
                        "también pudo haber limpiado registros anteriores. " +
                        "Revise la carga antes de reintentar.";
                }
                else
                {
                    mensaje =
                        "Los registros se cargaron, pero falló su revisión. " +
                        "Revise la carga antes de reintentar o aplicar los abonos.";
                }

                return DbHelper.CreateErrorResponse(
                    mensaje,
                    -1,
                    respuestaVacia);
            }
        }

        /// <summary>
        /// Envía cada abono al procedimiento de carga. Se conserva la limpieza
        /// inicial que realiza el SP solamente para el primer registro.
        /// </summary>
        private static void CargarRegistros(
            IDbConnection connection,
            IReadOnlyCollection<CrAplicacionAbonoMasivoRegistroRequest> registros,
            string usuario,
            ref int cargados)
        {
            foreach (var registro in registros)
            {
                connection.Execute(
                    SqlCargaMasiva,
                    CrearParametrosCarga(
                        registro,
                        usuario,
                        limpiar: cargados == 0),
                    commandType: CommandType.Text);

                cargados++;
            }
        }

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
        /// Crea los parámetros posicionales esperados por
        /// spSys_Carga_Masiva_Sube.
        /// </summary>
        private static object CrearParametrosCarga(
            CrAplicacionAbonoMasivoRegistroRequest registro,
            string usuario,
            bool limpiar)
        {
            return new
            {
                Tipo = TipoCarga,
                Proceso = ProcesoCarga,
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
        /// Consulta los abonos cargados que reconoce el procedimiento de revisión.
        /// </summary>
        private static List<CrAplicacionAbonoMasivoDetalle> RevisarRegistros(
            IDbConnection connection,
            string usuario)
        {
            const string sql = """
                EXEC spSys_Carga_Masiva_Revisa_AplMasCrd_Abono
                     @Tipo,
                     @Proceso,
                     @Llave01,
                     @Llave02,
                     @Usuario;
                """;

            return connection.Query<CrAplicacionAbonoMasivoDetalle>(
                sql,
                new
                {
                    Tipo = TipoCarga,
                    Proceso = ProcesoCarga,
                    Llave01 = string.Empty,
                    Llave02 = string.Empty,
                    Usuario = usuario
                },
                commandType: CommandType.Text).ToList();
        }

        /// <summary>
        /// Ejecuta directamente el procedimiento que aplica los abonos.
        /// Sus cambios internos no se revierten desde este DB si ocurre un error.
        /// </summary>
        private static CrAplicacionAbonoMasivoProcesarResponse?
            AplicarAbonosMasivos(
                IDbConnection connection,
                CrAplicacionAbonoMasivoProcesarRequest request)
        {
            const string procedure =
                "spSys_Carga_Masiva_Aplica_AplMasCrd_Abono";

            var documento =
                connection.QuerySingleOrDefault<DocumentoAplicacionDb>(
                    procedure,
                    new
                    {
                        Tipo = TipoCarga,
                        ProcesoId = ProcesoCarga,
                        Llave_01 = string.Empty,
                        Llave_02 = string.Empty,
                        request.Usuario,
                        request.Operadora,
                        request.Plan,
                        request.Cuenta,
                        FondoComun = request.FondoGeneral ? 1 : 0,
                        TipoApl = request.Tipo
                    },
                    commandType: CommandType.StoredProcedure);

            if (documento is null ||
                string.IsNullOrWhiteSpace(documento.Tipo_Documento) ||
                string.IsNullOrWhiteSpace(documento.Num_Documento))
            {
                return null;
            }

            // El SP devuelve Tipo_Documento y Num_Documento; el modelo público
            // conserva TipoDocumento y NumeroDocumento para sus consumidores.
            return new CrAplicacionAbonoMasivoProcesarResponse
            {
                TipoDocumento = documento.Tipo_Documento,
                NumeroDocumento = documento.Num_Documento
            };
        }

        /// <summary>
        /// Aplica los abonos masivos sin iniciar una transacción en el DB.
        /// Valida los datos y las condiciones básicas antes de llamar al SP.
        /// </summary>
        public ErrorDto<CrAplicacionAbonoMasivoProcesarResponse>
            ProcesarAbonosMasivos(
                int codEmpresa,
                CrAplicacionAbonoMasivoProcesarRequest request)
        {
            var respuestaVacia =
                new CrAplicacionAbonoMasivoProcesarResponse();

            if (request is null ||
                string.IsNullOrWhiteSpace(request.Usuario) ||
                request.Usuario.Length > 30 ||
                request.Operadora <= 0 ||
                request.Operadora > int.MaxValue ||
                string.IsNullOrWhiteSpace(request.Plan) ||
                request.Plan.Length > 10 ||
                string.IsNullOrWhiteSpace(request.Cuenta) ||
                request.Cuenta.Length > 10 ||
                string.IsNullOrWhiteSpace(request.Tipo) ||
                request.Tipo.Length != 1)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos necesarios para aplicar los abonos están incompletos o son inválidos.",
                    -1,
                    respuestaVacia);
            }

            var procedimientoIniciado = false;

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);

                connection.Open();

                const string sqlPendientes = """
                    SELECT COUNT(1)
                    FROM SYS_CARGA_MASIVA_LOAD AS L
                    INNER JOIN reg_Creditos AS R
                        ON TRY_CONVERT(int, L.LLAVE_01) = R.ID_SOLICITUD
                    INNER JOIN Socios AS S
                        ON R.CEDULA = S.CEDULA
                    WHERE L.Tipo = @Tipo
                      AND L.Proceso_id = @ProcesoId
                      AND L.Usuario = @Usuario
                      AND ISNULL(L.Procesado, 0) = 0;
                    """;

                var pendientes = connection.ExecuteScalar<int>(
                    sqlPendientes,
                    new
                    {
                        Tipo = TipoCarga,
                        ProcesoId = ProcesoCarga,
                        request.Usuario
                    });

                if (pendientes == 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "No hay abonos pendientes válidos para este usuario. " +
                        "No se ejecutó la aplicación.",
                        -1,
                        respuestaVacia);
                }

                const string sqlDisponible = """
                    SELECT SUM(Aportes)
                    FROM FND_CONTRATOS
                    WHERE COD_OPERADORA = @Operadora
                      AND COD_PLAN = @Plan
                      AND ESTADO = 'A';
                    """;

                var disponible =
                    connection.ExecuteScalar<decimal?>(
                        sqlDisponible,
                        new
                        {
                            request.Operadora,
                            request.Plan
                        }) ?? 0m;

                if (disponible <= 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "El fondo no tiene saldo disponible. " +
                        "No se ejecutó la aplicación.",
                        -1,
                        respuestaVacia);
                }

                procedimientoIniciado = true;

                var resultado = AplicarAbonosMasivos(
                    connection,
                    request);

                if (resultado is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "El procedimiento terminó sin devolver un documento. " +
                        "Puede haber modificado datos o el consecutivo. " +
                        "Revise las operaciones, el fondo y el asiento antes de reintentar.",
                        -1,
                        respuestaVacia);
                }

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (Exception)
            {
                var mensaje = procedimientoIniciado
                    ? "Falló la aplicación de los abonos. El procedimiento puede " +
                      "haber realizado cambios parciales. Revise las operaciones, " +
                      "el fondo, el documento y el asiento antes de reintentar."
                    : "No se pudo validar la aplicación de los abonos. " +
                      "No se inició el procedimiento de aplicación.";

                return DbHelper.CreateErrorResponse(
                    mensaje,
                    -1,
                    respuestaVacia);
            }
        }

        /// <summary>
        /// Representa los nombres exactos de las columnas devueltas por el SP.
        /// </summary>
        private sealed class DocumentoAplicacionDb
        {
            public string Tipo_Documento { get; set; } = string.Empty;
            public string Num_Documento { get; set; } = string.Empty;
        }
    }
}