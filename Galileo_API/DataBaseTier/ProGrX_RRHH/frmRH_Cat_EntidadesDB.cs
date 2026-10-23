using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_RRHH;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_RRHH
{
    public class FrmRHCatEntidadesDB
    {
        private const int ModuloRRHH = 23;

        private const string EstadoActivo = "A";
        private const string EstadoInactivo = "I";
        private const string EstadoSuspendido = "S";
        private const string TipoTransferencia = "TE";

        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly MRRHHDB _mRRHHDB;

        public FrmRHCatEntidadesDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
            _mRRHHDB = new MRRHHDB(config);
        }

        #region Helpers privados

        private static string NormalizarTexto(string? value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static string NormalizarUsuario(string? usuario)
        {
            return NormalizarTexto(usuario).ToUpperInvariant();
        }

        private static string NormalizarEstado(string? estado)
        {
            var value = NormalizarTexto(estado).ToUpperInvariant();

            return value switch
            {
                EstadoActivo => EstadoActivo,
                EstadoInactivo => EstadoInactivo,
                EstadoSuspendido => EstadoSuspendido,
                _ => string.Empty
            };
        }

        /// <summary>
        /// Normaliza el tipo de documento a su código interno.
        /// Acepta tanto el código como la descripción utilizada por RRHH.
        /// </summary>
        private string NormalizarTipoDocumento(string? tipoDocumento)
        {
            var value = NormalizarTexto(tipoDocumento);

            if (EsCodigoTipoDocumento(value))
            {
                return value.ToUpperInvariant();
            }

            return NormalizarTexto(_mRRHHDB.fxTipoDocumento(value))
                .ToUpperInvariant();
        }

        private static bool EsCodigoTipoDocumento(string value)
        {
            return value.ToUpperInvariant() switch
            {
                "CK" or "TE" or "EF" or "RE" or "ND" or "NC" or "OT" or "CD"
                    => true,
                _ => false
            };
        }

        private static bool ExisteEntidad(
            SqlConnection connection,
            string codEr,
            SqlTransaction? transaction = null)
        {
            const string sql = @"
                select count(1)
                from RH_ENTIDADES_RELACIONADAS
                where COD_ER = @codEr;";

            return connection.QuerySingle<int>(
                sql,
                new { codEr },
                transaction) > 0;
        }

        private static int ContarCuentasEntidad(
            SqlConnection connection,
            string identificacion,
            SqlTransaction? transaction = null)
        {
            const string sql = @"
                select count(1)
                from SYS_CUENTAS_BANCARIAS
                where IDENTIFICACION = @identificacion;";

            return connection.QuerySingle<int>(
                sql,
                new { identificacion },
                transaction);
        }

        /// <summary>
        /// Valida la información requerida antes de registrar o modificar
        /// una entidad relacionada.
        /// </summary>
        private static ErrorDto? ValidarEntidadGuardar(
            SqlConnection connection,
            RhCatEntidadData? entidad,
            string tipoDocumento,
            bool existe)
        {
            if (entidad == null)
            {
                return DbHelper.ErrorResponse(
                    "No se recibió información de la entidad.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(entidad.cod_er))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el código de la entidad.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(entidad.nombre))
            {
                return DbHelper.ErrorResponse(
                    "No se indicó el nombre de la entidad.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(entidad.identificacion))
            {
                return DbHelper.ErrorResponse(
                    "Ingrese una identificación válida.",
                    -2);
            }

            if (!entidad.cod_banco.HasValue ||
                entidad.cod_banco.Value <= 0)
            {
                return DbHelper.ErrorResponse(
                    "No se especificó una cuenta bancaria para desembolsos.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(
                NormalizarEstado(entidad.estado)))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar un estado válido para la entidad.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(tipoDocumento))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar una forma de pago válida.",
                    -2);
            }

            return ValidarCuentaTransferencia(
                connection,
                entidad,
                tipoDocumento,
                existe);
        }

        /// <summary>
        /// Conserva la validación del VB6: cuando la entidad ya existe
        /// y la forma de pago es transferencia debe existir al menos una cuenta.
        /// </summary>
        private static ErrorDto? ValidarCuentaTransferencia(
            SqlConnection connection,
            RhCatEntidadData entidad,
            string tipoDocumento,
            bool existe)
        {
            if (!existe ||
                !string.Equals(
                    tipoDocumento,
                    TipoTransferencia,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var identificacion =
                NormalizarTexto(entidad.identificacion);

            var cuentas = ContarCuentasEntidad(
                connection,
                identificacion);

            if (cuentas > 0)
            {
                return null;
            }

            return DbHelper.ErrorResponse(
                "No se especificó la cuenta para las transferencias.",
                -2);
        }

        private static object CrearParametrosEntidad(
            RhCatEntidadData entidad,
            string usuario,
            string estado,
            string tipoDocumento)
        {
            return new
            {
                codEr = NormalizarTexto(entidad.cod_er),
                identificacion = NormalizarTexto(entidad.identificacion),
                nombre = NormalizarTexto(entidad.nombre),
                nombreCorto = NormalizarTexto(entidad.nombre_corto),
                estado,
                asocSolidarista = entidad.asoc_solidarista,
                emitir = tipoDocumento,
                codBanco = entidad.cod_banco,
                contactoNombre = NormalizarTexto(entidad.contacto_nombre),
                contactoEmail = NormalizarTexto(entidad.contacto_email),
                contactoMovil = NormalizarTexto(entidad.contacto_movil),
                contactoTelefono = NormalizarTexto(entidad.contacto_telefono),
                usuario
            };
        }

        private static int InsertarEntidad(
            SqlConnection connection,
            RhCatEntidadData entidad,
            string usuario,
            string estado,
            string tipoDocumento,
            SqlTransaction transaction)
        {
            const string sql = @"
                insert into RH_ENTIDADES_RELACIONADAS
                (
                    COD_ER,
                    IDENTIFICACION,
                    NOMBRE,
                    NOMBRE_CORTO,
                    ESTADO,
                    ASOC_SOLIDARISTA,
                    EMITIR,
                    COD_BANCO,
                    CONTACTO_NOMBRE,
                    CONTACTO_EMAIL,
                    CONTACTO_MOVIL,
                    CONTACTO_TELEFONO,
                    REGISTRO_USUARIO,
                    REGISTRO_FECHA
                )
                values
                (
                    @codEr,
                    @identificacion,
                    @nombre,
                    @nombreCorto,
                    @estado,
                    @asocSolidarista,
                    @emitir,
                    @codBanco,
                    @contactoNombre,
                    @contactoEmail,
                    @contactoMovil,
                    @contactoTelefono,
                    @usuario,
                    dbo.MyGetdate()
                );";

            var parametros = CrearParametrosEntidad(
                entidad,
                usuario,
                estado,
                tipoDocumento);

            return connection.Execute(
                sql,
                parametros,
                transaction);
        }

        private static int ActualizarEntidad(
            SqlConnection connection,
            RhCatEntidadData entidad,
            string usuario,
            string estado,
            string tipoDocumento,
            SqlTransaction transaction)
        {
            const string sql = @"
                update RH_ENTIDADES_RELACIONADAS
                set
                    IDENTIFICACION = @identificacion,
                    NOMBRE = @nombre,
                    NOMBRE_CORTO = @nombreCorto,
                    ESTADO = @estado,
                    ASOC_SOLIDARISTA = @asocSolidarista,
                    EMITIR = @emitir,
                    COD_BANCO = @codBanco,
                    CONTACTO_NOMBRE = @contactoNombre,
                    CONTACTO_EMAIL = @contactoEmail,
                    CONTACTO_MOVIL = @contactoMovil,
                    CONTACTO_TELEFONO = @contactoTelefono
                where COD_ER = @codEr;";

            var parametros = CrearParametrosEntidad(
                entidad,
                usuario,
                estado,
                tipoDocumento);

            return connection.Execute(
                sql,
                parametros,
                transaction);
        }

        private ErrorDto RegistrarBitacoraEntidad(
            int CodEmpresa,
            string usuario,
            string codEr,
            bool modifica)
        {
            return Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = CodEmpresa,
                Usuario = usuario,
                DetalleMovimiento = $"Responsabilidad: {codEr}",
                Movimiento = modifica
                    ? "Modifica - WEB"
                    : "Registra - WEB",
                Modulo = ModuloRRHH
            });
        }

        #endregion

        #region Bitácora

        /// <summary>
        /// Registra movimientos realizados desde el módulo de Recursos Humanos.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return _securityMainDb.Bitacora(data);
        }

        #endregion

        #region Entidad relacionada

        /// <summary>
        /// Obtiene toda la información de una entidad relacionada por código,
        /// incluyendo la descripción del banco configurado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<RhCatEntidadData> RH_CatEntidades_Obtener(
            int CodEmpresa,
            RhCatEntidadObtenerRequest? request)
        {
            try
            {
                var codEr = NormalizarTexto(request?.cod_er);

                if (string.IsNullOrWhiteSpace(codEr))
                {
                    return DbHelper.CreateErrorResponse<RhCatEntidadData>(
                        "Debe indicar el código de la entidad.",
                        -2,
                        new RhCatEntidadData());
                }

                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
                    select
                        rtrim(E.COD_ER) as cod_er,
                        isnull(rtrim(E.IDENTIFICACION), '') as identificacion,
                        isnull(rtrim(E.NOMBRE), '') as nombre,
                        isnull(rtrim(E.NOMBRE_CORTO), '') as nombre_corto,
                        isnull(rtrim(E.ESTADO), '') as estado,
                        cast(isnull(E.ASOC_SOLIDARISTA, 0) as bit)
                            as asoc_solidarista,
                        isnull(rtrim(E.EMITIR), '') as emitir,
                        E.COD_BANCO as cod_banco,
                        isnull(rtrim(B.DESCRIPCION), '') as banco,
                        isnull(rtrim(E.CONTACTO_NOMBRE), '')
                            as contacto_nombre,
                        isnull(rtrim(E.CONTACTO_EMAIL), '')
                            as contacto_email,
                        isnull(rtrim(E.CONTACTO_MOVIL), '')
                            as contacto_movil,
                        isnull(rtrim(E.CONTACTO_TELEFONO), '')
                            as contacto_telefono,
                        E.REGISTRO_FECHA as registro_fecha,
                        isnull(rtrim(E.REGISTRO_USUARIO), '')
                            as registro_usuario
                    from RH_ENTIDADES_RELACIONADAS E
                    left join TES_BANCOS B
                        on E.COD_BANCO = B.ID_BANCO
                    where E.COD_ER = @codEr;";

                var entidad = connection
                    .QueryFirstOrDefault<RhCatEntidadData>(
                        sql,
                        new { codEr });

                if (entidad == null)
                {
                    return DbHelper.CreateErrorResponse<RhCatEntidadData>(
                        "No se encontró el registro.",
                        -2,
                        new RhCatEntidadData());
                }

                return DbHelper.CreateOkResponse(entidad);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<RhCatEntidadData>(
                    ex.Message,
                    -1,
                    new RhCatEntidadData());
            }
        }

        /// <summary>
        /// Obtiene la entidad anterior o siguiente.
        /// La navegación no es circular una vez existe una entidad seleccionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<RhCatEntidadData>RH_CatEntidades_Scroll_Obtener(int CodEmpresa,RhCatEntidadScrollRequest? request)
        {
            try
            {
                var codEr =
                    NormalizarTexto(request?.cod_er);

                var direccion =
                    request?.direccion == 0
                        ? 0
                        : 1;

                using var connection =
                    DbHelper.OpenConnection(
                        _portalDB,
                        CodEmpresa);

                var codigo =
                    ObtenerCodigoScroll(
                        connection,
                        codEr,
                        direccion);

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    return DbHelper.CreateErrorResponse<RhCatEntidadData>(
                        "No hay más entidades para consultar.",
                        -2,
                        new RhCatEntidadData());
                }

                return ObtenerEntidadDesdeConexion(
                    connection,
                    codigo);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<RhCatEntidadData>(
                    ex.Message,
                    -1,
                    new RhCatEntidadData());
            }
        }

        /// <summary>
        /// Obtiene el catálogo completo de entidades para el buscador F4.
        /// El filtrado adicional, ordenamiento y paginación visual se manejan
        /// en frontend.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<RhCatEntidadBusquedaData>>
            RH_CatEntidades_Busqueda_Obtener(
                int CodEmpresa,
                RhCatEntidadBusquedaRequest? request)
        {
            try
            {
                var texto = NormalizarTexto(request?.texto);
                var like = $"%{texto}%";

                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
                    select
                        rtrim(COD_ER) as cod_er,
                        isnull(rtrim(IDENTIFICACION), '') as identificacion,
                        isnull(rtrim(NOMBRE), '') as nombre
                    from RH_ENTIDADES_RELACIONADAS
                    where
                        @texto = ''
                        or COD_ER like @like
                        or IDENTIFICACION like @like
                        or NOMBRE like @like
                    order by COD_ER;";

                var lista = connection
                    .Query<RhCatEntidadBusquedaData>(
                        sql,
                        new
                        {
                            texto,
                            like
                        })
                    .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<
                    List<RhCatEntidadBusquedaData>>(
                    ex.Message,
                    -1,
                    new List<RhCatEntidadBusquedaData>());
            }
        }

        /// <summary>
        /// Registra o modifica una entidad relacionada.
        /// La operación se determina por la existencia real del código,
        /// conservando el comportamiento de frmRH_Cat_Entidades en VB6.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_CatEntidades_Guardar(
      int CodEmpresa,
      string? usuario,
      RhCatEntidadGuardarRequest? request)
        {
            var usuarioNormalizado =
                NormalizarUsuario(usuario);

            if (string.IsNullOrWhiteSpace(
                usuarioNormalizado))
            {
                return DbHelper.ErrorResponse(
                    "No se indicó el usuario de sesión.",
                    -2);
            }

            if (request?.entidad == null)
            {
                return DbHelper.ErrorResponse(
                    "No se recibió información de la entidad.",
                    -2);
            }

            try
            {
                var entidad = request.entidad;

                NormalizarEntidad(entidad);

                var tipoDocumento =
                    NormalizarTipoDocumento(
                        entidad.emitir);

                var estado =
                    NormalizarEstado(
                        entidad.estado);

                bool existe;

                using (
                    var connection =
                        DbHelper.OpenConnection(
                            _portalDB,
                            CodEmpresa)
                )
                {
                    connection.Open();

                    existe = ExisteEntidad(
                        connection,
                        entidad.cod_er);

                    var validacion =
                        ValidarEntidadGuardar(
                            connection,
                            entidad,
                            tipoDocumento,
                            existe);

                    if (validacion != null)
                    {
                        return validacion;
                    }

                    using var transaction =
                        connection.BeginTransaction();

                    var filas = GuardarEntidad(
                        connection,
                        transaction,
                        entidad,
                        usuarioNormalizado,
                        estado,
                        tipoDocumento,
                        existe);

                    if (filas <= 0)
                    {
                        transaction.Rollback();

                        return DbHelper.ErrorResponse(
                            "No se guardó ningún registro.",
                            -2);
                    }

                    transaction.Commit();
                }

                var bitacora =
                    RegistrarBitacoraEntidad(
                        CodEmpresa,
                        usuarioNormalizado,
                        entidad.cod_er,
                        existe);

                if (bitacora.Code < 0)
                {
                    return DbHelper.ErrorResponse(
                        bitacora.Description ??
                        "No fue posible registrar la bitácora.");
                }

                return DbHelper.OkResponse(
                    "Información guardada satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }
        }

        private static int GuardarEntidad(SqlConnection connection, SqlTransaction transaction,  RhCatEntidadData entidad,string usuario,string estado, string tipoDocumento,bool existe)
        {
            return existe
                ? ActualizarEntidad(
                    connection,
                    entidad,
                    usuario,
                    estado,
                    tipoDocumento,
                    transaction)
                : InsertarEntidad(
                    connection,
                    entidad,
                    usuario,
                    estado,
                    tipoDocumento,
                    transaction);
        }

        private static void NormalizarEntidad(
            RhCatEntidadData entidad)
        {
            entidad.cod_er =
                NormalizarTexto(entidad.cod_er);

            entidad.identificacion =
                NormalizarTexto(entidad.identificacion);

            entidad.nombre =
                NormalizarTexto(entidad.nombre);

            entidad.nombre_corto =
                NormalizarTexto(entidad.nombre_corto);

            entidad.estado =
                NormalizarTexto(entidad.estado);

            entidad.emitir =
                NormalizarTexto(entidad.emitir);

            entidad.contacto_nombre =
                NormalizarTexto(entidad.contacto_nombre);

            entidad.contacto_email =
                NormalizarTexto(entidad.contacto_email);

            entidad.contacto_movil =
                NormalizarTexto(entidad.contacto_movil);

            entidad.contacto_telefono =
                NormalizarTexto(entidad.contacto_telefono);
        }
        /// <summary>
        /// Obtiene el código anterior o siguiente.
        /// La navegación no es circular.
        /// Si no existe una entidad cargada, únicamente permite
        /// iniciar la navegación hacia adelante desde el primer registro.
        /// </summary>
        private static string ObtenerCodigoScroll(SqlConnection connection,string codEr,int direccion)
        {
            if (string.IsNullOrWhiteSpace(codEr))
            {
                return direccion == 1
                    ? ObtenerPrimerCodigo(connection)
                    : string.Empty;
            }

            const string sqlAnterior = @"
        select top 1 rtrim(COD_ER)
        from RH_ENTIDADES_RELACIONADAS
        where COD_ER < @codEr
        order by COD_ER desc;";

            const string sqlSiguiente = @"
        select top 1 rtrim(COD_ER)
        from RH_ENTIDADES_RELACIONADAS
        where COD_ER > @codEr
        order by COD_ER asc;";

            var sql = direccion == 0
                ? sqlAnterior
                : sqlSiguiente;

            return connection.QueryFirstOrDefault<string>(
                       sql,
                       new
                       {
                           codEr
                       })
                   ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el primer código disponible para iniciar
        /// la navegación desde una pantalla sin entidad cargada.
        /// </summary>
        private static string ObtenerPrimerCodigo(SqlConnection connection)
        {
            const string sql = @"
        select top 1 rtrim(COD_ER)
        from RH_ENTIDADES_RELACIONADAS
        order by COD_ER asc;";

            return connection.QueryFirstOrDefault<string>(sql)
                   ?? string.Empty;
        }
        private static ErrorDto<RhCatEntidadData>
            ObtenerEntidadDesdeConexion(
                SqlConnection connection,
                string codEr)
        {
            const string sql = @"
                select
                    rtrim(E.COD_ER) as cod_er,
                    isnull(rtrim(E.IDENTIFICACION), '') as identificacion,
                    isnull(rtrim(E.NOMBRE), '') as nombre,
                    isnull(rtrim(E.NOMBRE_CORTO), '') as nombre_corto,
                    isnull(rtrim(E.ESTADO), '') as estado,
                    cast(isnull(E.ASOC_SOLIDARISTA, 0) as bit)
                        as asoc_solidarista,
                    isnull(rtrim(E.EMITIR), '') as emitir,
                    E.COD_BANCO as cod_banco,
                    isnull(rtrim(B.DESCRIPCION), '') as banco,
                    isnull(rtrim(E.CONTACTO_NOMBRE), '')
                        as contacto_nombre,
                    isnull(rtrim(E.CONTACTO_EMAIL), '')
                        as contacto_email,
                    isnull(rtrim(E.CONTACTO_MOVIL), '')
                        as contacto_movil,
                    isnull(rtrim(E.CONTACTO_TELEFONO), '')
                        as contacto_telefono,
                    E.REGISTRO_FECHA as registro_fecha,
                    isnull(rtrim(E.REGISTRO_USUARIO), '')
                        as registro_usuario
                from RH_ENTIDADES_RELACIONADAS E
                left join TES_BANCOS B
                    on E.COD_BANCO = B.ID_BANCO
                where E.COD_ER = @codEr;";

            var entidad = connection
                .QueryFirstOrDefault<RhCatEntidadData>(
                    sql,
                    new { codEr });

            if (entidad == null)
            {
                return DbHelper.CreateErrorResponse<RhCatEntidadData>(
                    "No se encontró el registro.",
                    -2,
                    new RhCatEntidadData());
            }

            return DbHelper.CreateOkResponse(entidad);
        }

        #endregion

        #region Bancos

        /// <summary>
        /// Obtiene los bancos disponibles para el usuario utilizando
        /// spCrd_SGT_Bancos y normaliza el resultado para el selector.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<RhCatEntidadBancoData>>
            RH_CatEntidades_Bancos_Obtener(
                int CodEmpresa,
                string? usuario)
        {
            try
            {
                var usuarioNormalizado =
                    NormalizarUsuario(usuario);

                if (string.IsNullOrWhiteSpace(usuarioNormalizado))
                {
                    return DbHelper.CreateErrorResponse<
                        List<RhCatEntidadBancoData>>(
                        "Debe indicar el usuario.",
                        -2,
                        new List<RhCatEntidadBancoData>());
                }

                using var connection =
                    DbHelper.OpenConnection(
                        _portalDB,
                        CodEmpresa);

                var resultadoSp = connection
                    .Query<BancoSgtData>(
                        "spCrd_SGT_Bancos",
                        new
                        {
                            Usuario = usuarioNormalizado,
                            Divisa = (string?)null
                        },
                        commandType: CommandType.StoredProcedure)
                    .ToList();

                var lista = resultadoSp
                    .Select(item => new RhCatEntidadBancoData
                    {
                        id_banco = item.ID_BANCO,

                        descripcion =
                            NormalizarTexto(
                                item.DESCRIPCION),

                        item_descripcion =
                            NormalizarTexto(
                                item.ItmX)
                    })
                    .ToList();

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<
                    List<RhCatEntidadBancoData>>(
                    ex.Message,
                    -1,
                    new List<RhCatEntidadBancoData>());
            }
        }
        private sealed class BancoSgtData
        {
            public int ID_BANCO { get; init; }

            public string? DESCRIPCION { get; init; }

            public string? DESC_CORTA { get; init; }

            public string? CTA { get; init; }

            public string? COD_DIVISA { get; init; }

            public int IdX { get; init; }

            public string? ItmX { get; init; }
        }
        #endregion

        #region Cuentas bancarias

        /// <summary>
        /// Obtiene todas las cuentas bancarias vinculadas a una identificación.
        /// La paginación, filtrado y ordenamiento se realizan en frontend.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        public ErrorDto<RhCatEntidadCuentasLista>
            RH_CatEntidades_Cuentas_Obtener(
                int CodEmpresa,
                string? identificacion)
        {
            var resultado = new RhCatEntidadCuentasLista();

            try
            {
                var identificacionNormalizada =
                    NormalizarTexto(identificacion);

                if (string.IsNullOrWhiteSpace(
                    identificacionNormalizada))
                {
                    return DbHelper.CreateOkResponse(resultado);
                }

                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
                    select
                        rtrim(C.CUENTA_INTERNA) as cuenta_interna,
                        isnull(rtrim(B.DESCRIPCION), '') as banco,
                        case
                            when C.TIPO = 'A' then 'Ahorros'
                            else 'Corriente'
                        end as tipo,
                        isnull(rtrim(C.COD_DIVISA), '') as cod_divisa,
                        cast(
                            isnull(C.CUENTA_INTERBANCA, 0)
                            as bit
                        ) as cuenta_interbanca,
                        isnull(rtrim(C.DESTINO), '') as destino,
                        cast(isnull(C.ACTIVA, 0) as bit) as activa,
                        C.REGISTRO_FECHA as registro_fecha,
                        isnull(rtrim(C.REGISTRO_USUARIO), '')
                            as registro_usuario
                    from SYS_CUENTAS_BANCARIAS C
                    inner join TES_BANCOS_GRUPOS B
                        on C.COD_BANCO = B.COD_GRUPO
                    where C.IDENTIFICACION = @identificacion
                    order by C.CUENTA_INTERNA;";

                var lista = connection
                    .Query<RhCatEntidadCuentaData>(
                        sql,
                        new
                        {
                            identificacion =
                                identificacionNormalizada
                        })
                    .ToList();

                resultado.lista = lista;
                resultado.total = lista.Count;

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<
                    RhCatEntidadCuentasLista>(
                    ex.Message,
                    -1,
                    resultado);
            }
        }

        #endregion

        #region Conceptos

        /// <summary>
        /// Obtiene todos los conceptos activos asignados a la entidad
        /// y todos los conceptos activos que actualmente se encuentran libres.
        /// La tabla se entrega completa para paginación, filtro y sort en FE.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codEr"></param>
        /// <returns></returns>
        public ErrorDto<RhCatEntidadConceptosLista>
            RH_CatEntidades_Conceptos_Obtener(
                int CodEmpresa,
                string? codEr)
        {
            var resultado =
                new RhCatEntidadConceptosLista();

            try
            {
                var codigo = NormalizarTexto(codEr);

                if (string.IsNullOrWhiteSpace(codigo))
                {
                    return DbHelper.CreateOkResponse(resultado);
                }

                using var connection =
                    DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
                    select
                        rtrim(COD_CONCEPTO) as cod_concepto,
                        isnull(rtrim(DESCRIPCION), '') as descripcion,
                        cast(
                            case
                                when COD_ER = @codEr then 1
                                else 0
                            end
                            as bit
                        ) as asignado
                    from RH_CONCEPTOS
                    where ACTIVO = 1
                      and
                      (
                          COD_ER = @codEr
                          or COD_ER is null
                      )
                    order by
                        case
                            when COD_ER = @codEr then 0
                            else 1
                        end,
                        COD_CONCEPTO;";

                var lista = connection
                    .Query<RhCatEntidadConceptoData>(
                        sql,
                        new { codEr = codigo })
                    .ToList();

                resultado.lista = lista;
                resultado.total = lista.Count;

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<
                    RhCatEntidadConceptosLista>(
                    ex.Message,
                    -1,
                    resultado);
            }
        }

        /// <summary>
        /// Asigna o desasigna inmediatamente un concepto de nómina
        /// a una entidad relacionada, conservando el comportamiento
        /// original de frmRH_Cat_Entidades.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto RH_CatEntidades_Concepto_Actualizar(
    int CodEmpresa,
    string? usuario,
    RhCatEntidadConceptoActualizarRequest? request)
        {
            var validacion =
                ValidarConceptoActualizar(request);

            if (validacion != null)
            {
                return validacion;
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDB,
                        CodEmpresa);

                var codEr =
                    NormalizarTexto(request!.cod_er);

                var codConcepto =
                    NormalizarTexto(request.cod_concepto);

                bool asignar = request.asignar == true;

                var filas = ActualizarConcepto(
                    connection,
                    codEr,
                    codConcepto,
                    asignar);

                if (filas <= 0)
                {
                    return DbHelper.ErrorResponse(
                        "No fue posible actualizar el concepto.",
                        -2);
                }

                return DbHelper.OkResponse(
                    asignar
                        ? "Concepto asignado correctamente."
                        : "Concepto desasignado correctamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }
        }

        private static ErrorDto?ValidarConceptoActualizar(RhCatEntidadConceptoActualizarRequest? request)
        {
            if (request == null)
            {
                return DbHelper.ErrorResponse(
                    "No se recibió información del concepto.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.cod_er))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar la entidad.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(
                request.cod_concepto))
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar el concepto.",
                    -2);
            }

            return null;
        }

        private static int ActualizarConcepto(SqlConnection connection, string codEr,string codConcepto, bool asignar)
        {
            const string sqlAsignar = @"
        update RH_CONCEPTOS
        set COD_ER = @codEr
        where COD_CONCEPTO = @codConcepto;";

            const string sqlDesasignar = @"
        update RH_CONCEPTOS
        set COD_ER = null
        where COD_CONCEPTO = @codConcepto;";

            return connection.Execute(
                asignar
                    ? sqlAsignar
                    : sqlDesasignar,
                new
                {
                    codEr,
                    codConcepto
                });
        }

        #endregion
    }
}