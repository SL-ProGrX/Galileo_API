using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    public class FrmCrConveniosRebajosDb
    {
        private const int CodigoMensajeControlado = -2;
        private const int VModulo = 16;

        private const string AccionAgregar = "A";
        private const string AccionEditar = "E";
        private const string AccionBorrar = "B";

        private const string MensajeConvenioRequerido =
            "El convenio es requerido.";

        private const string MensajeTransaccionRequerida =
            "El número de transacción es requerido.";

        private const string MensajeRegistroNoExiste =
            "No se encontró el rebajo programado indicado.";

        private const string MensajeRecaudacionAplicada =
            "No se puede eliminar el cargo porque ya tiene "
            + "recaudación aplicada.";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _bitacora;
        private readonly FrmCrConveniosConsultasDb _consultasDb;

        public FrmCrConveniosRebajosDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _bitacora = new MSecurityMainDb(config);
            _consultasDb =
                new FrmCrConveniosConsultasDb(_portalDb);
        }

        /// <summary>
        /// Obtiene las fechas iniciales utilizadas por el formulario.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CrConveniosRebajosInicialData?>
            CR_Convenios_Rebajos_Inicializar(
                int codEmpresa)
        {
            const string sql = """
                SELECT
                    CONVERT(date, Getdate())
                        AS fecha_servidor,
                    CONVERT(date, Getdate())
                        AS cobro_inicio_min,
                    CONVERT(
                        date,
                        DATEADD(day, 45, Getdate()))
                        AS cobro_inicio_max;
                """;

            return DbHelper.ExecuteSingleQuery
                <CrConveniosRebajosInicialData>(
                    _portalDb,
                    codEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene los convenios disponibles para la búsqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConvenioConsultaData>>
            CR_Convenios_Rebajos_Convenios_Obtener(
                int codEmpresa)
        {
            return _consultasDb
                .CrConveniosConsultas_Convenios_Obtener(
                    codEmpresa);
        }

        /// <summary>
        /// Consulta un convenio por código o descripción exacta.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConvenio"></param>
        /// <param name="descripcion"></param>
        /// <returns></returns>
        public ErrorDto<CrConvenioConsultaData?>
            CR_Convenios_Rebajos_Convenio_Obtener(
                int codEmpresa,
                string? codConvenio,
                string? descripcion)
        {
            return _consultasDb
                .CrConveniosConsultas_Convenio_Obtener(
                    codEmpresa,
                    codConvenio,
                    descripcion);
        }

        /// <summary>
        /// Obtiene el convenio anterior o siguiente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConvenio"></param>
        /// <param name="siguiente"></param>
        /// <returns></returns>
        public ErrorDto<CrConvenioConsultaData?>
            CR_Convenios_Rebajos_Convenio_Navegar(
                int codEmpresa,
                string? codConvenio,
                bool siguiente)
        {
            const string sql = """
                SELECT TOP (1)
                    RTRIM(COD_CONVENIO) AS cod_convenio,
                    RTRIM(ISNULL(DESCRIPCION, ''))
                        AS descripcion
                FROM CRD_CONVENIOS
                WHERE @cod_convenio = ''
                   OR (
                        @siguiente = 1
                        AND COD_CONVENIO > @cod_convenio
                   )
                   OR (
                        @siguiente = 0
                        AND COD_CONVENIO < @cod_convenio
                   )
                ORDER BY
                    CASE
                        WHEN @siguiente = 1
                        THEN COD_CONVENIO
                    END ASC,
                    CASE
                        WHEN @siguiente = 0
                        THEN COD_CONVENIO
                    END DESC;
                """;

            return DbHelper.ExecuteSingleQuery
                <CrConvenioConsultaData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    null,
                    new
                    {
                        cod_convenio =
                            NormalizarTexto(codConvenio),
                        siguiente = siguiente ? 1 : 0
                    });
        }

        /// <summary>
        /// Obtiene los cargos activos configurados para el convenio.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConvenio"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConveniosRebajosCargoData>>
            CR_Convenios_Rebajos_Cargos_Obtener(
                int codEmpresa,
                string? codConvenio)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeConvenioRequerido,
                    CodigoMensajeControlado,
                    new List<CrConveniosRebajosCargoData>());
            }

            const string sql = """
                SELECT
                    RTRIM(Ca.COD_CARGO) AS cod_cargo,
                    RTRIM(ISNULL(Ca.DESCRIPCION, ''))
                        AS descripcion
                FROM CRD_CONVENIOS_CARGOS_CXP Cc
                INNER JOIN CXP_CARGOS Ca
                    ON Cc.COD_CARGO = Ca.COD_CARGO
                WHERE Cc.COD_CONVENIO = @cod_convenio
                  AND Ca.ACTIVO = 1
                ORDER BY Ca.DESCRIPCION;
                """;

            return DbHelper.ExecuteListQuery
                <CrConveniosRebajosCargoData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        cod_convenio =
                            codConvenio.Trim()
                    });
        }

        /// <summary>
        /// Obtiene las transacciones disponibles para la búsqueda.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConvenio"></param>
        /// <returns></returns>
        public ErrorDto
            <List<CrConveniosRebajosTransaccionBusquedaData>>
            CR_Convenios_Rebajos_Transacciones_Obtener(
                int codEmpresa,
                string? codConvenio)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeConvenioRequerido,
                    CodigoMensajeControlado,
                    new List
                        <CrConveniosRebajosTransaccionBusquedaData>());
            }

            const string sql = """
                SELECT
                    ID_TRANSAC AS id_transac,
                    RTRIM(ISNULL(DOCUMENTO, ''))
                        AS documento,
                    RTRIM(ISNULL(DETALLE, ''))
                        AS detalle,
                    ISNULL(MONTO, 0) AS monto,
                    REGISTRO_FECHA AS registro_fecha
                FROM CRD_CONVENIOS_CARGOS_CXP_CONTROL
                WHERE COD_CONVENIO = @cod_convenio
                ORDER BY ID_TRANSAC DESC;
                """;

            return DbHelper.ExecuteListQuery
                <CrConveniosRebajosTransaccionBusquedaData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        cod_convenio =
                            codConvenio.Trim()
                    });
        }

        /// <summary>
        /// Consulta un rebajo programado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConvenio"></param>
        /// <param name="idTransac"></param>
        /// <returns></returns>
        public ErrorDto<CrConveniosRebajosData?>
            CR_Convenios_Rebajos_Transaccion_Obtener(
                int codEmpresa,
                string? codConvenio,
                int idTransac)
        {
            var validation =
                CR_Convenios_Rebajos_Consulta_Validar(
                    codConvenio,
                    idTransac);

            if (validation != null)
            {
                return validation;
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    CR_Convenios_Rebajos_Transaccion_Consultar(
                        connection,
                        codConvenio!.Trim(),
                        idTransac));
        }

        /// <summary>
        /// Obtiene el rebajo programado anterior o siguiente.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConvenio"></param>
        /// <param name="idTransac"></param>
        /// <param name="siguiente"></param>
        /// <returns></returns>
        public ErrorDto<CrConveniosRebajosData?>
            CR_Convenios_Rebajos_Transaccion_Navegar(
                int codEmpresa,
                string? codConvenio,
                int idTransac,
                bool siguiente)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        MensajeConvenioRequerido,
                        CodigoMensajeControlado);
            }

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    CR_Convenios_Rebajos_Transaccion_Navegar_Consultar(
                        connection,
                        codConvenio.Trim(),
                        idTransac,
                        siguiente));
        }

        /// <summary>
        /// Registra o modifica un rebajo programado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConveniosRebajosData?>
            CR_Convenios_Rebajos_Guardar(
                int codEmpresa,
                CrConveniosRebajosGuardarRequest? request)
        {
            var validation =
                CR_Convenios_Rebajos_Guardar_Validar(
                    request);

            if (validation != null)
            {
                return validation;
            }

            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    CR_Convenios_Rebajos_Guardar_Ejecutar(
                        connection,
                        request!));

            if (response.Code != 0)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        response.Description ??
                        "Ocurrió un error al guardar "
                        + "el rebajo programado.");
            }

            var idTransac = response.Result;

            if (idTransac <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        "El proceso no devolvió un número "
                        + "de transacción válido.");
            }

            CR_Convenios_Rebajos_Bitacora_Registrar(
                codEmpresa,
                request!.usuario,
                request.id_transac > 0
                    ? "Modifica - WEB"
                    : "Registra - WEB",
                $"Rebajo Programado: "
                + $"{request.cod_convenio.Trim()} "
                + $"ID: {idTransac}");

            var transactionResponse =
                CR_Convenios_Rebajos_Transaccion_Obtener(
                    codEmpresa,
                    request.cod_convenio,
                    idTransac);

            if (transactionResponse.Code == 0)
            {
                transactionResponse.Description =
                    "Información guardada "
                    + "satisfactoriamente.";
            }

            return transactionResponse;
        }

        /// <summary>
        /// Elimina un rebajo programado que no tenga recaudación.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            CR_Convenios_Rebajos_Eliminar(
                int codEmpresa,
                CrConveniosRebajosEliminarRequest? request)
        {
            var validation =
                CR_Convenios_Rebajos_Eliminar_Validar(
                    request);

            if (validation != null)
            {
                return validation;
            }

            var response = DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    CR_Convenios_Rebajos_Eliminar_Ejecutar(
                        connection,
                        request!));

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurrió un error al eliminar "
                    + "el rebajo programado.");
            }

            if (response.Result == null)
            {
                return DbHelper.ErrorResponse(
                    MensajeRegistroNoExiste,
                    CodigoMensajeControlado);
            }

            if (!response.Result.eliminado)
            {
                return DbHelper.ErrorResponse(
                    MensajeRecaudacionAplicada,
                    CodigoMensajeControlado);
            }

            CR_Convenios_Rebajos_Bitacora_Registrar(
                codEmpresa,
                request!.usuario,
                "Elimina - WEB",
                $"Rebajo Programado: "
                + $"{request.cod_convenio.Trim()} "
                + $"ID: {request.id_transac} "
                + $"Monto: {response.Result.monto}");

            return DbHelper.OkResponse(
                "Información eliminada satisfactoriamente.");
        }

        /// <summary>
        /// Obtiene los cobros del convenio según el estado solicitado.
        /// Estado 1: en cobro, 2: cancelados, 3: todos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConvenio"></param>
        /// <param name="estado"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConveniosRebajosCobroData>>
            CR_Convenios_Rebajos_Cobros_Obtener(
                int codEmpresa,
                string? codConvenio,
                int estado)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeConvenioRequerido,
                    CodigoMensajeControlado,
                    new List<CrConveniosRebajosCobroData>());
            }

            const string sql = """
                SELECT
                    C.ID_TRANSAC AS id_transac,
                    RTRIM(ISNULL(C.COD_CARGO, ''))
                        AS cod_cargo,
                    RTRIM(ISNULL(D.DESCRIPCION, ''))
                        AS cargo_desc,
                    RTRIM(ISNULL(C.DOCUMENTO, ''))
                        AS documento,
                    RTRIM(ISNULL(C.DETALLE, ''))
                        AS detalle,
                    ISNULL(C.MONTO, 0) AS monto,
                    ISNULL(C.SALDO, 0) AS saldo,
                    ISNULL(C.MONTO, 0)
                        - ISNULL(C.SALDO, 0)
                        AS recaudado,
                    C.COBRO_INICIO_FECHA
                        AS cobro_inicio_fecha,
                    ISNULL(C.COBRO_FRACCIONES, 0)
                        AS cobro_fracciones,
                    ISNULL(C.ACTIVO, 0) AS activo,
                    RTRIM(ISNULL(C.NOTAS, ''))
                        AS notas
                FROM CRD_CONVENIOS_CARGOS_CXP_CONTROL C
                INNER JOIN CXP_CARGOS D
                    ON C.COD_CARGO = D.COD_CARGO
                WHERE C.COD_CONVENIO = @cod_convenio
                  AND (
                        @estado = 3
                        OR (
                            @estado = 1
                            AND ISNULL(C.SALDO, 0) > 0
                        )
                        OR (
                            @estado = 2
                            AND ISNULL(C.SALDO, 0) <= 0
                        )
                  )
                ORDER BY C.ID_TRANSAC DESC;
                """;

            return DbHelper.ExecuteListQuery
                <CrConveniosRebajosCobroData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        cod_convenio =
                            codConvenio.Trim(),
                        estado =
                            CR_Convenios_Rebajos_Estado_Normalizar(
                                estado)
                    });
        }

        /// <summary>
        /// Obtiene los pagos aplicados a un rebajo programado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codConvenio"></param>
        /// <param name="idTransac"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConveniosRebajosPagoData>>
            CR_Convenios_Rebajos_Pagos_Obtener(
                int codEmpresa,
                string? codConvenio,
                int idTransac)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeConvenioRequerido,
                    CodigoMensajeControlado,
                    new List<CrConveniosRebajosPagoData>());
            }

            if (idTransac <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeTransaccionRequerida,
                    CodigoMensajeControlado,
                    new List<CrConveniosRebajosPagoData>());
            }

            const string sql = """
                SELECT
                    P.ID_FRACCION AS id_fraccion,
                    RTRIM(ISNULL(O.COD_FACTURA, ''))
                        AS cod_factura,
                    O.FECHA_CORTE AS fecha_corte,
                    ISNULL(P.MONTO, 0) AS monto,
                    RTRIM(ISNULL(O.NOTAS, '')) AS notas
                FROM CRD_CONVENIOS_ORDENES O
                INNER JOIN CRD_CONVENIOS_DT_CARGOS_CXP P
                    ON O.COD_CONVENIO = P.COD_CONVENIO
                   AND O.COD_ORDEN = P.COD_ORDEN
                WHERE P.COD_CONVENIO = @cod_convenio
                  AND P.ID_TRANSAC = @id_transac
                  AND O.ESTADO = 'C'
                ORDER BY O.FECHA_CORTE DESC,
                         P.ID_FRACCION;
                """;

            return DbHelper.ExecuteListQuery
                <CrConveniosRebajosPagoData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        cod_convenio =
                            codConvenio.Trim(),
                        id_transac = idTransac
                    });
        }

        /// <summary>
        /// Consulta un rebajo programado mediante el procedimiento
        /// utilizado por el formulario VB6.
        /// </summary>
        private static CrConveniosRebajosData?
            CR_Convenios_Rebajos_Transaccion_Consultar(
                SqlConnection connection,
                string codConvenio,
                int idTransac)
        {
            const string sql = """
                EXEC spConvenios_Rebajos_Programa_Consulta
                    @cod_convenio,
                    @id_transac;
                """;

            return connection.QueryFirstOrDefault
                <CrConveniosRebajosData>(
                    sql,
                    new
                    {
                        cod_convenio = codConvenio,
                        id_transac = idTransac
                    });
        }

        /// <summary>
        /// Obtiene y carga la transacción anterior o siguiente.
        /// </summary>
        private static CrConveniosRebajosData?
            CR_Convenios_Rebajos_Transaccion_Navegar_Consultar(
                SqlConnection connection,
                string codConvenio,
                int idTransac,
                bool siguiente)
        {
            const string sql = """
                SELECT TOP (1)
                    ID_TRANSAC
                FROM CRD_CONVENIOS_CARGOS_CXP_CONTROL
                WHERE COD_CONVENIO = @cod_convenio
                  AND (
                        (
                            @siguiente = 1
                            AND ID_TRANSAC > @id_transac
                        )
                        OR (
                            @siguiente = 0
                            AND ID_TRANSAC < @id_transac
                        )
                  )
                ORDER BY
                    CASE
                        WHEN @siguiente = 1
                        THEN ID_TRANSAC
                    END ASC,
                    CASE
                        WHEN @siguiente = 0
                        THEN ID_TRANSAC
                    END DESC;
                """;

            var siguienteId =
                connection.QueryFirstOrDefault<int?>(
                    sql,
                    new
                    {
                        cod_convenio = codConvenio,
                        id_transac = idTransac,
                        siguiente = siguiente ? 1 : 0
                    });

            if (siguienteId is null or <= 0)
            {
                return null;
            }

            return CR_Convenios_Rebajos_Transaccion_Consultar(
                connection,
                codConvenio,
                siguienteId.Value);
        }

        /// <summary>
        /// Ejecuta el registro o modificación del rebajo programado.
        /// </summary>
        private static int
            CR_Convenios_Rebajos_Guardar_Ejecutar(
                SqlConnection connection,
                CrConveniosRebajosGuardarRequest request)
        {
            var accion =
                request.id_transac > 0
                    ? AccionEditar
                    : AccionAgregar;

            var parametros =
                CR_Convenios_Rebajos_Parametros_Crear(
                    request,
                    accion);

            if (accion == AccionAgregar)
            {
                var result = connection.QueryFirstOrDefault
                    <CrConveniosRebajosGuardarResult>(
                        CR_Convenios_Rebajos_ProcedimientoSql,
                        parametros);

                return result?.transaccion_id ?? 0;
            }

            connection.Execute(
                CR_Convenios_Rebajos_ProcedimientoSql,
                parametros);

            return request.id_transac;
        }

        /// <summary>
        /// Ejecuta la eliminación después de validar la recaudación.
        /// </summary>
        private static CrConveniosRebajosEliminarResult?
            CR_Convenios_Rebajos_Eliminar_Ejecutar(
                SqlConnection connection,
                CrConveniosRebajosEliminarRequest request)
        {
            var registro =
                CR_Convenios_Rebajos_Transaccion_Consultar(
                    connection,
                    request.cod_convenio.Trim(),
                    request.id_transac);

            if (registro == null)
            {
                return null;
            }

            if (registro.recaudado > 0)
            {
                return new CrConveniosRebajosEliminarResult
                {
                    eliminado = false,
                    monto = registro.monto
                };
            }

            var guardarRequest =
                new CrConveniosRebajosGuardarRequest
                {
                    cod_convenio =
                        registro.cod_convenio,
                    id_transac =
                        registro.id_transac,
                    cobro_fracciones = 0,
                    cod_cargo =
                        registro.cod_cargo,
                    monto =
                        registro.monto,
                    cobro_inicio_fecha =
                        registro.cobro_inicio_fecha,
                    documento =
                        registro.documento,
                    detalle =
                        registro.detalle,
                    notas =
                        registro.notas,
                    usuario =
                        request.usuario,
                    activo =
                        registro.activo
                };

            connection.Execute(
                CR_Convenios_Rebajos_ProcedimientoSql,
                CR_Convenios_Rebajos_Parametros_Crear(
                    guardarRequest,
                    AccionBorrar));

            return new CrConveniosRebajosEliminarResult
            {
                eliminado = true,
                monto = registro.monto
            };
        }

        /// <summary>
        /// Construye los parámetros del procedimiento de mantenimiento.
        /// </summary>
        private static object
            CR_Convenios_Rebajos_Parametros_Crear(
                CrConveniosRebajosGuardarRequest request,
                string accion)
        {
            return new
            {
                cod_convenio =
                    request.cod_convenio.Trim(),
                id_transac =
                    request.id_transac,
                cobro_fracciones =
                    request.cobro_fracciones,
                cod_cargo =
                    request.cod_cargo.Trim(),
                monto =
                    request.monto,
                cobro_inicio_fecha =
                    request.cobro_inicio_fecha!.Value.Date,
                documento =
                    request.documento.Trim(),
                detalle =
                    NormalizarTexto(request.detalle),
                notas =
                    NormalizarTexto(request.notas),
                usuario =
                    request.usuario.Trim(),
                activo =
                    request.activo == 0 ? 0 : 1,
                accion
            };
        }

        /// <summary>
        /// Valida los datos requeridos para consultar una transacción.
        /// </summary>
        private static ErrorDto<CrConveniosRebajosData?>?
            CR_Convenios_Rebajos_Consulta_Validar(
                string? codConvenio,
                int idTransac)
        {
            if (string.IsNullOrWhiteSpace(codConvenio))
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        MensajeConvenioRequerido,
                        CodigoMensajeControlado);
            }

            if (idTransac <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        MensajeTransaccionRequerida,
                        CodigoMensajeControlado);
            }

            return null;
        }

        /// <summary>
        /// Valida la información requerida para guardar.
        /// </summary>
        private static ErrorDto<CrConveniosRebajosData?>?
            CR_Convenios_Rebajos_Guardar_Validar(
                CrConveniosRebajosGuardarRequest? request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        "La información del rebajo es requerida.",
                        CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(
                    request.cod_convenio))
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        MensajeConvenioRequerido,
                        CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(
                    request.cod_cargo))
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        "El cargo es requerido.",
                        CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(
                    request.documento))
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        "El documento no es válido.",
                        CodigoMensajeControlado);
            }

            if (request.monto <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        "El monto no es válido.",
                        CodigoMensajeControlado);
            }

            if (request.cobro_fracciones <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        "El número de pagos debe ser mayor que cero.",
                        CodigoMensajeControlado);
            }

            if (request.cobro_inicio_fecha == null)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        "La fecha de inicio del cobro es requerida.",
                        CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosRebajosData?>(
                        "El usuario es requerido.",
                        CodigoMensajeControlado);
            }

            return null;
        }

        /// <summary>
        /// Valida la información requerida para eliminar.
        /// </summary>
        private static ErrorDto?
            CR_Convenios_Rebajos_Eliminar_Validar(
                CrConveniosRebajosEliminarRequest? request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(
                    "La información del rebajo es requerida.",
                    CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(
                    request.cod_convenio))
            {
                return DbHelper.ErrorResponse(
                    MensajeConvenioRequerido,
                    CodigoMensajeControlado);
            }

            if (request.id_transac <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeTransaccionRequerida,
                    CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoMensajeControlado);
            }

            return null;
        }

        /// <summary>
        /// Registra el movimiento equivalente a Bitacora del VB6.
        /// </summary>
        private void
            CR_Convenios_Rebajos_Bitacora_Registrar(
                int codEmpresa,
                string usuario,
                string movimiento,
                string detalle)
        {
            _bitacora.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario.Trim(),
                    DetalleMovimiento = detalle,
                    Movimiento = movimiento,
                    Modulo = VModulo
                });
        }

        /// <summary>
        /// Normaliza el estado del filtro de cobros.
        /// </summary>
        private static int
            CR_Convenios_Rebajos_Estado_Normalizar(
                int estado)
        {
            return estado is >= 1 and <= 3
                ? estado
                : 3;
        }

        /// <summary>
        /// Normaliza textos provenientes del request.
        /// </summary>
        private static string NormalizarTexto(
            string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        private const string
            CR_Convenios_Rebajos_ProcedimientoSql = """
                EXEC spConvenios_Rebajos_Programa
                    @cod_convenio,
                    @id_transac,
                    @cobro_fracciones,
                    @cod_cargo,
                    @monto,
                    @cobro_inicio_fecha,
                    @documento,
                    @detalle,
                    @notas,
                    @usuario,
                    @activo,
                    @accion;
                """;

        private sealed class CrConveniosRebajosEliminarResult
        {
            public bool eliminado { get; init; }
            public decimal monto { get; init; }
        }
    }
}