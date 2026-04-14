using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoGestorExternoModels;
using Galileo.Models.Security;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoGestorExternoDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;
        private const int ModuloCobros = 4;

        private const string SpGestorExternoList = @"
            EXEC spCBR_Gestor_Externo_List
                @Estado,
                @Inicio,
                @Corte,
                @Filtro,
                @Expediente,
                @Usuario,
                @Operacion,
                @Gestiona;";

        private const string SpGestorExternoAdd = @"
            EXEC spCBR_Gestor_Externo_Add
                @Operacion,
                @Gestiona,
                @Cedula,
                @Nombre,
                @Expediente,
                @Notas,
                @Usuario;";

        private const string SpGestorExternoDel = @"
            EXEC spCBR_Gestor_Externo_Del
                @Id,
                @Notas,
                @Usuario;";

        private const string SpGestorExternoDelMasivo = @"
            EXEC spCBR_Gestor_Externo_Del_Masivo
                @Operacion,
                @Notas,
                @Usuario;";

        private const string QueryOperaciones = @"
            SELECT
                Id_Solicitud,
                Cedula,
                Nombre,
                Codigo,
                Antiguedad,
                Saldo
            FROM vCbr_Gestion_Externa_Operaciones_General_Lista
            ORDER BY Id_Solicitud;";

        private const string QueryGestores = @"
            SELECT
                RTRIM(Usuario) AS item,
                RTRIM(Usuario) AS descripcion
            FROM CBR_USUARIOS
            WHERE OPERADOR_EXTERNO = 1
            ORDER BY Usuario;";

        public FrmCoGestorExternoDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Registro en bitacora
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        /// <summary>
        /// Consulta el listado de casos con gestor externo según filtros indicados
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdGestorExternoListaItemModel>> Crd_GestorExterno_Listado_Obtener(int codEmpresa,
            CrdGestorExternoFiltroRequest request)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var (inicio, corte) = ResolverRangoFechas(request);
                var parameters = CrearParametrosListado(request, inicio, corte);

                return conn.Query<CrdGestorExternoListaItemModel>(SpGestorExternoList, parameters).ToList();
            });
        }

        /// <summary>
        /// Registra un caso con gestor externo para una operación dada, siempre que no exista un caso activo para la misma operación. Si el registro es exitoso, se almacena un movimiento en bitácora con el detalle del caso registrado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<string> Crd_GestorExterno_Registrar(int codEmpresa,CrdGestorExternoRegistrarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var mensajeValidacion = ValidarRegistrar(request);
                if (!string.IsNullOrEmpty(mensajeValidacion))
                {
                    return CrearErrorString(mensajeValidacion, -1);
                }

                var result = EjecutarSp(
                    connection,
                    SpGestorExternoAdd,
                    CrearParametrosRegistrar(request));

                var respuesta = ConstruirRespuestaStringDesdeSp(
                    result,
                    "No se recibió respuesta del proceso de registro.",
                    "No fue posible registrar el caso.",
                    -2);

                if (respuesta.Code == 0 && result is not null)
                {
                    RegistrarBitacora(codEmpresa, request.UsuarioEjecuta, result);
                    respuesta.Result =
                        $"{result.Movimiento} Caso con Gestor Externo, procesado satisfactoriamente!";
                }

                return respuesta;
            }
            catch (Exception)
            {
                return CrearErrorString("Error al registrar el caso con gestor externo.", -1);
            }
        }

        /// <summary>
        /// Reversa el registro de un caso con gestor externo, siempre que el caso se encuentre activo. Si la reversa es exitosa, se almacena un movimiento en bitácora con el detalle del caso reversado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<string> Crd_GestorExterno_Reversar(int codEmpresa,CrdGestorExternoReversaRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var mensajeValidacion = ValidarReversa(request);
                if (!string.IsNullOrEmpty(mensajeValidacion))
                {
                    return CrearErrorString(mensajeValidacion, -1);
                }

                var result = EjecutarSp(
                    connection,
                    SpGestorExternoDel,
                    CrearParametrosReversa(request));

                var respuesta = ConstruirRespuestaStringDesdeSp(
                       result,
                       "No se recibió respuesta del proceso de registro.",
                       "No fue posible registrar el caso.",
                       -2);

                if (respuesta.Code == 0 && result is not null)
                {
                    RegistrarBitacora(codEmpresa, request.UsuarioEjecuta, result);
                    respuesta.Result =
                        $"{result.Movimiento} Caso con Gestion Externa, desvinculado satisfactoriamente!";
                }

                return respuesta;

            }
            catch (Exception)
            {
                return CrearErrorString("Error al registrar el caso con gestor externo.", -1);
            }
        }

        /// <summary>
        ///  Consulta el listado de operaciones para accion de f4
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrdGestorExternoOperacionModel>> Crd_GestorExterno_Operacion_Buscar(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<CrdGestorExternoOperacionModel>(QueryOperaciones).ToList());
        }

        /// <summary>
        /// Consulta el listado de usuarios para asignar la gestion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_GestorExterno_Gestores_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.Query<DropDownListaGenericaModel>(QueryGestores).ToList());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrdGestorExternoCargaMasivaResponse> Crd_GestorExterno_CargaMasiva_Procesar(int codEmpresa,CrdGestorExternoCargaMasivaRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                var response = CrearRespuestaInicial(request);
                var validacion = ValidarCargaMasiva(request, response);

                if (validacion is not null)
                {
                    return validacion;
                }

                foreach (var registro in request.Registros)
                {
                    ProcesarRegistroCargaMasiva(connection, request, registro, response);
                }

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CrdGestorExternoCargaMasivaResponse>(
                    "Error al procesar la carga masiva de gestor externo.",
                    -1,
                    new CrdGestorExternoCargaMasivaResponse());
            }
        }

        /// <summary>
        /// Revisa y valida las fechas del filtro de busqueda
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static (DateTime inicio, DateTime corte) ResolverRangoFechas(CrdGestorExternoFiltroRequest request)
        {
            if (request.IgnorarFechas)
            {
                return (
                    new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2100, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            }

            var inicio = DateTime.SpecifyKind(
                (request.FechaInicio ?? DateTime.Today).Date,
                DateTimeKind.Utc);

            var corte = DateTime.SpecifyKind(
                (request.FechaCorte ?? DateTime.Today).Date.AddDays(1).AddTicks(-1),
                DateTimeKind.Utc);

            return (inicio, corte);
        }

        /// <summary>
        /// creacion de parametros para consulta de listado de casos con gestor externo, aplicando limpieza de texto y valores por defecto según corresponda
        /// </summary>
        /// <param name="request"></param>
        /// <param name="inicio"></param>
        /// <param name="corte"></param>
        /// <returns></returns>
        private static DynamicParameters CrearParametrosListado(CrdGestorExternoFiltroRequest request,DateTime inicio, DateTime corte)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Estado", ObtenerEstado(request.Estado));
            parameters.Add("@Inicio", inicio);
            parameters.Add("@Corte", corte);
            parameters.Add("@Filtro", LimpiarTexto(request.Filtro));
            parameters.Add("@Expediente", LimpiarTexto(request.Expediente));
            parameters.Add("@Usuario", LimpiarTexto(request.Usuario));
            parameters.Add("@Operacion", request.Operacion);
            parameters.Add("@Gestiona", ObtenerGestiona(request.Gestiona));
            return parameters;
        }

        /// <summary>
        /// Crea parametros para el registro
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static DynamicParameters CrearParametrosRegistrar(CrdGestorExternoRegistrarRequest request)
        {
            var parameters = CrearParametrosOperacionBase(
                request.Operacion,
                request.Notas,
                request.UsuarioEjecuta);

            parameters.Add("@Gestiona", LimpiarTexto(request.GestionUsuario));
            parameters.Add("@Cedula", LimpiarTexto(request.Cedula));
            parameters.Add("@Nombre", LimpiarTexto(request.Nombre));
            parameters.Add("@Expediente", LimpiarTexto(request.Expediente));
            return parameters;
        }

        /// <summary>
        /// Crea parametros para la reversion 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static DynamicParameters CrearParametrosReversa(CrdGestorExternoReversaRequest request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Id", request.CasoId);
            parameters.Add("@Notas", LimpiarTexto(request.Notas));
            parameters.Add("@Usuario", LimpiarTexto(request.UsuarioEjecuta));
            return parameters;
        }

        /// <summary>
        /// Crea parametros base para una operación
        /// </summary>
        /// <param name="operacion"></param>
        /// <param name="notas"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static DynamicParameters CrearParametrosOperacionBase(long operacion,string? notas,string? usuario)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Operacion", operacion);
            parameters.Add("@Notas", LimpiarTexto(notas));
            parameters.Add("@Usuario", LimpiarTexto(usuario));
            return parameters;
        }

        /// <summary>
        /// Crea parametros para la asignación de un gestor externo
        /// </summary>
        /// <param name="request"></param>
        /// <param name="registro"></param>
        /// <returns></returns>
        private static DynamicParameters CrearParametrosAsignacion(CrdGestorExternoCargaMasivaRequest request, CrdGestorExternoCargaFilaRequest registro)
        {
            var parameters = CrearParametrosOperacionBase(
                registro.Operacion,
                registro.Notas,
                request.UsuarioEjecuta);

            parameters.Add("@Gestiona", LimpiarTexto(request.GestionUsuario));
            parameters.Add("@Cedula", string.Empty);
            parameters.Add("@Nombre", string.Empty);
            parameters.Add("@Expediente", LimpiarTexto(registro.Expediente));
            return parameters;
        }

        /// <summary>
        /// Crea parametros para la desvinvulacion
        /// </summary>
        /// <param name="request"></param>
        /// <param name="registro"></param>
        /// <returns></returns>
        private static DynamicParameters CrearParametrosDesvinculacion(CrdGestorExternoCargaMasivaRequest request, CrdGestorExternoCargaFilaRequest registro)
        {
            return CrearParametrosOperacionBase(
                registro.Operacion,
                registro.Notas,
                request.UsuarioEjecuta);
        }

        /// <summary>
        /// Ejecuta el sp correspondiente
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="query"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static CrdGestorExternoSpResponse? EjecutarSp( IDbConnection connection, string query, DynamicParameters parameters)
        {
            return connection.QueryFirstOrDefault<CrdGestorExternoSpResponse>(query, parameters);
        }

       /// <summary>
       /// Validaciones de registro
       /// </summary>
       /// <param name="request"></param>
       /// <returns></returns>
        private static string? ValidarRegistrar(CrdGestorExternoRegistrarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.GestionUsuario))
            {
                return "Debe indicar un gestor externo.";
            }

            if (request.Operacion <= 0)
            {
                return "Debe indicar una operación válida.";
            }

            if (string.IsNullOrWhiteSpace(request.Cedula))
            {
                return "Debe indicar la cédula.";
            }

            return ValidarLongitudMinimaNotas(request.Notas, 10);
        }

       /// <summary>
       /// Validacion de reversion 
       /// </summary>
       /// <param name="request"></param>
       /// <returns></returns>
        private static string? ValidarReversa(CrdGestorExternoReversaRequest request)
        {
            if (request.CasoId <= 0)
            {
                return "Debe indicar un caso válido.";
            }

            return ValidarLongitudMinimaNotas(request.Notas, 30);
        }

        /// <summary>
        /// Validacion de nota
        /// </summary>
        /// <param name="notas"></param>
        /// <param name="minimo"></param>
        /// <returns></returns>
        private static string? ValidarLongitudMinimaNotas(string? notas, int minimo)
        {
            if (string.IsNullOrWhiteSpace(notas) || notas.Trim().Length < minimo)
            {
                return $"Debe indicar una nota válida de al menos {minimo} caracteres.";
            }

            return null;
        }

       /// <summary>
       /// Creacion de respuesta de error
       /// </summary>
       /// <param name="mensaje"></param>
       /// <param name="codigo"></param>
       /// <returns></returns>
        private static ErrorDto<string> CrearErrorString(string mensaje, int codigo)
        {
            return DbHelper.CreateErrorResponse<string>(mensaje, codigo, string.Empty);
        }

        /// <summary>
        /// Creacion de respuesta de sp
        /// </summary>
        /// <param name="result"></param>
        /// <param name="mensajeNull"></param>
        /// <param name="mensajeDefault"></param>
        /// <param name="codigoError"></param>
        /// <returns></returns>
        private static ErrorDto<string> ConstruirRespuestaStringDesdeSp(
            CrdGestorExternoSpResponse? result,
            string mensajeNull,
            string mensajeDefault,
            int codigoError)
        {
            if (result is null)
            {
                return CrearErrorString(mensajeNull, -1);
            }

            if (result.Pass == 1)
            {
                return DbHelper.CreateOkResponse(string.Empty);
            }

            return CrearErrorString(
                string.IsNullOrWhiteSpace(result.Mensaje) ? mensajeDefault : result.Mensaje,
                codigoError);
        }

     
        /// <summary>
        /// Registro en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="result"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, CrdGestorExternoSpResponse result)
        {
            Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"{result.Movimiento},{result.Mensaje}",
                Movimiento = $"{result.Movimiento}-WEB" ,
                Modulo = ModuloCobros
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static CrdGestorExternoCargaMasivaResponse CrearRespuestaInicial(
            CrdGestorExternoCargaMasivaRequest request)
        {
            return new CrdGestorExternoCargaMasivaResponse
            {
                TotalRecibidos = request.Registros.Count
            };
        }

        /// <summary>
        /// Validacion de carga de archivo
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        private static ErrorDto<CrdGestorExternoCargaMasivaResponse>? ValidarCargaMasiva(
            CrdGestorExternoCargaMasivaRequest request,
            CrdGestorExternoCargaMasivaResponse response)
        {
            if (request.Registros.Count == 0)
            {
                return DbHelper.CreateErrorResponse<CrdGestorExternoCargaMasivaResponse>(
                    "No se recibieron registros para procesar.",
                    -1,
                    response);
            }

            if (string.IsNullOrWhiteSpace(request.GestionUsuario) )
            {
                return DbHelper.CreateErrorResponse<CrdGestorExternoCargaMasivaResponse>(
                    "Debe indicar un gestor externo para la asignación masiva.",
                    -1,
                    response);
            }

            return null;
        }

        /// <summary>
        /// Validacion de datos carga de archivo
        /// </summary>
        /// <param name="registro"></param>
        /// <returns></returns>
        private static string? ValidarRegistroCargaMasiva(CrdGestorExternoCargaFilaRequest registro)
        {
            if (registro.Operacion <= 0)
            {
                return "Se omitió un registro por operación inválida.";
            }

            if (string.IsNullOrWhiteSpace(registro.Notas))
            {
                return $"La operación {registro.Operacion} fue omitida porque no tiene notas.";
            }

            return null;
        }

        /// <summary>
        /// Procesa cada registro de la carga masiva, realizando validaciones y contabilizando resultados para la respuesta final
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="request"></param>
        /// <param name="registro"></param>
        /// <param name="response"></param>
        private static void ProcesarRegistroCargaMasiva(
            IDbConnection connection,
            CrdGestorExternoCargaMasivaRequest request,
            CrdGestorExternoCargaFilaRequest registro,
            CrdGestorExternoCargaMasivaResponse response)
        {
            var mensajeValidacion = ValidarRegistroCargaMasiva(registro);

            if (!string.IsNullOrEmpty(mensajeValidacion))
            {
                response.TotalConError++;
                response.Mensajes.Add(mensajeValidacion);
                return;
            }

            var esAsignacion = EsAsignacion(request);
            var query = esAsignacion ? SpGestorExternoAdd : SpGestorExternoDelMasivo;
            var parameters = esAsignacion
                ? CrearParametrosAsignacion(request, registro)
                : CrearParametrosDesvinculacion(request, registro);
            var mensajeDefault = esAsignacion
                ? "No fue posible asignar."
                : "No fue posible desvincular.";

            var result = EjecutarSp(connection, query, parameters);
            ProcesarResultadoSp(result, registro.Operacion, response, mensajeDefault);
        }

        /// <summary>
        /// Valida si es asignacion o desvinculacion en la carga masiva, para determinar el proceso a ejecutar
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static bool EsAsignacion(CrdGestorExternoCargaMasivaRequest request)
        {
            return string.Equals(request.EstadoProceso, "A", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        ///  Realiza el procesamiento del resultado de cada registro en la carga masiva, actualizando los contadores de procesados y con error, y acumulando mensajes para la respuesta final
        /// </summary>
        /// <param name="result"></param>
        /// <param name="operacion"></param>
        /// <param name="response"></param>
        /// <param name="mensajeDefault"></param>
        private static void ProcesarResultadoSp(
            CrdGestorExternoSpResponse? result,
            long operacion,
            CrdGestorExternoCargaMasivaResponse response,
            string mensajeDefault)
        {
            if (result?.Pass == 1)
            {
                response.TotalProcesados++;
                return;
            }

            response.TotalConError++;
            response.Mensajes.Add(
                $"Operación {operacion}: {result?.Mensaje ?? mensajeDefault}");
        }

        /// <summary>
        /// limpia  el texto de entrada, aplicando trim y valores por defecto según corresponda
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private static string LimpiarTexto(string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el estado del registro, aplicando valores por defecto si es necesario
        /// </summary>
        /// <param name="estado"></param>
        /// <returns></returns>
        private static string ObtenerEstado(string? estado)
        {
            return string.IsNullOrWhiteSpace(estado)
                ? "A"
                : estado.Trim()[0].ToString();
        }

        /// <summary>
        ///  Asigna el usuario que gestiona el caso, aplicando limpieza de texto y valores por defecto según corresponda. Si el valor recibido es "TODOS" (ignorando mayúsculas), se interpreta como sin filtro y se devuelve cadena vacía.
        /// </summary>
        /// <param name="gestiona"></param>
        /// <returns></returns>
        private static string ObtenerGestiona(string? gestiona)
        {
            return string.IsNullOrWhiteSpace(gestiona) ||
                   gestiona.Equals("TODOS", StringComparison.OrdinalIgnoreCase)
                ? string.Empty
                : gestiona.Trim();
        }
    }
}
