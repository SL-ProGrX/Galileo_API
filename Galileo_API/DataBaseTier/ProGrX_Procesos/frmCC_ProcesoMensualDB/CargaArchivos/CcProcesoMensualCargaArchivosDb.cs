using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.Security;
using Galileo.Models.ERROR;
using System.Data; 
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Helpers;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualCargaArchivos;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.CargaArchivos
{
    public class CcProcesoMensualCargaArchivosDb
    {
        private readonly PortalDB _portalDb;
        private readonly int vModulo = 3;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly CcProcesoMensualGeneralDb _mGeneral;
        private readonly string _rutaBaseArchivos;

        /// <summary>
        /// Inicializa una nueva instancia para gestionar la carga de deducciones del proceso mensual.
        /// </summary>
        /// <param name="config">Configuración general de la aplicación.</param>
        public CcProcesoMensualCargaArchivosDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
            _mGeneral = new CcProcesoMensualGeneralDb(config);
            _rutaBaseArchivos = config["ArchivosGenerados:RutaBase"] ?? string.Empty;
        }

        /// <summary>
        /// Ejecuta la carga genérica de deducciones, aplicando validaciones, inserción y bitácora.
        /// </summary>
        /// <param name="request">Solicitud con los datos de carga.</param>
        /// <param name="reglas">Reglas de transformación y filtrado para la carga.</param>
        /// <returns>Resultado de la operación de carga.</returns>
        public ErrorDto<CcProcesoMensualCargaDeduccionesResponse> CargarDeduccionesGenerico(CcProcesoMensualCargaDeduccionesRequest request, IReadOnlyCollection<CcProcesoMensualReglaDeduccionConfig> reglas)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, request.CodEmpresa);

            _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, request.CodEmpresa, "03", "PRE", request.Usuario, request.CodInstitucion, request.FechaProceso);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
              

                var configuracion = ObtenerConfiguracionCarga(connection, transaction, request.CodInstitucion);

                EliminarCargaAnterior(connection, transaction, request);
                var tiposArchivoPlano = new HashSet<string>
                    {
                        "00",
                        "03",
                        "28",
                        "32",
                        "33"
                    };

                var registros = request.TipoCarga switch
                {
                    "30" =>
                        CrearRegistrosPrmCargadoDetallePorFila(
                            request,
                            configuracion,
                            usarCodigoObreroPatronal: true,
                            insertarSoloSiMontoMayorQueCero: true),

                    "02" =>
                        CrearRegistrosPrmCargadoDetallePorFila(
                            request,
                            configuracion,
                            usarCodigoObreroPatronal: false,
                            insertarSoloSiMontoMayorQueCero: false),

                    _ when tiposArchivoPlano.Contains(request.TipoCarga) =>
                          CrearRegistrosPrmCargado(
                                                    request,
                                                    reglas,
                                                    configuracion),
                    _ =>
                       CrearRegistrosPrmCargadoDesdeFilasProcesadas(request),
                };

                InsertarRegistrosPrmCargado(connection, transaction, registros);
                GuardarArchivoRecepcion(connection, transaction, request);

                RevisarCedulasCargadas(connection, transaction, request);

                MProcesoMensualDb.SbBitacoraPlanilla(connection,
                                                    new CcProcesoMensualBitacoraPlanillaDto
                                                    {
                                                        Transaccion = "03",
                                                        CodInstitucion = request.CodInstitucion,
                                                        Proceso = request.FechaProceso,
                                                        Gestion = "R",
                                                        Usuario = request.Usuario,
                                                        Documento = "Pla.Num." + request.Pago
                                                    }, transaction);


                 _Security_MainDB.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = request.CodEmpresa,
                    Usuario = request.Usuario,
                    DetalleMovimiento = $"PRM-CREDITO Carga Deducciones Inst: {request.CodInstitucion}",
                    Movimiento = "Aplica - WEB",
                    Modulo = vModulo
                });

                MarcarInstitucionCargaRealizada(connection, transaction, request.CodInstitucion);
              

                transaction.Commit();

                _mGeneral.CcProcesoMensual_ProcesosAdd_Ejecutar(connection, request.CodEmpresa, "03", "POS", request.Usuario, request.CodInstitucion, request.FechaProceso);
                var existenNoEncontrados = ObtenerPersonasNoEncontradas(  connection, request.CodInstitucion, request.FechaProceso);
                
                return DbHelper.CreateOkResponse(
                    new CcProcesoMensualCargaDeduccionesResponse
                    {
                        Cargado = true,
                        PersonasNoEncontradas = existenNoEncontrados,
                        RegistrosProcesados = request.Filas.Count,
                        RegistrosInsertados = registros.Count,
                        Mensaje = "Información cargada correctamente."
                    });
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return DbHelper.CreateErrorResponse<CcProcesoMensualCargaDeduccionesResponse>(
                    ex.Message,
                    -1,
                    new CcProcesoMensualCargaDeduccionesResponse());
            }
        }

        /// <summary>
        /// Construye los registros a insertar tomando filas ya procesadas del archivo plano.
        /// </summary>
        /// <param name="request">Solicitud con las filas de entrada.</param>
        /// <returns>Listado de registros listos para insertar.</returns>
        private static List<CcProcesoMensualPrmCargadoDbModel> CrearRegistrosPrmCargadoDesdeFilasProcesadas(CcProcesoMensualCargaDeduccionesRequest request)
        {
            var registros = new List<CcProcesoMensualPrmCargadoDbModel>();

            foreach (var fila in request.Filas)
            {
                if (string.IsNullOrWhiteSpace(fila.Cedula))
                {
                    continue;
                }

                if (fila.Monto <= 0)
                {
                    continue;
                }

                registros.Add(new CcProcesoMensualPrmCargadoDbModel
                {
                    CodInstitucion = request.CodInstitucion,
                    Pago = request.Pago,
                    FechaProceso = request.FechaProceso,
                    Tipo = fila.Tipo ?? 3,
                    Cedula = fila.Cedula.Trim(),
                    Monto = fila.Monto,
                    CodDeduccion = fila.Codigo.Trim(),
                    Up = fila.Up.Trim(),
                    Ut = fila.Ut.Trim()
                });
            }

            return registros;
        }

        /// <summary>
        /// Obtiene la configuración de carga de la institución.
        /// </summary>
        /// <param name="connection">Conexión activa a la base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <returns>Configuración de carga encontrada.</returns>
        private static CcProcesoMensualCargaConfigDbModel ObtenerConfiguracionCarga(IDbConnection connection, IDbTransaction transaction, int codInstitucion)
        {
            const string query = @"
               SELECT
                    ISNULL(I.planilla, '') AS Planilla,
                    ISNULL(I.codigo_aportes, '') AS CodigoAportes,
                    ISNULL(I.codigo_creditos, '') AS CodigoCreditos,
                    ISNULL(Ta.COD_DEDUCCION, I.codigo_aportes) AS CodigoObrero,
                    ISNULL(Tp.COD_DEDUCCION, 'x-PAT-x') AS CodigoPatronal
                FROM instituciones I
                LEFT JOIN vPrm_Codigos_Patrimonio Ta
                    ON I.cod_institucion = Ta.cod_institucion
                   AND Ta.Tipo = 'O'
                LEFT JOIN vPrm_Codigos_Patrimonio Tp
                    ON I.cod_institucion = Tp.cod_institucion
                   AND Tp.Tipo = 'P'
                WHERE I.cod_institucion = @CodInstitucion";

            return connection.QueryFirstOrDefault<CcProcesoMensualCargaConfigDbModel>(
                query,
                new { CodInstitucion = codInstitucion },
                transaction: transaction) ?? new CcProcesoMensualCargaConfigDbModel();
        }

        /// <summary>
        /// Genera registros de deducciones con base en reglas configuradas.
        /// </summary>
        /// <param name="request">Solicitud con filas de entrada.</param>
        /// <param name="reglas">Reglas a evaluar para crear registros.</param>
        /// <param name="configuracion">Configuración de la institución.</param>
        /// <returns>Listado de registros listos para insertar.</returns>
        private static List<CcProcesoMensualPrmCargadoDbModel> CrearRegistrosPrmCargado(CcProcesoMensualCargaDeduccionesRequest request, IEnumerable<CcProcesoMensualReglaDeduccionConfig> reglas, CcProcesoMensualCargaConfigDbModel configuracion)
        {
            var registros = new List<CcProcesoMensualPrmCargadoDbModel>();

            foreach (var fila in request.Filas.Where(f => !string.IsNullOrWhiteSpace(f.Cedula)))
            {
                foreach (var regla in reglas)
                {
                    if (!DebeAplicarRegla(regla, fila, configuracion))
                    {
                        continue;
                    }

                    var monto = ObtenerMonto(fila, regla.ColumnasOrigen);

                    if (regla.InsertaSoloSiMontoMayorQueCero && monto <= 0)
                    {
                        continue;
                    }

                    registros.Add(new CcProcesoMensualPrmCargadoDbModel
                    {
                        CodInstitucion = request.CodInstitucion,
                        Pago = request.Pago,
                        FechaProceso = request.FechaProceso,
                        Tipo = regla.Tipo,
                        Cedula = fila.Cedula.Trim(),
                        Monto = monto,
                        CodDeduccion = regla.CodDeduccion
                    });
                }
            }

            return registros;
        }

        /// <summary>
        /// Determina si una regla debe aplicarse a una fila según la configuración vigente.
        /// </summary>
        /// <param name="regla">Regla de deducción a validar.</param>
        /// <param name="fila">Fila de datos a evaluar.</param>
        /// <param name="configuracion">Configuración de carga de la institución.</param>
        /// <returns><c>true</c> si la regla aplica; en caso contrario, <c>false</c>.</returns>
        private static bool DebeAplicarRegla(CcProcesoMensualReglaDeduccionConfig regla, CcProcesoMensualCargaDeduccionFilaRequest fila, CcProcesoMensualCargaConfigDbModel configuracion)
        {
            if (regla.RequiereAportesHabilitados && EsCodigoNo(configuracion.CodigoAportes))
            {
                return false;
            }

            if (regla.RequiereCreditosHabilitados && EsCodigoNo(configuracion.CodigoCreditos))
            {
                return false;
            }

            if (regla.RequiereColumnaExistente && !TieneTodasLasColumnas(fila, regla.ColumnasOrigen))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Crea registros a partir de filas que contienen código de deducción explícito.
        /// </summary>
        /// <param name="request">Solicitud con filas de entrada.</param>
        /// <param name="configuracion">Configuración de la institución.</param>
        /// <param name="usarCodigoObreroPatronal">Indica si se valida contra códigos obrero/patronal.</param>
        /// <param name="insertarSoloSiMontoMayorQueCero">Indica si solo se insertan montos positivos.</param>
        /// <returns>Listado de registros listos para insertar.</returns>
        private static List<CcProcesoMensualPrmCargadoDbModel> CrearRegistrosPrmCargadoDetallePorFila(
           CcProcesoMensualCargaDeduccionesRequest request,
           CcProcesoMensualCargaConfigDbModel configuracion,
           bool usarCodigoObreroPatronal,
           bool insertarSoloSiMontoMayorQueCero)
        {
            var registros = new List<CcProcesoMensualPrmCargadoDbModel>();

            foreach (var fila in request.Filas)
            {
                if (string.IsNullOrWhiteSpace(fila.Cedula))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(fila.Codigo))
                {
                    continue;
                }

                if (EsEncabezadoCodigoDeduccion(fila.Codigo))
                {
                    continue;
                }

                if (insertarSoloSiMontoMayorQueCero && fila.Monto <= 0)
                {
                    continue;
                }

                var codigo = fila.Codigo.Trim();

                registros.Add(new CcProcesoMensualPrmCargadoDbModel
                {
                    CodInstitucion = request.CodInstitucion,
                    Pago = request.Pago,
                    FechaProceso = request.FechaProceso,
                    Tipo = ObtenerTipoDetallePorFila(
                        codigo,
                        configuracion,
                        usarCodigoObreroPatronal),
                    Cedula = fila.Cedula.Trim(),
                    Monto = fila.Monto,
                    CodDeduccion = codigo
                });
            }

            return registros;
        }

        /// <summary>
        /// Indica si el valor de código corresponde al encabezado del archivo.
        /// </summary>
        /// <param name="codigo">Código a evaluar.</param>
        /// <returns><c>true</c> si es encabezado; en caso contrario, <c>false</c>.</returns>
        private static bool EsEncabezadoCodigoDeduccion(string codigo)
        {
            return string.Equals(
                codigo?.Trim(),
                "codigodeduccion",
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Obtiene el tipo de registro según el código y la configuración de carga.
        /// </summary>
        /// <param name="codigo">Código de deducción.</param>
        /// <param name="configuracion">Configuración de la institución.</param>
        /// <param name="usarCodigoObreroPatronal">Indica si se debe validar con códigos obrero/patronal.</param>
        /// <returns>Tipo de registro calculado.</returns>
        private static int ObtenerTipoDetallePorFila(string codigo, CcProcesoMensualCargaConfigDbModel configuracion, bool usarCodigoObreroPatronal)
        {
            if (usarCodigoObreroPatronal)
            {
                return EsCodigoAporteOPatronal(codigo, configuracion) ? 1 : 3;
            }

            return string.Equals(
                codigo,
                configuracion.CodigoAportes?.Trim(),
                StringComparison.OrdinalIgnoreCase)
                    ? 1
                    : 3;
        }

        /// <summary>
        /// Evalúa si el código corresponde a aporte obrero o patronal.
        /// </summary>
        /// <param name="codigo">Código a validar.</param>
        /// <param name="configuracion">Configuración de la institución.</param>
        /// <returns><c>true</c> si coincide con código obrero o patronal; en caso contrario, <c>false</c>.</returns>
        private static bool EsCodigoAporteOPatronal(string codigo, CcProcesoMensualCargaConfigDbModel configuracion)
        {
            return string.Equals(
                    codigo,
                    configuracion.CodigoObrero?.Trim(),
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    codigo,
                    configuracion.CodigoPatronal?.Trim(),
                    StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Verifica que la fila contenga todas las columnas requeridas por una regla.
        /// </summary>
        /// <param name="fila">Fila de datos a validar.</param>
        /// <param name="columnasOrigen">Columnas requeridas.</param>
        /// <returns><c>true</c> si contiene todas las columnas; en caso contrario, <c>false</c>.</returns>
        private static bool TieneTodasLasColumnas(CcProcesoMensualCargaDeduccionFilaRequest fila, IEnumerable<string> columnasOrigen)
        {
            return columnasOrigen.All(fila.Montos.ContainsKey);
        }

        /// <summary>
        /// Suma los montos de las columnas existentes indicadas en una fila.
        /// </summary>
        /// <param name="fila">Fila que contiene los montos.</param>
        /// <param name="columnasOrigen">Columnas a considerar para el cálculo.</param>
        /// <returns>Monto total calculado.</returns>
        private static decimal ObtenerMonto(CcProcesoMensualCargaDeduccionFilaRequest fila, IEnumerable<string> columnasOrigen)
        { 
            return columnasOrigen
                .Where(fila.Montos.ContainsKey)
                .Sum(columna => fila.Montos[columna]);
              
        }

        /// <summary>
        /// Determina si un código está marcado como NO.
        /// </summary>
        /// <param name="codigo">Código a evaluar.</param>
        /// <returns><c>true</c> si el código es NO; en caso contrario, <c>false</c>.</returns>
        private static bool EsCodigoNo(string? codigo)
        {
            return string.Equals(
                codigo?.Trim(),
                "NO",
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Elimina la carga previa de una institución para el proceso y pago indicados.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="request">Solicitud con los criterios de eliminación.</param>
        private static void EliminarCargaAnterior(IDbConnection connection, IDbTransaction transaction, CcProcesoMensualCargaDeduccionesRequest request)
        {
            const string query = @"
            DELETE prm_cargado
            WHERE fecha_proceso = @FechaProceso
              AND pago = @Pago
              AND cod_institucion = @CodInstitucion";

            connection.Execute(
                query,
                new
                {
                    request.FechaProceso,
                    request.Pago,
                    request.CodInstitucion
                },
                transaction: transaction,
                commandTimeout: 0);
        }

        /// <summary>
        /// Guarda una copia del archivo recibido en la carpeta de planilla de la institución y año del proceso.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="request">Solicitud con los datos del archivo recibido.</param>
        private void GuardarArchivoRecepcion(IDbConnection connection, IDbTransaction transaction, CcProcesoMensualCargaDeduccionesRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NombreArchivo)
                || string.IsNullOrWhiteSpace(_rutaBaseArchivos))
            {
                return;
            }

            var nombreArchivo = ObtenerNombreArchivoRecepcion(request.NombreArchivo);
            var rutaDirectorio = ObtenerRutaDirectorioRecepcion(connection, transaction, request, _rutaBaseArchivos);
            var rutaArchivo = CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(
                _rutaBaseArchivos,
                rutaDirectorio,
                nombreArchivo);

            var contenido = Convert.FromBase64String(request.ArchivoBase64);

            CcProcesoMensualArchivoRutaHelperDb.CrearDirectorioSiNoExiste(
                _rutaBaseArchivos,
                rutaDirectorio);

            File.WriteAllBytes(rutaArchivo, contenido);
        }

        /// <summary>
        /// Obtiene la ruta de recepción homologada con la generación de archivos de planilla.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="request">Solicitud con institución y proceso.</param>
        /// <param name="rutaBaseArchivos">Ruta base configurada para los archivos de planilla.</param>
        /// <returns>Ruta del directorio de recepción.</returns>
        private static string ObtenerRutaDirectorioRecepcion(IDbConnection connection, IDbTransaction transaction, CcProcesoMensualCargaDeduccionesRequest request, string rutaBaseArchivos)
        {
            var nombreInstitucion = connection.QuerySingleOrDefault<string>(
                "SELECT ISNULL(descripcion, '') FROM instituciones WHERE cod_institucion = @CodInstitucion",
                new { request.CodInstitucion },
                transaction: transaction) ?? string.Empty;

            var requestRuta = new CcProcesoMensualGeneraArchivoRequest
            {
                EmpresaId = request.CodEmpresa,
                CodInstitucion = request.CodInstitucion,
                FechaProceso = request.FechaProceso,
                NombreInstitucion = nombreInstitucion
            };

            return CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(
                requestRuta,
                rutaBaseArchivos);
        }

        /// <summary>
        /// Normaliza el nombre del archivo recibido evitando rutas o segmentos no permitidos.
        /// </summary>
        /// <param name="nombreArchivo">Nombre original del archivo.</param>
        /// <returns>Nombre seguro del archivo recibido.</returns>
        private static string ObtenerNombreArchivoRecepcion(string nombreArchivo)
        {
            var nombreSeguro = Path.GetFileName(nombreArchivo);

            if (string.IsNullOrWhiteSpace(nombreSeguro)
                || !string.Equals(nombreSeguro, nombreArchivo, StringComparison.Ordinal)
                || nombreSeguro.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("El nombre del archivo de recepción no es válido.", nameof(nombreArchivo));
            }

            return $"R-{nombreSeguro}";
        }

        /// <summary>
        /// Inserta en lote los registros de deducciones procesados.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="registros">Registros a insertar.</param>
        private static void InsertarRegistrosPrmCargado(IDbConnection connection, IDbTransaction transaction, List<CcProcesoMensualPrmCargadoDbModel> registros)
        {
            if (registros.Count == 0)
            {
                return;
            }

            const string query = @"
                    INSERT INTO prm_cargado(
                        cod_institucion,
                        pago,
                        fecha_proceso,
                        tipo,
                        cedula,
                        monto,
                        cod_deduccion,
                        UP,
                        UT)
                    VALUES(
                        @CodInstitucion,
                        @Pago,
                        @FechaProceso,
                        @Tipo,
                        @Cedula,
                        @Monto,
                        @CodDeduccion,
                        @Up,
                        @Ut)";

            foreach (var lote in registros.Chunk(500))
            {
                connection.Execute(
                    query,
                    lote,
                    transaction: transaction);
            }
        }

        /// <summary>
        /// Ejecuta la revisión de cédulas cargadas mediante procedimiento almacenado.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="request">Solicitud con criterios de revisión.</param>
        private static void RevisarCedulasCargadas(IDbConnection connection, IDbTransaction transaction, CcProcesoMensualCargaDeduccionesRequest request)
        {
            const string query = @"
            EXEC spPrmCargado_Revision_Cedulas
                @CodInstitucion,
                @FechaProceso,
                @Pago";

            connection.Execute(
                query,
                new
                {
                    request.CodInstitucion,
                    request.FechaProceso,
                    request.Pago
                },
                transaction: transaction);
        }

        /// <summary>
        /// Marca la institución como cargada en el proceso mensual.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        private static void MarcarInstitucionCargaRealizada(IDbConnection connection, IDbTransaction transaction, int codInstitucion)
        {
            const string query = @"
            UPDATE instituciones
            SET pr_carga = 1
            WHERE cod_institucion = @CodInstitucion";

            connection.Execute(
                query,
                new { CodInstitucion = codInstitucion },
                transaction: transaction);
        }

        /// <summary>
        /// Obtiene la cantidad de personas no encontradas tras la carga.
        /// </summary>
        /// <param name="connection">Conexión activa a base de datos.</param>
        /// <param name="codInstitucion">Código de la institución.</param>
        /// <param name="fechaProceso">Fecha del proceso.</param>
        /// <returns>Cantidad de personas no encontradas.</returns>
        public static int ObtenerPersonasNoEncontradas( IDbConnection connection, int codInstitucion, decimal fechaProceso)
        {
            const string query = @"  EXEC spPrmCargadoPersonasNoEncontradas  @CodInstitucion, @FechaProceso";
            var result = connection.QueryFirstOrDefault<PersonasNoEncontradasDbModel>(
                query,
                new
                {
                    CodInstitucion = codInstitucion,
                    FechaProceso = fechaProceso
                });

            return result?.Existen ?? 0;
        }
    }
    public sealed class PersonasNoEncontradasDbModel
    {
        public int Existen { get; set; }
    }
    internal sealed class CcProcesoMensualPrmCargadoDbModel
    {
        public int CodInstitucion { get; set; }
        public int Pago { get; set; }
        public decimal FechaProceso { get; set; }
        public int Tipo { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string CodDeduccion { get; set; } = string.Empty;
        public string Up { get; set; } = string.Empty;
        public string Ut { get; set; } = string.Empty;

    }
}
