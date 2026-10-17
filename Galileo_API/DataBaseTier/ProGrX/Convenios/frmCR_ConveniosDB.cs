using System.Data;
using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Convenios;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    /// <summary>
    /// Gestiona el acceso a datos del registro de convenios.
    /// </summary>
    public class FrmCRConveniosDB
    {
        private const int VModulo = 16;
        private const string MensajeNoMasConvenios = "No existen más convenios para navegar.";
        private const string MensajeNoMasRegistros = "No existen más registros para navegar.";

        private const string ConvenioDetalleSelectSql = @"
            SELECT
                RTRIM(C.COD_CONVENIO) AS cod_convenio,
                ISNULL(C.DESCRIPCION, '') AS descripcion,
                RTRIM(C.TIPO_CONVENIO) AS tipo_convenio,
                ISNULL(CT.DESCRIPCION, '') AS tipo_descripcion,
                ISNULL(RTRIM(C.CEDULA), '') AS cedula,
                ISNULL(S.NOMBRE, '') AS cliente_nombre,
                C.COD_PROVEEDOR AS cod_proveedor,
                ISNULL(P.DESCRIPCION, '') AS proveedor_nombre,
                ISNULL(C.CONTRATO_NUMERO, '') AS contrato_numero,
                C.FECHA_INICIO AS fecha_inicio,
                C.FECHA_VENCIMIENTO AS fecha_vencimiento,
                CAST(ISNULL(C.ACTIVO, 0) AS bit) AS activo,
                ISNULL(C.PORC_COMISION_CREDITOS, 0) AS porc_comision_creditos,
                ISNULL(C.PORC_COMISION_RECAUDACION, 0) AS porc_comision_recaudacion,
                ISNULL(C.RESERVAS_PORC, 0) AS reservas_porc,
                ISNULL(C.RESERVAS_RETENIDAS, 0) AS reservas_retenidas,
                ISNULL(C.RESERVAS_EJECUTADO, 0) AS reservas_ejecutado,
                ISNULL(C.RESERVAS_SALDO, 0) AS reservas_saldo,
                C.RESERVAS_CORTE AS reservas_corte,
                ISNULL(C.RESERVA_TOPE, 0) AS reserva_tope,
                C.ULTIMO_CORTE AS ultimo_corte,
                CAST(ISNULL(C.COMISION_INFORMATIVA, 0) AS bit) AS comision_informativa,
                CAST(ISNULL(C.COBRA_CREDITOS_ANULADOS, 0) AS bit) AS cobra_creditos_anulados
            FROM CRD_CONVENIOS C
            LEFT JOIN CRD_CONVENIO_TIPO CT ON CT.TIPO_CONVENIO = C.TIPO_CONVENIO
            LEFT JOIN SOCIOS S ON S.CEDULA = C.CEDULA
            LEFT JOIN CXP_PROVEEDORES P ON P.COD_PROVEEDOR = C.COD_PROVEEDOR";

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MFndXtaDB _mFndXta;
        private readonly MProGrxMain _mProGrxMain;
        /// <summary>
        /// Gestiona el acceso a datos del registro de convenios.
        /// </summary>
        public FrmCRConveniosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mFndXta = new MFndXtaDB(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        #region Helpers privados

        private sealed class BusquedaF4
        {
            public string? texto { get; init; }
            public string? like { get; init; }
            public int offset { get; init; }
            public int filas { get; init; }
            public bool usar_paginacion { get; init; }
        }

        private static string NormalizarCodigo(string? valor) => (valor ?? string.Empty).Trim().ToUpperInvariant();

        private static string NormalizarUsuario(string? usuario) => (usuario ?? string.Empty).Trim().ToUpperInvariant();

        private ErrorDto<T> Ejecutar<T>(int codEmpresa, Func<SqlConnection, ErrorDto<T>> accion)
        {
            ErrorDto<ErrorDto<T>> respuesta = DbHelper.WithConn(_portalDB, codEmpresa, accion);

            if (respuesta.Code != 0)
            {
                return DbHelper.CreateErrorResponse<T>(
                    respuesta.Description ?? "No fue posible ejecutar la operación en la base de datos.",
                    respuesta.Code ?? -1);
            }

            return respuesta.Result
                ?? DbHelper.CreateErrorResponse<T>("No se obtuvo una respuesta de la base de datos.");
        }

        private ErrorDto Ejecutar(int codEmpresa, Func<SqlConnection, ErrorDto> accion)
        {
            ErrorDto<ErrorDto> respuesta = DbHelper.WithConn(_portalDB, codEmpresa, accion);

            if (respuesta.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    respuesta.Description ?? "No fue posible ejecutar la operación en la base de datos.",
                    respuesta.Code ?? -1);
            }

            return respuesta.Result
                ?? DbHelper.ErrorResponse("No se obtuvo una respuesta de la base de datos.");
        }

        private static BusquedaF4 CrearBusqueda(FiltrosLazyLoadData? filtros, string? texto)
        {
            string valor = (texto ?? filtros?.filtro ?? string.Empty).Trim();
            int filas = filtros?.paginacion ?? 0;
            int pagina = filtros?.pagina ?? 0;
            bool usarPaginacion = filas > 0;

            return new BusquedaF4
            {
                texto = string.IsNullOrWhiteSpace(valor) ? null : valor,
                like = string.IsNullOrWhiteSpace(valor) ? null : $"%{valor}%",
                offset = usarPaginacion ? Math.Max(0, pagina) * filas : 0,
                filas = Math.Max(0, filas),
                usar_paginacion = usarPaginacion
            };
        }

        private static ErrorDto<TablasListaGenericaModel> CrearResultadoLista<T>(int total, List<T> lista)
        {
            return DbHelper.CreateOkResponse(new TablasListaGenericaModel
            {
                total = total,
                lista = lista
            });
        }

        private ErrorDto<TablasListaGenericaModel> ObtenerF4<T>(int codEmpresa, FiltrosLazyLoadData? filtros, string? texto, string sqlTotal, string sqlLista)
        {
            return Ejecutar(codEmpresa, connection =>
            {
                BusquedaF4 busqueda = CrearBusqueda(filtros, texto);
                var parametros = new { Texto = busqueda.texto, Like = busqueda.like, Offset = busqueda.offset, Filas = busqueda.filas };
                int total = connection.QueryFirstOrDefault<int>(sqlTotal, parametros);
                string consulta = busqueda.usar_paginacion
                    ? sqlLista + " OFFSET @Offset ROWS FETCH NEXT @Filas ROWS ONLY;"
                    : sqlLista + ";";
                List<T> lista = connection.Query<T>(consulta, parametros).ToList();
                return CrearResultadoLista(total, lista);
            });
        }

        private static string ObtenerCodigoScroll(SqlConnection connection, int direccion, string codigo, string sqlSiguiente, string sqlAnterior)
        {
            string sql = direccion == 1 ? sqlSiguiente : sqlAnterior;
            return connection.QueryFirstOrDefault<string>(sql, new { Codigo = codigo }) ?? string.Empty;
        }

        private static bool ExisteConvenio(SqlConnection connection, string codConvenio)
        {
            const string sql = "SELECT TOP 1 1 FROM CRD_CONVENIOS WHERE COD_CONVENIO = @CodConvenio;";
            return connection.QueryFirstOrDefault<int?>(sql, new { CodConvenio = codConvenio }).HasValue;
        }

        private static bool ExisteRegistro(SqlConnection connection, string sql, object parametros)
        {
            return connection.QueryFirstOrDefault<int?>(sql, parametros).HasValue;
        }

        private void RegistrarBitacora(int codEmpresa, string? usuario, string movimiento, string detalle)
        {
            try
            {
                _securityMainDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = NormalizarUsuario(usuario),
                    Movimiento = movimiento,
                    DetalleMovimiento = detalle,
                    Modulo = VModulo
                });
            }
            catch
            {
            }
        }

        private static string ObtenerMensajeValidacionConvenio(CrConvenioDto? convenio)
        {
            if (convenio == null)
                return "Debe indicar la información del convenio.";

            string codigo = NormalizarCodigo(convenio.cod_convenio);
            string descripcion = convenio.descripcion?.Trim() ?? string.Empty;
            string cedula = NormalizarCodigo(convenio.cedula);
            string tipo = NormalizarCodigo(convenio.tipo_convenio);
            string contrato = convenio.contrato_numero?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(codigo))
                return "No se ha indicado un código único al convenio.";

            if (string.IsNullOrWhiteSpace(descripcion))
                return "No se especificó el nombre del convenio.";

            if (string.IsNullOrWhiteSpace(cedula))
                return "Debe indicar un cliente, verifique.";

            if (!convenio.cod_proveedor.HasValue || convenio.cod_proveedor.Value <= 0)
                return "Debe indicar un proveedor, verifique.";

            if (string.IsNullOrWhiteSpace(tipo))
                return "Debe indicar un tipo de convenio, verifique.";

            if (string.IsNullOrWhiteSpace(contrato))
                return "Debe indicar un número de contrato para el convenio, verifique.";

            if (codigo.Length > 10)
                return "El código del convenio no puede superar los 10 caracteres.";

            if (tipo.Length > 10)
                return "El tipo de convenio no puede superar los 10 caracteres.";

            if (descripcion.Length > 100)
                return "La descripción del convenio no puede superar los 100 caracteres.";
            
            if (cedula.Length > 20)
                return "La identificación del cliente no puede superar los 20 caracteres.";

            if (contrato.Length > 20)
                return "El número de contrato no puede superar los 20 caracteres.";

            return string.Empty;
        }

        private static CrConvenioDto? ObtenerConvenio(SqlConnection connection, string codConvenio)
        {
            string sql = ConvenioDetalleSelectSql + " WHERE C.COD_CONVENIO = @CodConvenio;";
            return connection.QueryFirstOrDefault<CrConvenioDto>(sql, new { CodConvenio = codConvenio });
        }
        private static bool ExisteConceptoDevolucion(SqlConnection connection, string codigo)
        {
            const string sql = "SELECT TOP 1 1 FROM FND_RETENCION_CONCEPTOS WHERE RETENCION_CODIGO = @Codigo AND ACTIVO = 1;";
            return ExisteRegistro(connection, sql, new { Codigo = codigo });
        }

        private static bool ExisteCargo(SqlConnection connection, string codigo)
        {
            const string sql = "SELECT TOP 1 1 FROM CARGOS_ADICIONALES WHERE COD_CARGO = @Codigo;";
            return ExisteRegistro(connection, sql, new { Codigo = codigo });
        }

        private static bool ExisteCargoCxp(SqlConnection connection, string codigo)
        {
            const string sql = "SELECT TOP 1 1 FROM CXP_CARGOS WHERE COD_CARGO = @Codigo AND ACTIVO = 1;";
            return ExisteRegistro(connection, sql, new { Codigo = codigo });
        }

        private static bool ExisteLineaCredito(SqlConnection connection, string codigo)
        {
            const string sql = @"
                SELECT TOP 1 1 FROM CATALOGO
                WHERE CODIGO = @Codigo AND RETENCION = 'N' AND ACTIVO = 1 AND LINEA_INTERNA = 1;";
            return ExisteRegistro(connection, sql, new { Codigo = codigo });
        }

        private static bool ExisteDestinoCredito(SqlConnection connection, string codigo)
        {
            const string sql = "SELECT TOP 1 1 FROM CATALOGO_DESTINOS WHERE COD_DESTINO = @Codigo;";
            return ExisteRegistro(connection, sql, new { Codigo = codigo });
        }

        #endregion

        #region Base y General

        /// <summary>
        /// Obtiene la fecha de proceso de la empresa para inicializar el registro de convenios.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<CrConveniosInicialData> CR_Convenios_Inicial_Obtener(int CodEmpresa)
        {
            return DbHelper.CreateOkResponse(new CrConveniosInicialData
            {
                fecha_servidor = _mProGrxMain.fxFechaServidor(CodEmpresa, 0)
            });
        }

        /// <summary>
        /// Obtiene los convenios disponibles para la búsqueda F4 respetando el filtro y paginación recibidos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            const string total = @"
                SELECT COUNT(1) FROM CRD_CONVENIOS
                WHERE @Texto IS NULL OR COD_CONVENIO LIKE @Like OR DESCRIPCION LIKE @Like;";
            const string lista = @"
                SELECT RTRIM(COD_CONVENIO) AS cod_convenio, ISNULL(DESCRIPCION, '') AS descripcion
                FROM CRD_CONVENIOS
                WHERE @Texto IS NULL OR COD_CONVENIO LIKE @Like OR DESCRIPCION LIKE @Like
                ORDER BY COD_CONVENIO";
            return ObtenerF4<CrConveniosF4Data>(CodEmpresa, request?.filtros, request?.texto, total, lista);
        }

        /// <summary>
        /// Obtiene toda la información general de un convenio registrado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConvenioDto> CR_Convenio_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            string codConvenio = NormalizarCodigo(request?.cod_convenio);
            if (string.IsNullOrWhiteSpace(codConvenio))
                return DbHelper.CreateErrorResponse<CrConvenioDto>("Debe indicar el código del convenio.", -2);

            return Ejecutar(CodEmpresa, connection =>
            {
                CrConvenioDto? convenio = ObtenerConvenio(connection, codConvenio);
                return convenio == null
                    ? DbHelper.CreateErrorResponse<CrConvenioDto>("Convenio no encontrado.", -2)
                    : DbHelper.CreateOkResponse(convenio);
            });
        }

        /// <summary>
        /// Navega al convenio siguiente o anterior y retorna su información general completa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrConvenioDto> CR_Convenios_Scroll_Obtener(int CodEmpresa,CrConveniosScrollRequest? request,string? usuario)
        {
            string codigo = NormalizarCodigo(
                request?.cod_convenio);

            int direccion = request?.direccion ?? 1;

            ErrorDto<string> respuestaCodigo = Ejecutar(
                CodEmpresa,
                connection =>
                {
                    string codigoDestino =
                        ObtenerCodigoConvenioScroll(
                            connection,
                            direccion,
                            codigo);

                    return string.IsNullOrWhiteSpace(codigoDestino)
                        ? DbHelper.CreateErrorResponse<string>(
                            MensajeNoMasConvenios,
                            -2)
                        : DbHelper.CreateOkResponse(codigoDestino);
                });

            if (respuestaCodigo.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CrConvenioDto>(
                    respuestaCodigo.Description ??
                        "No fue posible navegar entre los convenios.",
                    respuestaCodigo.Code ?? -1);
            }

            string codigoDestino =
                respuestaCodigo.Result ?? string.Empty;

            if (string.IsNullOrWhiteSpace(codigoDestino))
            {
                return DbHelper.CreateErrorResponse<CrConvenioDto>(
                    "No fue posible obtener el convenio a consultar.",
                    -1);
            }

            return CR_Convenio_Obtener(
                CodEmpresa,
                new CrConveniosConvenioRequest
                {
                    cod_convenio = codigoDestino
                });
        }

        /// <summary>
        /// Obtiene los tipos de convenio activos para el selector de información general.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Convenios_Tipos_Dropdown_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT RTRIM(TIPO_CONVENIO) AS item, ISNULL(DESCRIPCION, '') AS descripcion
                FROM CRD_CONVENIO_TIPO
                WHERE ACTIVO = 1
                ORDER BY DESCRIPCION;";
            return Ejecutar(CodEmpresa, connection => DbHelper.CreateOkResponse(connection.Query<DropDownListaGenericaModel>(sql).ToList()));
        }

        /// <summary>
        /// Obtiene los clientes disponibles para la búsqueda F4 del enlace con clientes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Clientes_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            const string total = @"
                SELECT COUNT(1) FROM SOCIOS
                WHERE @Texto IS NULL OR CEDULA LIKE @Like OR NOMBRE LIKE @Like;";
            const string lista = @"
                SELECT RTRIM(CEDULA) AS cod_convenio, ISNULL(NOMBRE, '') AS descripcion
                FROM SOCIOS
                WHERE @Texto IS NULL OR CEDULA LIKE @Like OR NOMBRE LIKE @Like
                ORDER BY NOMBRE, CEDULA";
            return ObtenerF4<CrConveniosF4Data>(CodEmpresa, request?.filtros, request?.texto, total, lista);
        }

        /// <summary>
        /// Obtiene los proveedores disponibles para la búsqueda F4 del enlace con proveedores.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Proveedores_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            const string total = @"
                SELECT COUNT(1) FROM CXP_PROVEEDORES
                WHERE @Texto IS NULL OR CAST(COD_PROVEEDOR AS varchar(20)) LIKE @Like OR DESCRIPCION LIKE @Like;";
            const string lista = @"
                SELECT CAST(COD_PROVEEDOR AS varchar(20)) AS cod_convenio, ISNULL(DESCRIPCION, '') AS descripcion
                FROM CXP_PROVEEDORES
                WHERE @Texto IS NULL OR CAST(COD_PROVEEDOR AS varchar(20)) LIKE @Like OR DESCRIPCION LIKE @Like
                ORDER BY DESCRIPCION, COD_PROVEEDOR";
            return ObtenerF4<CrConveniosF4Data>(CodEmpresa, request?.filtros, request?.texto, total, lista);
        }

        /// <summary>
        /// Registra o modifica la información general del convenio y deja la bitácora correspondiente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConvenioDto> CR_Convenios_Guardar(int CodEmpresa, string? usuario, CrConveniosGuardarRequest? request)
        {
            CrConvenioDto? convenio = request?.convenio;

            string validacion = ObtenerMensajeValidacionConvenio(convenio);

            if (!string.IsNullOrWhiteSpace(validacion))
                return DbHelper.CreateErrorResponse<CrConvenioDto>(validacion, -2);

            if (convenio == null)
                return DbHelper.CreateErrorResponse<CrConvenioDto>("Debe indicar la información del convenio.", -2);

            string codigo = NormalizarCodigo(convenio.cod_convenio);
            string tipo = NormalizarCodigo(convenio.tipo_convenio);
            string cedula = NormalizarCodigo(convenio.cedula);
            string descripcion = convenio.descripcion.Trim();
            string contrato = convenio.contrato_numero.Trim();
            string usuarioRegistro = NormalizarUsuario(usuario);

            return Ejecutar(CodEmpresa, connection =>
            {
                bool existe = ExisteConvenio(connection, codigo);

                if (request?.v_edita == true && !existe)
                {
                    return DbHelper.CreateErrorResponse<CrConvenioDto>(
                        "El convenio que desea modificar no existe.",
                        -2);
                }

                if (request?.v_edita != true && existe)
                {
                    return DbHelper.CreateErrorResponse<CrConvenioDto>(
                        "El código del convenio ya se encuentra registrado.",
                        -2);
                }

                var parametros = new
                {
                    CodConvenio = codigo,
                    TipoConvenio = tipo,
                    Descripcion = descripcion,
                    Cedula = cedula,
                    CodProveedor = convenio.cod_proveedor,
                    ContratoNumero = contrato,
                    FechaInicio = convenio.fecha_inicio,
                    FechaVencimiento = convenio.fecha_vencimiento,
                    Activo = convenio.activo,
                    Usuario = usuarioRegistro,
                    PorcComisionCreditos = convenio.porc_comision_creditos,
                    PorcComisionRecaudacion = convenio.porc_comision_recaudacion,
                    ReservasPorc = convenio.reservas_porc,
                    ReservaTope = convenio.reserva_tope,
                    ComisionInformativa = convenio.comision_informativa,
                    CobraCreditosAnulados = convenio.cobra_creditos_anulados
                };

                try
                {
                    string sql = request?.v_edita == true
                        ? CrearSqlActualizaConvenio()
                        : CrearSqlInsertaConvenio();

                    int afectados = connection.Execute(sql, parametros);

                    if (afectados <= 0)
                    {
                        return DbHelper.CreateErrorResponse<CrConvenioDto>(
                            request?.v_edita == true
                                ? "No fue posible actualizar el convenio."
                                : "No fue posible registrar el convenio.",
                            -1);
                    }
                }
                catch (SqlException ex)
                {
                    return CrearErrorSqlGuardarConvenio(ex);
                }

                string movimiento = request?.v_edita == true
                    ? "MODIFICA - WEB"
                    : "REGISTRA - WEB";

                RegistrarBitacora(
                    CodEmpresa,
                    usuario,
                    movimiento,
                    $"Convenio: {codigo}");

                CrConvenioDto? resultado =
                    ObtenerConvenio(connection, codigo);

                return resultado == null
                    ? DbHelper.CreateErrorResponse<CrConvenioDto>(
                        "La información fue procesada, pero no fue posible recuperar el convenio guardado.",
                        -1)
                    : DbHelper.CreateOkResponse(
                        resultado,
                        "Información guardada satisfactoriamente.");
            });
        }
        private static ErrorDto<CrConvenioDto> CrearErrorSqlGuardarConvenio(SqlException ex)
        {
            return ex.Number switch
            {
                2628 or 8152 =>
                    DbHelper.CreateErrorResponse<CrConvenioDto>(
                        "Uno de los valores indicados supera la longitud permitida.",
                        -2),

                2601 or 2627 =>
                    DbHelper.CreateErrorResponse<CrConvenioDto>(
                        "Ya existe un registro con los datos indicados.",
                        -2),

                547 =>
                    DbHelper.CreateErrorResponse<CrConvenioDto>(
                        "Uno de los datos relacionados no existe o no puede ser utilizado.",
                        -2),

                515 =>
                    DbHelper.CreateErrorResponse<CrConvenioDto>(
                        "Existe un dato obligatorio que no fue indicado.",
                        -2),

                _ =>
                    DbHelper.CreateErrorResponse<CrConvenioDto>(
                        "Ocurrió un error al guardar la información del convenio.",
                        -1)
            };
        }
        /// <summary>
        /// Elimina un convenio y registra la acción en la bitácora si la operación fue exitosa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Convenios_Borrar(int CodEmpresa, string? usuario, CrConveniosConvenioRequest? request)
        {
            string codigo = NormalizarCodigo(request?.cod_convenio);
            if (string.IsNullOrWhiteSpace(codigo))
                return DbHelper.ErrorResponse("Debe indicar el código del convenio.", -2);

            return Ejecutar(CodEmpresa, connection =>
            {
                const string sql = "DELETE CRD_CONVENIOS WHERE COD_CONVENIO = @CodConvenio;";
                int afectados = connection.Execute(sql, new { CodConvenio = codigo });
                if (afectados == 0)
                    return DbHelper.ErrorResponse("Convenio no encontrado.", -2);
                RegistrarBitacora(CodEmpresa, usuario, "ELIMINA - WEB", $"Convenio: {codigo}");
                return DbHelper.OkResponse("Convenio eliminado satisfactoriamente.");
            });
        }

        private static string CrearSqlActualizaConvenio() => @"
            UPDATE CRD_CONVENIOS SET
                TIPO_CONVENIO = @TipoConvenio, DESCRIPCION = @Descripcion, CEDULA = @Cedula,
                COD_PROVEEDOR = @CodProveedor, CONTRATO_NUMERO = @ContratoNumero,
                FECHA_INICIO = @FechaInicio, FECHA_VENCIMIENTO = @FechaVencimiento,
                ACTIVO = @Activo, MODIFICA_FECHA = dbo.MyGetdate(), MODIFICA_USUARIO = @Usuario,
                PORC_COMISION_CREDITOS = @PorcComisionCreditos,
                PORC_COMISION_RECAUDACION = @PorcComisionRecaudacion,
                RESERVAS_PORC = @ReservasPorc, RESERVA_TOPE = @ReservaTope,
                COMISION_INFORMATIVA = @ComisionInformativa,
                COBRA_CREDITOS_ANULADOS = @CobraCreditosAnulados
            WHERE COD_CONVENIO = @CodConvenio;";

        private static string CrearSqlInsertaConvenio() => @"
            INSERT CRD_CONVENIOS
            (
                COD_CONVENIO, TIPO_CONVENIO, DESCRIPCION, CEDULA, COD_PROVEEDOR,
                CONTRATO_NUMERO, FECHA_INICIO, FECHA_VENCIMIENTO, ACTIVO,
                REGISTRO_FECHA, REGISTRO_USUARIO, MODIFICA_FECHA, MODIFICA_USUARIO,
                PORC_COMISION_CREDITOS, PORC_COMISION_RECAUDACION, RESERVAS_PORC,
                RESERVA_TOPE, ULTIMO_CORTE, COMISION_INFORMATIVA, COBRA_CREDITOS_ANULADOS
            )
            VALUES
            (
                @CodConvenio, @TipoConvenio, @Descripcion, @Cedula, @CodProveedor,
                @ContratoNumero, @FechaInicio, @FechaVencimiento, @Activo,
                dbo.MyGetdate(), @Usuario, dbo.MyGetdate(), @Usuario,
                @PorcComisionCreditos, @PorcComisionRecaudacion, @ReservasPorc,
                @ReservaTope, dbo.MyGetdate(), @ComisionInformativa, @CobraCreditosAnulados
            );";

        #endregion

        #region Recaudación

        /// <summary>
        /// Obtiene las retenciones o carteras administradas asociadas al convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConveniosRecaudacionData>> CR_Convenios_Recaudacion_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            return Ejecutar(CodEmpresa, connection =>
            {
                const string sql = @"
                    SELECT RTRIM(R.CODIGO) AS codigo, ISNULL(C.DESCRIPCION, '') AS descripcion,
                           R.REGISTRO_FECHA AS registro_fecha, ISNULL(R.REGISTRO_USUARIO, '') AS registro_usuario
                    FROM CRD_CONVENIOS_RETENCION R
                    INNER JOIN CATALOGO C ON C.CODIGO = R.CODIGO
                    WHERE R.COD_CONVENIO = @Convenio
                    ORDER BY R.CODIGO;";
                return DbHelper.CreateOkResponse(connection.Query<CrConveniosRecaudacionData>(sql, new { Convenio = convenio }).ToList());
            });
        }

        /// <summary>
        /// Obtiene las retenciones activas disponibles para asociar al convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel>CR_Convenios_Recaudacion_Retenciones_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            const string total = @"
        SELECT COUNT(1)
        FROM CATALOGO
        WHERE ACTIVO = 1
          AND (
                RETENCION = 'S'
                OR LINEA_INTERNA = 0
              );";

            const string lista = @"
        SELECT
            RTRIM(CODIGO) AS cod_convenio,
            ISNULL(DESCRIPCION, '') AS descripcion
        FROM CATALOGO
        WHERE ACTIVO = 1
          AND (
                RETENCION = 'S'
                OR LINEA_INTERNA = 0
              )
        ORDER BY DESCRIPCION, CODIGO;";

            return Ejecutar(CodEmpresa, connection =>
            {
                int totalRegistros =
                    connection.QueryFirstOrDefault<int>(total);

                List<CrConveniosF4Data> registros =
                    connection.Query<CrConveniosF4Data>(lista)
                        .ToList();

                return CrearResultadoLista(
                    totalRegistros,
                    registros);
            });
        }

        /// <summary>
        /// Navega por las retenciones activas disponibles para el convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>

        public ErrorDto<CrConveniosF4Data> CR_Convenios_Recaudacion_Scroll_Obtener(int CodEmpresa,CrConveniosRecaudacionScrollRequest? request)
        {
            string codigo = NormalizarCodigo(request?.codigo);
            int direccion = request?.direccion ?? 1;

            if (direccion != 1 && direccion != 2)
            {
                return DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                    "La dirección de navegación no es válida.",
                    -2);
            }

            return Ejecutar(CodEmpresa, connection =>
            {
                if (string.IsNullOrWhiteSpace(codigo) && direccion == 2)
                {
                    return DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                        MensajeNoMasRegistros,
                        -2);
                }

                const string sql = @"
            SELECT TOP 1
                RTRIM(CODIGO) AS cod_convenio,
                ISNULL(DESCRIPCION, '') AS descripcion
            FROM CATALOGO
            WHERE ACTIVO = 1
              AND (
                  RETENCION = 'S'
                  OR LINEA_INTERNA = 0
              )
              AND (
                  (@Codigo = '' AND @Direccion = 1)
                  OR (@Direccion = 1 AND CODIGO > @Codigo)
                  OR (@Direccion = 2 AND CODIGO < @Codigo)
              )
            ORDER BY
                CASE WHEN @Direccion = 1 THEN CODIGO END ASC,
                CASE WHEN @Direccion = 2 THEN CODIGO END DESC;";

                CrConveniosF4Data? resultado =
                    connection.QueryFirstOrDefault<CrConveniosF4Data>(
                        sql,
                        new
                        {
                            Codigo = codigo,
                            Direccion = direccion
                        });

                return resultado == null
                    ? DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                        MensajeNoMasRegistros,
                        -2)
                    : DbHelper.CreateOkResponse(resultado);
            });
        }


        /// <summary>
        /// Asocia o desasocia una retención de recaudación y registra el movimiento efectuado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Convenios_Recaudacion_Actualizar(int CodEmpresa,string? usuario,CrConveniosRecaudacionActualizarRequest? request)
        {
            string convenio =
                NormalizarCodigo(request?.cod_convenio);

            string codigo =
                NormalizarCodigo(request?.codigo);

            if (
                string.IsNullOrWhiteSpace(convenio) ||
                string.IsNullOrWhiteSpace(codigo)
            )
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el convenio y la retención.",
                    -2);
            }

            return Ejecutar(
                CodEmpresa,
                connection =>
                {
                    if (!ExisteConvenio(connection, convenio))
                    {
                        return DbHelper.ErrorResponse(
                            "Convenio no encontrado.",
                            -2);
                    }

                    const string existeOtro = @"
                SELECT TOP 1 1
                FROM CRD_CONVENIOS_RETENCION
                WHERE CODIGO = @Codigo
                  AND COD_CONVENIO <> @Convenio;";

                    if (
                        request?.asigna == true &&
                        ExisteRegistro(
                            connection,
                            existeOtro,
                            new
                            {
                                Codigo = codigo,
                                Convenio = convenio
                            })
                    )
                    {
                        return DbHelper.ErrorResponse(
                            "El código ya está asignado a otro convenio.",
                            -2);
                    }

                    string sql =
                        request?.asigna == true
                            ? @"
                        IF NOT EXISTS
                        (
                            SELECT 1
                            FROM CRD_CONVENIOS_RETENCION
                            WHERE CODIGO = @Codigo
                              AND COD_CONVENIO = @Convenio
                        )
                        INSERT CRD_CONVENIOS_RETENCION
                        (
                            CODIGO,
                            COD_CONVENIO,
                            REGISTRO_FECHA,
                            REGISTRO_USUARIO
                        )
                        VALUES
                        (
                            @Codigo,
                            @Convenio,
                            dbo.MyGetdate(),
                            @Usuario
                        );"
                            : @"
                        DELETE CRD_CONVENIOS_RETENCION
                        WHERE CODIGO = @Codigo
                          AND COD_CONVENIO = @Convenio;";

                    int afectados = connection.Execute(
                        sql,
                        new
                        {
                            Codigo = codigo,
                            Convenio = convenio,
                            Usuario = NormalizarUsuario(usuario)
                        });

                    if (
                        request?.asigna != true &&
                        afectados == 0
                    )
                    {
                        return DbHelper.ErrorResponse(
                            "La retención no se encuentra asociada al convenio.",
                            -2);
                    }

                    RegistrarBitacora(
                        CodEmpresa,
                        usuario,
                        request?.asigna == true
                            ? "REGISTRA - WEB"
                            : "ELIMINA - WEB",
                        $"Convenio: {convenio} - Recaudación: {codigo}");

                    return DbHelper.OkResponse(
                        "Información actualizada satisfactoriamente.");
                });
        }

        #endregion

        #region Devoluciones

        /// <summary>
        /// Obtiene los conceptos de retención asociados a las devoluciones del convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConveniosDevolucionesData>> CR_Convenios_Devoluciones_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            return Ejecutar(CodEmpresa, connection =>
            {
                const string sql = @"
                    SELECT RTRIM(D.RETENCION_CODIGO) AS retencion_codigo, ISNULL(C.DESCRIPCION, '') AS descripcion,
                           D.REGISTRO_FECHA AS registro_fecha, ISNULL(D.REGISTRO_USUARIO, '') AS registro_usuario
                    FROM CRD_CONVENIOS_DEVOLUCION D
                    INNER JOIN FND_RETENCION_CONCEPTOS C ON C.RETENCION_CODIGO = D.RETENCION_CODIGO
                    WHERE D.COD_CONVENIO = @Convenio
                    ORDER BY D.RETENCION_CODIGO;";
                return DbHelper.CreateOkResponse(connection.Query<CrConveniosDevolucionesData>(sql, new { Convenio = convenio }).ToList());
            });
        }

        /// <summary>
        /// Obtiene los conceptos de retención activos para la búsqueda F4 de devoluciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Devoluciones_Conceptos_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            const string total = @"
                SELECT COUNT(1) FROM FND_RETENCION_CONCEPTOS
                WHERE ACTIVO = 1 AND (@Texto IS NULL OR RETENCION_CODIGO LIKE @Like OR DESCRIPCION LIKE @Like);";
            const string lista = @"
                SELECT RTRIM(RETENCION_CODIGO) AS cod_convenio, ISNULL(DESCRIPCION, '') AS descripcion
                FROM FND_RETENCION_CONCEPTOS
                WHERE ACTIVO = 1 AND (@Texto IS NULL OR RETENCION_CODIGO LIKE @Like OR DESCRIPCION LIKE @Like)
                ORDER BY RETENCION_CODIGO";
            return ObtenerF4<CrConveniosF4Data>(CodEmpresa, request?.filtros, request?.texto, total, lista);
        }

        /// <summary>
        /// Navega por los conceptos de retención activos para devoluciones.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConveniosF4Data> CR_Convenios_Devoluciones_Scroll_Obtener( int CodEmpresa,CrConveniosDevolucionesScrollRequest? request)
        {
            string codigo = NormalizarCodigo(request?.retencion_codigo);
            int direccion = request?.direccion ?? 1;

            if (direccion != 1 && direccion != 2)
            {
                return DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                    "La dirección de navegación no es válida.", -2);
            }

            return Ejecutar(CodEmpresa, connection =>
            {
                if (string.IsNullOrWhiteSpace(codigo) && direccion == 2)
                {
                    return DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                        MensajeNoMasRegistros, -2);
                }

                const string sql = @"
            SELECT TOP 1
                RTRIM(RETENCION_CODIGO) AS cod_convenio,
                ISNULL(DESCRIPCION, '') AS descripcion
            FROM FND_RETENCION_CONCEPTOS
            WHERE ACTIVO = 1
              AND (
                  (@Codigo = '' AND @Direccion = 1)
                  OR (@Direccion = 1 AND RETENCION_CODIGO > @Codigo)
                  OR (@Direccion = 2 AND RETENCION_CODIGO < @Codigo)
              )
            ORDER BY
                CASE WHEN @Direccion = 1 THEN RETENCION_CODIGO END ASC,
                CASE WHEN @Direccion = 2 THEN RETENCION_CODIGO END DESC;";

                CrConveniosF4Data? resultado =
                    connection.QueryFirstOrDefault<CrConveniosF4Data>(
                        sql,
                        new { Codigo = codigo, Direccion = direccion });

                return resultado == null
                    ? DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                        MensajeNoMasRegistros, -2)
                    : DbHelper.CreateOkResponse(resultado);
            });
        }

        /// <summary>
        /// Asocia o desasocia un concepto de devolución y registra el movimiento efectuado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Convenios_Devoluciones_Actualizar(int CodEmpresa, string? usuario, CrConveniosDevolucionesActualizarRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            string codigo = NormalizarCodigo(request?.retencion_codigo);
            if (string.IsNullOrWhiteSpace(convenio) || string.IsNullOrWhiteSpace(codigo))
                return DbHelper.ErrorResponse("Debe indicar el convenio y el concepto de devolución.", -2);

            return Ejecutar(CodEmpresa, connection =>
            {
                if (!ExisteConvenio(connection, convenio))
                    return DbHelper.ErrorResponse("Convenio no encontrado.", -2);
                if (request?.asigna == true && !ExisteConceptoDevolucion(connection, codigo))
                    return DbHelper.ErrorResponse("El concepto de devolución indicado no está activo.", -2);

                const string existeOtro = "SELECT TOP 1 1 FROM CRD_CONVENIOS_DEVOLUCION WHERE RETENCION_CODIGO = @Codigo AND COD_CONVENIO <> @Convenio;";
                if (request?.asigna == true && ExisteRegistro(connection, existeOtro, new { Codigo = codigo, Convenio = convenio }))
                    return DbHelper.ErrorResponse("El código ya está asignado a otro convenio.", -2);

                string sql = request?.asigna == true
                    ? @"IF NOT EXISTS (SELECT 1 FROM CRD_CONVENIOS_DEVOLUCION WHERE RETENCION_CODIGO = @Codigo AND COD_CONVENIO = @Convenio)
                        INSERT CRD_CONVENIOS_DEVOLUCION(COD_CONVENIO, RETENCION_CODIGO, REGISTRO_FECHA, REGISTRO_USUARIO)
                        VALUES(@Convenio, @Codigo, dbo.MyGetdate(), @Usuario);"
                    : "DELETE CRD_CONVENIOS_DEVOLUCION WHERE COD_CONVENIO = @Convenio AND RETENCION_CODIGO = @Codigo;";
                int afectados = connection.Execute(sql, new { Codigo = codigo, Convenio = convenio, Usuario = NormalizarUsuario(usuario) });
                if (request?.asigna != true && afectados == 0)
                    return DbHelper.ErrorResponse("El concepto de devolución no se encuentra asociado al convenio.", -2);
                RegistrarBitacora(CodEmpresa, usuario, request?.asigna == true ? "REGISTRA - WEB" : "ELIMINA - WEB", $"Convenio: {convenio} - Devolución: {codigo}");
                return DbHelper.OkResponse("Información actualizada satisfactoriamente.");
            });
        }

        #endregion

        #region Créditos

        /// <summary>
        /// Obtiene las líneas o destinos de crédito asociados al convenio según el tipo solicitado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConveniosCreditoData>> CR_Convenios_Creditos_Lista_Obtener(int CodEmpresa, CrConveniosCreditosListaRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            string tipo = NormalizarCodigo(request?.tipo);
            return tipo == "D" ? ObtenerDestinosCredito(CodEmpresa, convenio) : ObtenerLineasCredito(CodEmpresa, convenio);
        }

        /// <summary>
        /// Obtiene las líneas o destinos disponibles para la búsqueda F4 de créditos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Creditos_F4_Obtener(int CodEmpresa, CrConveniosCreditosBusquedaRequest? request)
        {
            bool destinos = NormalizarCodigo(request?.tipo) == "D";
            return destinos ? ObtenerF4Destinos(CodEmpresa, request) : ObtenerF4Lineas(CodEmpresa, request);
        }

        /// <summary>
        /// Navega por las líneas o destinos de crédito disponibles según el tipo solicitado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConveniosF4Data> CR_Convenios_Creditos_Scroll_Obtener(int CodEmpresa, CrConveniosCreditosScrollRequest? request)
        {
            string tipo =
                NormalizarCodigo(request?.tipo);

            string codigo =
                NormalizarCodigo(request?.codigo);

            int direccion =
                request?.direccion ?? 1;

            return tipo == "D"
                ? ObtenerDestinoScroll(
                    CodEmpresa,
                    direccion,
                    codigo)
                : ObtenerLineaScroll(
                    CodEmpresa,
                    direccion,
                    codigo);
        }

        /// <summary>
        /// Asocia o desasocia una línea o destino de crédito preservando la regla de combinación de destino.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Convenios_Creditos_Actualizar(int CodEmpresa, string? usuario, CrConveniosCreditosActualizarRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            string tipo = NormalizarCodigo(request?.tipo);
            string codigo = NormalizarCodigo(request?.codigo);
            if (string.IsNullOrWhiteSpace(convenio) || string.IsNullOrWhiteSpace(codigo))
                return DbHelper.ErrorResponse("Debe indicar el convenio y el código de crédito.", -2);
            return tipo == "D"
                ? ActualizarDestinoCredito(CodEmpresa, usuario, convenio, codigo, request?.asigna == true)
                : ActualizarLineaCredito(CodEmpresa, usuario, convenio, codigo, request?.combina_destino == true, request?.asigna == true);
        }

        private ErrorDto<List<CrConveniosCreditoData>> ObtenerLineasCredito(int codEmpresa, string convenio)
        {
            return Ejecutar(codEmpresa, connection =>
            {
                const string sql = @"
                    SELECT 'C' AS tipo, RTRIM(L.CODIGO) AS codigo, ISNULL(C.DESCRIPCION, '') AS descripcion,
                           CAST(ISNULL(L.COMBINA_DESTINO, 0) AS bit) AS combina_destino,
                           CASE WHEN ISNULL(L.COMBINA_DESTINO, 0) = 1 THEN 'S' ELSE 'N' END AS asociar_descripcion,
                           L.REGISTRO_FECHA AS registro_fecha, ISNULL(L.REGISTRO_USUARIO, '') AS registro_usuario
                    FROM CRD_CONVENIOS_LINEAS L
                    INNER JOIN CATALOGO C ON C.CODIGO = L.CODIGO
                    WHERE L.COD_CONVENIO = @Convenio
                    ORDER BY L.CODIGO;";
                return DbHelper.CreateOkResponse(connection.Query<CrConveniosCreditoData>(sql, new { Convenio = convenio }).ToList());
            });
        }

        private ErrorDto<List<CrConveniosCreditoData>> ObtenerDestinosCredito(int codEmpresa, string convenio)
        {
            return Ejecutar(codEmpresa, connection =>
            {
                const string sql = @"
                    SELECT 'D' AS tipo, RTRIM(D.COD_DESTINO) AS codigo, ISNULL(C.DESCRIPCION, '') AS descripcion,
                           CAST(0 AS bit) AS combina_destino, '....' AS asociar_descripcion,
                           D.REGISTRO_FECHA AS registro_fecha, ISNULL(D.REGISTRO_USUARIO, '') AS registro_usuario
                    FROM CRD_CONVENIOS_DESTINOS D
                    INNER JOIN CATALOGO_DESTINOS C ON C.COD_DESTINO = D.COD_DESTINO
                    WHERE D.COD_CONVENIO = @Convenio
                    ORDER BY D.COD_DESTINO;";
                return DbHelper.CreateOkResponse(connection.Query<CrConveniosCreditoData>(sql, new { Convenio = convenio }).ToList());
            });
        }

        private ErrorDto<TablasListaGenericaModel> ObtenerF4Lineas(int codEmpresa, CrConveniosCreditosBusquedaRequest? request)
        {
            const string total = @"
                SELECT COUNT(1) FROM CATALOGO
                WHERE RETENCION = 'N' AND ACTIVO = 1 AND LINEA_INTERNA = 1
                  AND (@Texto IS NULL OR CODIGO LIKE @Like OR DESCRIPCION LIKE @Like);";
            const string lista = @"
                SELECT RTRIM(CODIGO) AS cod_convenio, ISNULL(DESCRIPCION, '') AS descripcion
                FROM CATALOGO
                WHERE RETENCION = 'N' AND ACTIVO = 1 AND LINEA_INTERNA = 1
                  AND (@Texto IS NULL OR CODIGO LIKE @Like OR DESCRIPCION LIKE @Like)
                ORDER BY CODIGO";
            return ObtenerF4<CrConveniosF4Data>(codEmpresa, request?.filtros, request?.texto, total, lista);
        }

        private ErrorDto<TablasListaGenericaModel> ObtenerF4Destinos(int codEmpresa, CrConveniosCreditosBusquedaRequest? request)
        {
            const string total = @"
                SELECT COUNT(1) FROM CATALOGO_DESTINOS
                WHERE @Texto IS NULL OR COD_DESTINO LIKE @Like OR DESCRIPCION LIKE @Like;";
            const string lista = @"
                SELECT RTRIM(COD_DESTINO) AS cod_convenio, ISNULL(DESCRIPCION, '') AS descripcion
                FROM CATALOGO_DESTINOS
                WHERE @Texto IS NULL OR COD_DESTINO LIKE @Like OR DESCRIPCION LIKE @Like
                ORDER BY COD_DESTINO";
            return ObtenerF4<CrConveniosF4Data>(codEmpresa, request?.filtros, request?.texto, total, lista);
        }

        private ErrorDto<CrConveniosF4Data> ObtenerLineaScroll(int codEmpresa,int direccion,string codigo)
        {
            if (direccion != 1 && direccion != 2)
            {
                return DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                    "La dirección de navegación no es válida.", -2);
            }

            if (string.IsNullOrWhiteSpace(codigo) && direccion == 2)
            {
                return DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                    MensajeNoMasRegistros, -2);
            }

            return Ejecutar(codEmpresa, connection =>
            {
                const string sql = @"
            SELECT TOP 1
                RTRIM(CODIGO) AS cod_convenio,
                ISNULL(DESCRIPCION, '') AS descripcion
            FROM CATALOGO
            WHERE RETENCION = 'N'
              AND ACTIVO = 1
              AND LINEA_INTERNA = 1
              AND (
                  (@Codigo = '' AND @Direccion = 1)
                  OR (@Direccion = 1 AND CODIGO > @Codigo)
                  OR (@Direccion = 2 AND CODIGO < @Codigo)
              )
            ORDER BY
                CASE WHEN @Direccion = 1 THEN CODIGO END ASC,
                CASE WHEN @Direccion = 2 THEN CODIGO END DESC;";

                CrConveniosF4Data? resultado =
                    connection.QueryFirstOrDefault<CrConveniosF4Data>(
                        sql,
                        new
                        {
                            Codigo = codigo,
                            Direccion = direccion
                        });

                return resultado == null
                    ? DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                        MensajeNoMasRegistros, -2)
                    : DbHelper.CreateOkResponse(resultado);
            });
        }

        private ErrorDto<CrConveniosF4Data> ObtenerDestinoScroll( int codEmpresa,int direccion,string codigo)
        {
            if (direccion != 1 && direccion != 2)
            {
                return DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                    "La dirección de navegación no es válida.", -2);
            }

            if (string.IsNullOrWhiteSpace(codigo) && direccion == 2)
            {
                return DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                    MensajeNoMasRegistros, -2);
            }

            return Ejecutar(codEmpresa, connection =>
            {
                const string sql = @"
            SELECT TOP 1
                RTRIM(COD_DESTINO) AS cod_convenio,
                ISNULL(DESCRIPCION, '') AS descripcion
            FROM CATALOGO_DESTINOS
            WHERE
                (@Codigo = '' AND @Direccion = 1)
                OR (@Direccion = 1 AND COD_DESTINO > @Codigo)
                OR (@Direccion = 2 AND COD_DESTINO < @Codigo)
            ORDER BY
                CASE WHEN @Direccion = 1 THEN COD_DESTINO END ASC,
                CASE WHEN @Direccion = 2 THEN COD_DESTINO END DESC;";

                CrConveniosF4Data? resultado =
                    connection.QueryFirstOrDefault<CrConveniosF4Data>(
                        sql,
                        new
                        {
                            Codigo = codigo,
                            Direccion = direccion
                        });

                return resultado == null
                    ? DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                        MensajeNoMasRegistros, -2)
                    : DbHelper.CreateOkResponse(resultado);
            });
        }

        private ErrorDto ActualizarLineaCredito(int codEmpresa, string? usuario, string convenio, string codigo, bool combinaDestino, bool asigna)
        {
            return Ejecutar(codEmpresa, connection =>
            {
                if (!ExisteConvenio(connection, convenio))
                    return DbHelper.ErrorResponse("Convenio no encontrado.", -2);
                if (asigna && !ExisteLineaCredito(connection, codigo))
                    return DbHelper.ErrorResponse("La línea de crédito indicada no está activa.", -2);

                const string sqlBloqueo = @"
                    SELECT TOP 1 COMBINA_DESTINO FROM CRD_CONVENIOS_LINEAS
                    WHERE CODIGO = @Codigo AND COD_CONVENIO <> @Convenio;";
                int? combinaRegistrada = connection.QueryFirstOrDefault<int?>(sqlBloqueo, new { Codigo = codigo, Convenio = convenio });
                if (asigna && combinaRegistrada.HasValue && combinaRegistrada.Value == 0)
                    return DbHelper.ErrorResponse("La línea de crédito ya se encuentra asignada a otro convenio.", -2);

                string sql = asigna
                    ? @"IF NOT EXISTS (SELECT 1 FROM CRD_CONVENIOS_LINEAS WHERE CODIGO = @Codigo AND COD_CONVENIO = @Convenio)
                        INSERT CRD_CONVENIOS_LINEAS(CODIGO, COD_CONVENIO, COMBINA_DESTINO, REGISTRO_FECHA, REGISTRO_USUARIO)
                        VALUES(@Codigo, @Convenio, @CombinaDestino, dbo.MyGetdate(), @Usuario);"
                    : "DELETE CRD_CONVENIOS_LINEAS WHERE CODIGO = @Codigo AND COD_CONVENIO = @Convenio;";
                int afectados = connection.Execute(sql, new { Codigo = codigo, Convenio = convenio, CombinaDestino = combinaDestino, Usuario = NormalizarUsuario(usuario) });
                if (!asigna && afectados == 0)
                    return DbHelper.ErrorResponse("La línea de crédito no se encuentra asociada al convenio.", -2);
                RegistrarBitacora(codEmpresa, usuario, asigna ? "REGISTRA - WEB" : "ELIMINA - WEB", $"Convenio: {convenio} - Línea de crédito: {codigo}");
                return DbHelper.OkResponse("Información actualizada satisfactoriamente.");
            });
        }

        private ErrorDto ActualizarDestinoCredito(int codEmpresa, string? usuario, string convenio, string codigo, bool asigna)
        {
            return Ejecutar(codEmpresa, connection =>
            {
                if (!ExisteConvenio(connection, convenio))
                    return DbHelper.ErrorResponse("Convenio no encontrado.", -2);
                if (asigna && !ExisteDestinoCredito(connection, codigo))
                    return DbHelper.ErrorResponse("El destino de crédito indicado no existe.", -2);

                const string existeOtro = "SELECT TOP 1 1 FROM CRD_CONVENIOS_DESTINOS WHERE COD_DESTINO = @Codigo AND COD_CONVENIO <> @Convenio;";
                if (asigna && ExisteRegistro(connection, existeOtro, new { Codigo = codigo, Convenio = convenio }))
                    return DbHelper.ErrorResponse("El destino de crédito ya se encuentra asignado a otro convenio.", -2);

                string sql = asigna
                    ? @"IF NOT EXISTS (SELECT 1 FROM CRD_CONVENIOS_DESTINOS WHERE COD_DESTINO = @Codigo AND COD_CONVENIO = @Convenio)
                        INSERT CRD_CONVENIOS_DESTINOS(COD_DESTINO, COD_CONVENIO, REGISTRO_FECHA, REGISTRO_USUARIO)
                        VALUES(@Codigo, @Convenio, dbo.MyGetdate(), @Usuario);"
                    : "DELETE CRD_CONVENIOS_DESTINOS WHERE COD_DESTINO = @Codigo AND COD_CONVENIO = @Convenio;";
                int afectados = connection.Execute(sql, new { Codigo = codigo, Convenio = convenio, Usuario = NormalizarUsuario(usuario) });
                if (!asigna && afectados == 0)
                    return DbHelper.ErrorResponse("El destino de crédito no se encuentra asociado al convenio.", -2);
                RegistrarBitacora(codEmpresa, usuario, asigna ? "REGISTRA - WEB" : "ELIMINA - WEB", $"Convenio: {convenio} - Destino de crédito: {codigo}");
                return DbHelper.OkResponse("Información actualizada satisfactoriamente.");
            });
        }

        #endregion

        #region Cargos

        /// <summary>
        /// Obtiene los cargos adicionales asociados al convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConveniosCargoData>> CR_Convenios_Cargos_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            return Ejecutar(CodEmpresa, connection =>
            {
                const string sql = @"
                    SELECT RTRIM(CC.COD_CARGO) AS cod_cargo, ISNULL(CA.DESCRIPCION, '') AS descripcion, ISNULL(CA.VALOR, 0) AS valor
                    FROM CRD_CONVENIOS_CARGOS CC
                    INNER JOIN CARGOS_ADICIONALES CA ON CA.COD_CARGO = CC.COD_CARGO
                    WHERE CC.COD_CONVENIO = @Convenio
                    ORDER BY CC.COD_CARGO;";
                return DbHelper.CreateOkResponse(connection.Query<CrConveniosCargoData>(sql, new { Convenio = convenio }).ToList());
            });
        }

        /// <summary>
        /// Obtiene los cargos adicionales disponibles para la búsqueda F4 del convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Cargos_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            const string total = @"
                SELECT COUNT(1) FROM CARGOS_ADICIONALES
                WHERE @Texto IS NULL OR COD_CARGO LIKE @Like OR DESCRIPCION LIKE @Like;";
            const string lista = @"
                SELECT RTRIM(COD_CARGO) AS cod_convenio, ISNULL(DESCRIPCION, '') AS descripcion
                FROM CARGOS_ADICIONALES
                WHERE @Texto IS NULL OR COD_CARGO LIKE @Like OR DESCRIPCION LIKE @Like
                ORDER BY COD_CARGO";
            return ObtenerF4<CrConveniosF4Data>(CodEmpresa, request?.filtros, request?.texto, total, lista);
        }

        /// <summary>
        /// Navega por los cargos adicionales disponibles para el convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConveniosF4Data> CR_Convenios_Cargos_Scroll_Obtener(int CodEmpresa,CrConveniosCargosScrollRequest? request)
        {
            string codigo = NormalizarCodigo(request?.cod_cargo);
            int direccion = request?.direccion ?? 1;

            if (direccion != 1 && direccion != 2)
            {
                return DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                    "La dirección de navegación no es válida.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(codigo) && direccion == 2)
            {
                return DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                    MensajeNoMasRegistros,
                    -2);
            }

            return Ejecutar(CodEmpresa, connection =>
            {
                const string sql = @"
            SELECT TOP 1
                RTRIM(COD_CARGO) AS cod_convenio,
                ISNULL(DESCRIPCION, '') AS descripcion
            FROM CARGOS_ADICIONALES
            WHERE
                (@Codigo = '' AND @Direccion = 1)
                OR (@Direccion = 1 AND COD_CARGO > @Codigo)
                OR (@Direccion = 2 AND COD_CARGO < @Codigo)
            ORDER BY
                CASE
                    WHEN @Direccion = 1 THEN COD_CARGO
                END ASC,
                CASE
                    WHEN @Direccion = 2 THEN COD_CARGO
                END DESC;";

                CrConveniosF4Data? resultado =
                    connection.QueryFirstOrDefault<CrConveniosF4Data>(
                        sql,
                        new
                        {
                            Codigo = codigo,
                            Direccion = direccion
                        });

                return resultado == null
                    ? DbHelper.CreateErrorResponse<CrConveniosF4Data>(
                        MensajeNoMasRegistros,
                        -2)
                    : DbHelper.CreateOkResponse(resultado);
            });
        }

        /// <summary>
        /// Asocia o desasocia un cargo adicional y registra el movimiento efectuado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Convenios_Cargos_Actualizar(int CodEmpresa, string? usuario, CrConveniosCargosActualizarRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            string codigo = NormalizarCodigo(request?.cod_cargo);
            if (string.IsNullOrWhiteSpace(convenio) || string.IsNullOrWhiteSpace(codigo))
                return DbHelper.ErrorResponse("Debe indicar el convenio y el cargo.", -2);

            return Ejecutar(CodEmpresa, connection =>
            {
                if (!ExisteConvenio(connection, convenio))
                    return DbHelper.ErrorResponse("Convenio no encontrado.", -2);
                if (request?.asigna == true && !ExisteCargo(connection, codigo))
                    return DbHelper.ErrorResponse("El cargo indicado no existe.", -2);
                const string existeOtro = "SELECT TOP 1 1 FROM CRD_CONVENIOS_CARGOS WHERE COD_CARGO = @Codigo AND COD_CONVENIO <> @Convenio;";
                if (request?.asigna == true && ExisteRegistro(connection, existeOtro, new { Codigo = codigo, Convenio = convenio }))
                    return DbHelper.ErrorResponse("El cargo ya está asignado a otro convenio.", -2);

                string sql = request?.asigna == true
                    ? @"IF NOT EXISTS (SELECT 1 FROM CRD_CONVENIOS_CARGOS WHERE COD_CARGO = @Codigo AND COD_CONVENIO = @Convenio)
                        INSERT CRD_CONVENIOS_CARGOS(COD_CARGO, COD_CONVENIO, REGISTRO_FECHA, REGISTRO_USUARIO)
                        VALUES(@Codigo, @Convenio, dbo.MyGetdate(), @Usuario);"
                    : "DELETE CRD_CONVENIOS_CARGOS WHERE COD_CARGO = @Codigo AND COD_CONVENIO = @Convenio;";
                int afectados = connection.Execute(sql, new { Codigo = codigo, Convenio = convenio, Usuario = NormalizarUsuario(usuario) });
                if (request?.asigna != true && afectados == 0)
                    return DbHelper.ErrorResponse("El cargo no se encuentra asociado al convenio.", -2);
                RegistrarBitacora(CodEmpresa, usuario, request?.asigna == true ? "REGISTRA - WEB" : "ELIMINA - WEB", $"Convenio: {convenio} - Cargo: {codigo}");
                return DbHelper.OkResponse("Información actualizada satisfactoriamente.");
            });
        }

        #endregion

        #region Rebajas de CxP

        /// <summary>
        /// Obtiene los cargos de cuentas por pagar y su estado de asociación con el convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConveniosRebajasCxpData>> CR_Convenios_RebajasCxP_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            return Ejecutar(CodEmpresa, connection =>
            {
                const string sql = @"
                    SELECT RTRIM(C.COD_CARGO) AS cod_cargo, ISNULL(C.DESCRIPCION, '') AS descripcion,
                           ISNULL(C.COD_CUENTA, '') AS cod_cuenta, ISNULL(CTA.COD_CUENTA_MASK, '') AS cod_cuenta_mask,
                           ISNULL(CTA.DESCRIPCION, '') AS cuenta_descripcion,
                           CAST(CASE WHEN CC.COD_CARGO IS NULL THEN 0 ELSE 1 END AS bit) AS asignado
                    FROM CXP_CARGOS C
                    LEFT JOIN CRD_CONVENIOS_CARGOS_CXP CC ON CC.COD_CARGO = C.COD_CARGO AND CC.COD_CONVENIO = @Convenio
                    INNER JOIN vCNTX_CUENTAS_LOCAL CTA ON CTA.COD_CUENTA = C.COD_CUENTA
                    WHERE C.ACTIVO = 1
                    ORDER BY ISNULL(CC.COD_CARGO, 'ZZZZZZ'), C.COD_CARGO;";
                return DbHelper.CreateOkResponse(connection.Query<CrConveniosRebajasCxpData>(sql, new { Convenio = convenio }).ToList());
            });
        }

        /// <summary>
        /// Asocia o desasocia un cargo de cuentas por pagar para rebajas del convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Convenios_RebajasCxP_Actualizar(int CodEmpresa, string? usuario, CrConveniosRebajasCxpActualizarRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            string cargo = NormalizarCodigo(request?.cod_cargo);
            if (string.IsNullOrWhiteSpace(convenio) || string.IsNullOrWhiteSpace(cargo))
                return DbHelper.ErrorResponse("Debe indicar el convenio y el cargo de CxP.", -2);

            return Ejecutar(CodEmpresa, connection =>
            {
                if (!ExisteConvenio(connection, convenio))
                    return DbHelper.ErrorResponse("Convenio no encontrado.", -2);
                if (request?.asigna == true && !ExisteCargoCxp(connection, cargo))
                    return DbHelper.ErrorResponse("El cargo de CxP indicado no está activo.", -2);
                string sql = request?.asigna == true
                    ? @"IF NOT EXISTS (SELECT 1 FROM CRD_CONVENIOS_CARGOS_CXP WHERE COD_CARGO = @Cargo AND COD_CONVENIO = @Convenio)
                        INSERT CRD_CONVENIOS_CARGOS_CXP(COD_CARGO, COD_CONVENIO, REGISTRO_FECHA, REGISTRO_USUARIO)
                        VALUES(@Cargo, @Convenio, dbo.MyGetdate(), @Usuario);"
                    : "DELETE CRD_CONVENIOS_CARGOS_CXP WHERE COD_CARGO = @Cargo AND COD_CONVENIO = @Convenio;";
                int afectados = connection.Execute(sql, new { Cargo = cargo, Convenio = convenio, Usuario = NormalizarUsuario(usuario) });
                if (request?.asigna != true && afectados == 0)
                    return DbHelper.ErrorResponse("El cargo de CxP no se encuentra asociado al convenio.", -2);
                RegistrarBitacora(CodEmpresa, usuario, request?.asigna == true ? "REGISTRA - WEB" : "ELIMINA - WEB", $"Convenio: {convenio} - Rebaja CxP: {cargo}");
                return DbHelper.OkResponse("Información actualizada satisfactoriamente.");
            });
        }

        #endregion

        #region Ahorros

        /// <summary>
        /// Obtiene los planes de ahorro y contratos asociados al convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConveniosAhorrosData>> CR_Convenios_Ahorros_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            return Ejecutar(CodEmpresa, connection =>
            {
                const string sql = @"
                    SELECT RTRIM(CVP.COD_PLAN) AS cod_plan, ISNULL(P.DESCRIPCION, '') AS descripcion,
                           CVP.COD_CONTRATO AS cod_contrato, CNT.FECHA_INICIO AS fecha_inicio,
                           ISNULL(CVP.MENSUALIDAD, 0) AS mensualidad,
                           ISNULL(CNT.APORTES, 0) + ISNULL(CNT.RENDIMIENTO, 0) AS acumulado
                    FROM CRD_CONVENIOS_PLANES CVP
                    INNER JOIN FND_CONTRATOS CNT ON CNT.COD_OPERADORA = 1 AND CNT.COD_PLAN = CVP.COD_PLAN AND CNT.COD_CONTRATO = CVP.COD_CONTRATO
                    INNER JOIN FND_PLANES P ON P.COD_OPERADORA = CNT.COD_OPERADORA AND P.COD_PLAN = CNT.COD_PLAN
                    WHERE CVP.COD_CONVENIO = @Convenio
                    ORDER BY CVP.COD_PLAN, CVP.COD_CONTRATO;";
                return DbHelper.CreateOkResponse(connection.Query<CrConveniosAhorrosData>(sql, new { Convenio = convenio }).ToList());
            });
        }

        /// <summary>
        /// Obtiene los planes de ahorro disponibles para la búsqueda F4 del convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Ahorros_Planes_F4_Obtener(int CodEmpresa, CrConveniosAhorrosPlanesBusquedaRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);

            const string total = @"
        SELECT COUNT(1)
        FROM FND_PLANES
        WHERE ESTADO = 'A';";

            const string lista = @"
        SELECT
            RTRIM(P.COD_PLAN) AS cod_plan,
            ISNULL(P.DESCRIPCION, '') AS descripcion,
            CP.COD_CONTRATO AS cod_contrato,
            CP.MENSUALIDAD AS mensualidad
        FROM FND_PLANES P
        OUTER APPLY
        (
            SELECT TOP 1
                C.COD_CONTRATO,
                C.MENSUALIDAD
            FROM CRD_CONVENIOS_PLANES C
            WHERE C.COD_CONVENIO = @Convenio
              AND C.COD_PLAN = P.COD_PLAN
            ORDER BY C.COD_CONTRATO DESC
        ) CP
        WHERE P.ESTADO = 'A'
        ORDER BY P.DESCRIPCION;";

            return Ejecutar(CodEmpresa, connection =>
            {
                int totalRegistros = connection.QueryFirstOrDefault<int>(total);

                List<CrConveniosAhorroPlanData> registros = connection.Query<CrConveniosAhorroPlanData>(
                    lista,
                    new
                    {
                        Convenio = convenio
                    }).ToList();

                return CrearResultadoLista(totalRegistros, registros);
            });
        }

        /// <summary>
        /// Navega por los planes de ahorro activos disponibles para el convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrConveniosAhorroPlanData> CR_Convenios_Ahorros_Planes_Scroll_Obtener(int CodEmpresa,CrConveniosAhorrosScrollRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            string codigo = NormalizarCodigo(request?.cod_plan);
            int direccion = request?.direccion ?? 1;

            if (direccion != 1 && direccion != 2)
            {
                return DbHelper.CreateErrorResponse<CrConveniosAhorroPlanData>(
                    "La dirección de navegación no es válida.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(codigo) && direccion == 2)
            {
                return DbHelper.CreateErrorResponse<CrConveniosAhorroPlanData>(
                    MensajeNoMasRegistros,
                    -2);
            }

            return Ejecutar(CodEmpresa, connection =>
            {
                const string sql = @"
            SELECT TOP 1
                RTRIM(P.COD_PLAN) AS cod_plan,
                ISNULL(P.DESCRIPCION, '') AS descripcion,
                CP.COD_CONTRATO AS cod_contrato,
                CP.MENSUALIDAD AS mensualidad
            FROM FND_PLANES P
            OUTER APPLY
            (
                SELECT TOP 1
                    C.COD_CONTRATO,
                    C.MENSUALIDAD
                FROM CRD_CONVENIOS_PLANES C
                WHERE C.COD_CONVENIO = @Convenio
                  AND C.COD_PLAN = P.COD_PLAN
                ORDER BY C.COD_CONTRATO DESC
            ) CP
            WHERE P.ESTADO = 'A'
              AND (
                    (@Codigo = '' AND @Direccion = 1)
                    OR (@Direccion = 1 AND P.COD_PLAN > @Codigo)
                    OR (@Direccion = 2 AND P.COD_PLAN < @Codigo)
              )
            ORDER BY
                CASE
                    WHEN @Direccion = 1 THEN P.COD_PLAN
                END ASC,
                CASE
                    WHEN @Direccion = 2 THEN P.COD_PLAN
                END DESC;";

                CrConveniosAhorroPlanData? resultado =
                    connection.QueryFirstOrDefault<CrConveniosAhorroPlanData>(
                        sql,
                        new
                        {
                            Convenio = convenio,
                            Codigo = codigo,
                            Direccion = direccion
                        });

                return resultado == null
                    ? DbHelper.CreateErrorResponse<CrConveniosAhorroPlanData>(
                        MensajeNoMasRegistros,
                        -2)
                    : DbHelper.CreateOkResponse(resultado);
            });
        }

        /// <summary>
        /// Obtiene contratos activos de ahorro del cliente del convenio o de terceros para la búsqueda F4.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Ahorros_Contratos_F4_Obtener(int CodEmpresa, CrConveniosAhorrosContratosBusquedaRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            if (string.IsNullOrWhiteSpace(convenio))
                return DbHelper.CreateErrorResponse<TablasListaGenericaModel>("Debe indicar el convenio.", -2);

            const string total = @"
                SELECT COUNT(1)
                FROM CRD_CONVENIOS V
                INNER JOIN FND_CONTRATOS C ON C.COD_OPERADORA = 1
                INNER JOIN FND_PLANES P ON P.COD_OPERADORA = C.COD_OPERADORA AND P.COD_PLAN = C.COD_PLAN
                WHERE V.COD_CONVENIO = @Convenio AND C.ESTADO = 'A' AND P.TIPO_CDP = 0
                  AND ((@Terceros = 0 AND V.CEDULA = C.CEDULA) OR (@Terceros = 1 AND V.CEDULA <> C.CEDULA))
                  AND (@Texto IS NULL OR C.COD_PLAN LIKE @Like OR P.DESCRIPCION LIKE @Like OR CAST(C.COD_CONTRATO AS varchar(20)) LIKE @Like);";
            const string lista = @"
                SELECT RTRIM(C.COD_PLAN) AS cod_plan, C.COD_CONTRATO AS cod_contrato, ISNULL(C.MONTO, 0) AS monto,
                       ISNULL(P.DESCRIPCION, '') AS descripcion, ISNULL(RTRIM(C.CEDULA), '') AS cedula,
                       ISNULL(S.NOMBRE, '') AS cliente_nombre
                FROM CRD_CONVENIOS V
                INNER JOIN FND_CONTRATOS C ON C.COD_OPERADORA = 1
                INNER JOIN FND_PLANES P ON P.COD_OPERADORA = C.COD_OPERADORA AND P.COD_PLAN = C.COD_PLAN
                LEFT JOIN SOCIOS S ON S.CEDULA = C.CEDULA
                WHERE V.COD_CONVENIO = @Convenio AND C.ESTADO = 'A' AND P.TIPO_CDP = 0
                  AND ((@Terceros = 0 AND V.CEDULA = C.CEDULA) OR (@Terceros = 1 AND V.CEDULA <> C.CEDULA))
                  AND (@Texto IS NULL OR C.COD_PLAN LIKE @Like OR P.DESCRIPCION LIKE @Like OR CAST(C.COD_CONTRATO AS varchar(20)) LIKE @Like)
                ORDER BY P.DESCRIPCION, C.COD_PLAN, C.COD_CONTRATO";

            return Ejecutar(CodEmpresa, connection =>
            {
                BusquedaF4 busqueda = CrearBusqueda(request?.filtros, request?.texto);
                var parametros = new { Convenio = convenio, Terceros = request?.terceros == true ? 1 : 0, Texto = busqueda.texto, Like = busqueda.like, Offset = busqueda.offset, Filas = busqueda.filas };
                int cantidad = connection.QueryFirstOrDefault<int>(total, parametros);
                string consulta = busqueda.usar_paginacion ? lista + " OFFSET @Offset ROWS FETCH NEXT @Filas ROWS ONLY;" : lista + ";";
                List<CrConveniosAhorrosContratoF4Data> contratos = connection.Query<CrConveniosAhorrosContratoF4Data>(consulta, parametros).ToList();
                return CrearResultadoLista(cantidad, contratos);
            });
        }

        /// <summary>
        /// Asocia, modifica o desasocia un fondo del convenio mediante el procedimiento histórico de fondos pool.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_Convenios_Ahorros_Actualizar(int CodEmpresa, string? usuario, CrConveniosAhorrosActualizarRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            string plan = NormalizarCodigo(request?.cod_plan);
            if (string.IsNullOrWhiteSpace(convenio) || string.IsNullOrWhiteSpace(plan))
                return DbHelper.ErrorResponse("Debe indicar el convenio y el plan de ahorro.", -2);
            if (request?.asigna == true && request.mensualidad <= 0)
                return DbHelper.ErrorResponse("Debe indicar una mensualidad mayor a cero.", -2);

            return Ejecutar(CodEmpresa, connection =>
            {
                if (!ExisteConvenio(connection, convenio))
                    return DbHelper.ErrorResponse("Convenio no encontrado.", -2);
                var parametros = new DynamicParameters();
                parametros.Add("@Convenio", convenio, DbType.String);
                parametros.Add("@Plan", plan, DbType.String);
                parametros.Add("@Contrato", request?.cod_contrato ?? 0, DbType.Int32);
                parametros.Add("@Monto", request?.mensualidad ?? 0, DbType.Decimal);
                parametros.Add("@Tipo", request?.asigna == true ? "M" : "E", DbType.StringFixedLength);
                connection.Execute("spConvenios_FondosPool", parametros, commandType: CommandType.StoredProcedure);
                RegistrarBitacora(CodEmpresa, usuario, request?.asigna == true ? "REGISTRA - WEB" : "ELIMINA - WEB", $"Convenio: {convenio} - Plan de ahorro: {plan}");
                return DbHelper.OkResponse("Información actualizada satisfactoriamente.");
            });
        }

        #endregion

        #region Órdenes y Reservas

        /// <summary>
        /// Obtiene las órdenes de pago procesadas del convenio según la cantidad de líneas solicitada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConveniosOrdenData>> CR_Convenios_Ordenes_Lista_Obtener(int CodEmpresa, CrConveniosOrdenesListaRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            int lineas = request?.lineas ?? 50;
            if (lineas <= 0)
                lineas = 50;

            return Ejecutar(CodEmpresa, connection =>
            {
                const string sql = @"
                    SELECT TOP (@Lineas) COD_ORDEN AS cod_orden, REGISTRO_FECHA AS registro_fecha,
                           ISNULL(REGISTRO_USUARIO, '') AS registro_usuario, FECHA_INICIO AS fecha_inicio,
                           FECHA_CORTE AS fecha_corte, ISNULL(RECAUDACION_CUOTAS, 0) AS recaudacion_cuotas,
                           ISNULL(RECAUDACION_CARGOS, 0) AS recaudacion_cargos, ISNULL(NUEVOS_CREDITOS, 0) AS nuevos_creditos,
                           ISNULL(TOTAL_REBAJOS_CXP, 0) AS total_rebajos_cxp, ISNULL(RETENCION_AHORROS, 0) AS retencion_ahorros,
                           ISNULL(TOTAL_PAGAR, 0) AS total_pagar, ISNULL(DOCUMENTO, '') AS documento
                    FROM CRD_CONVENIOS_ORDENES
                    WHERE COD_CONVENIO = @Convenio
                    ORDER BY COD_ORDEN DESC;";
                return DbHelper.CreateOkResponse(connection.Query<CrConveniosOrdenData>(sql, new { Convenio = convenio, Lineas = lineas }).ToList());
            });
        }

        /// <summary>
        /// Obtiene los cortes y saldos históricos de reservas registrados para el convenio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConveniosReservaData>> CR_Convenios_Reservas_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            string convenio = NormalizarCodigo(request?.cod_convenio);
            return Ejecutar(CodEmpresa, connection =>
            {
                const string sql = @"
                    SELECT RTRIM(COD_CONVENIO) AS cod_convenio, CORTE_INICIAL AS corte_inicial,
                           ISNULL(SALDO_INICIAL, 0) AS saldo_inicial, ISNULL(TOTAL_RECAUDADO, 0) AS total_recaudado,
                           ISNULL(TOTAL_EJECUTADO, 0) AS total_ejecutado, ISNULL(SALDO_ACTUAL, 0) AS saldo_actual,
                           ACTUALIZADO AS actualizado
                    FROM CRD_CONVENIOS_RESERVAS
                    WHERE COD_CONVENIO = @Convenio
                    ORDER BY CORTE_INICIAL DESC;";
                return DbHelper.CreateOkResponse(connection.Query<CrConveniosReservaData>(sql, new { Convenio = convenio }).ToList());
            });
        }

        #endregion

        #region Consultas de scroll compartidas


        private static string ObtenerCodigoConvenioScroll(SqlConnection connection,int direccion, string codigo)
        {
            if (direccion != 1 && direccion != 2)
                return string.Empty;

            if (string.IsNullOrWhiteSpace(codigo))
            {
                if (direccion == 2)
                    return string.Empty;

                const string sqlPrimero = @"
            SELECT TOP 1 COD_CONVENIO
            FROM CRD_CONVENIOS
            ORDER BY COD_CONVENIO ASC;";

                return connection.QueryFirstOrDefault<string>(
                    sqlPrimero) ?? string.Empty;
            }

            const string sqlSiguiente = @"
        SELECT TOP 1 COD_CONVENIO
        FROM CRD_CONVENIOS
        WHERE COD_CONVENIO > @Codigo
        ORDER BY COD_CONVENIO ASC;";

            const string sqlAnterior = @"
        SELECT TOP 1 COD_CONVENIO
        FROM CRD_CONVENIOS
        WHERE COD_CONVENIO < @Codigo
        ORDER BY COD_CONVENIO DESC;";

            return ObtenerCodigoScroll(
                connection,
                direccion,
                codigo,
                sqlSiguiente,
                sqlAnterior);
        }

        #endregion
    }
}