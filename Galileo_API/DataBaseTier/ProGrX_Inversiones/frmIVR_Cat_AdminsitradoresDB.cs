using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed partial class FrmIvrCatAdminsitradoresDb
    {
        private const int CodigoMensajeControlado = -2;
        private const int ModuloInversiones = 22;

        private const string MensajeNoEncontrado =
            "No se encontr&oacute; el administrador indicado.";

        private const string MensajeCuentaInvalida =
            "No se especific&oacute; una cuenta contable v&aacute;lida.";

        private readonly PortalDB _portalDb;
        private readonly MCntLinkDB _cntLinkDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmIvrCatAdminsitradoresDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _cntLinkDb = new MCntLinkDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los catálogos requeridos por el formulario
        /// de administradores.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <returns>
        /// Bancos, tipos de administrador, estados y tipos
        /// de documento.
        /// </returns>
        public ErrorDto<IvrCatAdministradorInicialData>
            IVR_Cat_Adminsitradores_Inicializar(
                int codEmpresa)
        {
            const string sqlBancos = """
                EXEC spIVR_Bancos_Autorizados;
                """;

            const string sqlTipos = """
                SELECT
                    RTRIM(TIPO_ADMINISTRADOR) AS item,
                    RTRIM(ISNULL(DESCRIPCION, ''))
                        AS descripcion
                FROM IVR_ADMINISTRADOR_TIPOS
                ORDER BY DESCRIPCION;
                """;

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                var bancosSp = connection
                    .Query<IvrCatAdministradorBancoSpData>(
                        sqlBancos)
                    .ToList();

                var tipos = connection
                    .Query<DropDownListaGenericaModel<string>>(
                        sqlTipos)
                    .ToList();

                var resultado =
                    new IvrCatAdministradorInicialData
                    {
                        bancos = bancosSp
                            .Select(item =>
                                new DropDownListaGenericaModel<int>
                                {
                                    item = item.idx,
                                    descripcion =
                                        item.itmx.Trim()
                                })
                            .ToList(),
                        tipos_administrador = tipos,
                        estados =
                        [
                            new()
                            {
                                item = "A",
                                descripcion = "Activo"
                            },
                            new()
                            {
                                item = "I",
                                descripcion = "Inactivo"
                            }
                        ],
                        tipos_documento =
                        [
                            new()
                            {
                                item = "CK",
                                descripcion =
                                    MTesFuncionesDb
                                        .fxTipoDocumento("CK")
                            },
                            new()
                            {
                                item = "TE",
                                descripcion =
                                    MTesFuncionesDb
                                        .fxTipoDocumento("TE")
                            },
                            new()
                            {
                                item = "ND",
                                descripcion =
                                    MTesFuncionesDb
                                        .fxTipoDocumento("ND")
                            }
                        ]
                    };

                return DbHelper.CreateOkResponse(resultado);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse(
                    ex.Message,
                    result:
                        new IvrCatAdministradorInicialData());
            }
        }

        /// <summary>
        /// Busca administradores por código, identificación
        /// o descripción.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="filtro">
        /// Texto utilizado como filtro.
        /// </param>
        /// <returns>
        /// Lista de administradores encontrados.
        /// </returns>
        public ErrorDto<List<IvrCatAdministradorBusquedaData>>
            IVR_Cat_Adminsitradores_Buscar(
                int codEmpresa,
                string? filtro)
        {
            const string sql = """
                SELECT
                    RTRIM(ISNULL(COD_ADMINISTRADOR, ''))
                        AS cod_administrador,
                    RTRIM(ISNULL(IDENTIFICACION, ''))
                        AS identificacion,
                    RTRIM(ISNULL(DESCRIPCION, ''))
                        AS descripcion
                FROM IVR_ADMINISTRADOR
                WHERE @filtro = ''
                   OR COD_ADMINISTRADOR LIKE '%' + @filtro + '%'
                   OR IDENTIFICACION LIKE '%' + @filtro + '%'
                   OR DESCRIPCION LIKE '%' + @filtro + '%'
                ORDER BY COD_ADMINISTRADOR;
                """;

            return DbHelper.ExecuteListQuery
                <IvrCatAdministradorBusquedaData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        filtro = filtro?.Trim() ?? string.Empty
                    });
        }

        /// <summary>
        /// Obtiene la información general de un administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codAdministrador">
        /// Código del administrador.
        /// </param>
        /// <returns>
        /// Administrador encontrado.
        /// </returns>
        public ErrorDto<IvrCatAdministradorData?>
            IVR_Cat_Adminsitradores_Obtener(
                int codEmpresa,
                string? codAdministrador)
        {
            var codigo = codAdministrador?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorData?>(
                        "El c&oacute;digo del administrador "
                        + "es requerido.",
                        CodigoMensajeControlado);
            }

            const string sql = """
                SELECT TOP (1)
                    RTRIM(ISNULL(COD_ADMINISTRADOR, ''))
                        AS cod_administrador,
                    RTRIM(ISNULL(DESCRIPCION, ''))
                        AS descripcion,
                    RTRIM(ISNULL(IDENTIFICACION, ''))
                        AS identificacion,
                    RTRIM(ISNULL(TIPO_ADMINISTRADOR, ''))
                        AS tipo_administrador,
                    RTRIM(ISNULL(TIPO_DESC, ''))
                        AS tipo_desc,
                    RTRIM(ISNULL(ESTADO, ''))
                        AS estado,
                    RTRIM(ISNULL(DIRECCION, ''))
                        AS direccion,
                    RTRIM(ISNULL(APTO_POSTAL, ''))
                        AS apto_postal,
                    RTRIM(ISNULL(WEBSITE, ''))
                        AS website,
                    RTRIM(ISNULL(EMAIL_01, ''))
                        AS email_01,
                    RTRIM(ISNULL(EMAIL_02, ''))
                        AS email_02,
                    RTRIM(ISNULL(TELEFONO1, ''))
                        AS telefono1,
                    RTRIM(ISNULL(TELEFONO2, ''))
                        AS telefono2,
                    RTRIM(ISNULL(FAX, ''))
                        AS fax,
                    RTRIM(ISNULL(COD_CUENTA_TRANSITO, ''))
                        AS cod_cuenta_transito,
                    RTRIM(ISNULL(
                        COD_CUENTA_TRANSITO_MASK,
                        ''))
                        AS cod_cuenta_transito_mask,
                    RTRIM(ISNULL(
                        COD_CUENTA_TRANSITO_DESC,
                        ''))
                        AS cod_cuenta_transito_desc,
                    RTRIM(ISNULL(TES_EMITIR, ''))
                        AS tes_emitir,
                    ISNULL(TES_BANCO, 0)
                        AS tes_banco,
                    RTRIM(ISNULL(BANCO_DESC, ''))
                        AS banco_desc
                FROM vIVR_ADMINISTRADORES
                WHERE COD_ADMINISTRADOR =
                    @cod_administrador;
                """;

            var response =
                DbHelper.ExecuteSingleQuery
                    <IvrCatAdministradorData>(
                        _portalDb,
                        codEmpresa,
                        sql,
                        defaultValue: null,
                        parameters: new
                        {
                            cod_administrador = codigo
                        });

            if (response.Code != 0)
            {
                return response;
            }

            return response.Result is null
                ? DbHelper.CreateErrorResponse
                    <IvrCatAdministradorData?>(
                        MensajeNoEncontrado,
                        CodigoMensajeControlado)
                : response;
        }

        /// <summary>
        /// Obtiene el administrador anterior o siguiente
        /// tomando como referencia el código actual.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codAdministrador">
        /// Código actual. Cuando está vacío, obtiene el
        /// primero o el último según la dirección solicitada.
        /// </param>
        /// <param name="siguiente">
        /// True para siguiente; false para anterior.
        /// </param>
        /// <returns>
        /// Código encontrado o null cuando no existe otro registro.
        /// </returns>
        public ErrorDto<IvrCatAdministradorNavegacionData?>
            IVR_Cat_Adminsitradores_Navegar(
                int codEmpresa,
                string? codAdministrador,
                bool siguiente)
        {
            const string sqlSiguiente = """
                SELECT TOP (1)
                    RTRIM(COD_ADMINISTRADOR)
                        AS cod_administrador
                FROM IVR_ADMINISTRADOR
                WHERE @cod_administrador = ''
                   OR COD_ADMINISTRADOR >
                        @cod_administrador
                ORDER BY COD_ADMINISTRADOR ASC;
                """;

            const string sqlAnterior = """
                SELECT TOP (1)
                    RTRIM(COD_ADMINISTRADOR)
                        AS cod_administrador
                FROM IVR_ADMINISTRADOR
                WHERE @cod_administrador = ''
                   OR COD_ADMINISTRADOR <
                        @cod_administrador
                ORDER BY COD_ADMINISTRADOR DESC;
                """;

            return DbHelper.ExecuteSingleQuery
                <IvrCatAdministradorNavegacionData>(
                    _portalDb,
                    codEmpresa,
                    siguiente
                        ? sqlSiguiente
                        : sqlAnterior,
                    defaultValue: null,
                    parameters: new
                    {
                        cod_administrador =
                            codAdministrador?.Trim() ??
                            string.Empty
                    });
        }

        /// <summary>
        /// Valida una cuenta contable y devuelve su máscara
        /// y descripción.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="cuenta">
        /// Cuenta recibida desde el formulario.
        /// </param>
        /// <returns>
        /// Resultado de la validación.
        /// </returns>
        public ErrorDto<IvrCatAdministradorCuentaContableData>
            IVR_Cat_Adminsitradores_Cuenta_Validar(
                int codEmpresa,
                string? cuenta)
        {
            var validacion =
                IVR_Cat_Adminsitradores_Cuenta_Normalizar(
                    codEmpresa,
                    cuenta);

            var resultado =
                new IvrCatAdministradorCuentaContableData
                {
                    es_valida = validacion.es_valida,
                    cod_cuenta = validacion.cuenta,
                    cod_cuenta_mask =
                        validacion.es_valida
                            ? _cntLinkDb.fxgCntCuentaFormato(
                                codEmpresa,
                                true,
                                validacion.cuenta,
                                0)
                            : string.Empty,
                    descripcion =
                        validacion.es_valida
                            ? _cntLinkDb.fxgCntCuentaDesc(
                                codEmpresa,
                                validacion.cuenta) ??
                              string.Empty
                            : string.Empty
                };

            return validacion.es_valida
                ? DbHelper.CreateOkResponse(resultado)
                : DbHelper.CreateErrorResponse(
                    validacion.mensaje,
                    CodigoMensajeControlado,
                    resultado);
        }

        /// <summary>
        /// Registra un administrador nuevo.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="request">
        /// Información del administrador.
        /// </param>
        /// <returns>
        /// Administrador registrado.
        /// </returns>
        public ErrorDto<IvrCatAdministradorData?>
            IVR_Cat_Adminsitradores_Registrar(
                int codEmpresa,
                IvrCatAdministradorGuardarRequest? request)
        {
            var validacion =
                IVR_Cat_Adminsitradores_Request_Validar(
                    codEmpresa,
                    request);

            if (!string.IsNullOrEmpty(validacion.mensaje))
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorData?>(
                        validacion.mensaje,
                        CodigoMensajeControlado);
            }

            const string sqlExiste = """
                SELECT COUNT(1)
                FROM IVR_ADMINISTRADOR
                WHERE COD_ADMINISTRADOR =
                    @cod_administrador;
                """;

            const string sqlInsertar = """
                INSERT INTO IVR_ADMINISTRADOR
                (
                    COD_ADMINISTRADOR,
                    TIPO_ADMINISTRADOR,
                    IDENTIFICACION,
                    DESCRIPCION,
                    ESTADO,
                    TELEFONO1,
                    TELEFONO2,
                    FAX,
                    EMAIL_01,
                    EMAIL_02,
                    APTO_POSTAL,
                    WEBSITE,
                    DIRECCION,
                    COD_CUENTA_TRANSITO,
                    TES_EMITIR,
                    TES_BANCO,
                    REGISTRO_FECHA,
                    REGISTRO_USUARIO
                )
                VALUES
                (
                    @cod_administrador,
                    @tipo_administrador,
                    @identificacion,
                    @descripcion,
                    @estado,
                    @telefono1,
                    @telefono2,
                    @fax,
                    @email_01,
                    @email_02,
                    @apto_postal,
                    @website,
                    @direccion,
                    @cod_cuenta_transito,
                    @tes_emitir,
                    @tes_banco,
                    dbo.MyGetdate(),
                    @usuario
                );
                """;

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                var parametros =
                    IVR_Cat_Adminsitradores_Parametros_Crear(
                        request!,
                        validacion.cuenta);

                var existe = connection.ExecuteScalar<int>(
                    sqlExiste,
                    parametros);

                if (existe > 0)
                {
                    return DbHelper.CreateErrorResponse
                        <IvrCatAdministradorData?>(
                            "El administrador indicado ya existe.",
                            CodigoMensajeControlado);
                }

                if (IVR_Cat_Adminsitradores_Identificacion_Duplicada(
                        connection,
                        request!.identificacion,
                        request.cod_administrador))
                {
                    return DbHelper.CreateErrorResponse
                        <IvrCatAdministradorData?>(
                            "Existe un administrador registrado "
                            + "con la misma identificaci&oacute;n.",
                            CodigoMensajeControlado);
                }

                _ = connection.Execute(
                    sqlInsertar,
                    parametros);

                IVR_Cat_Adminsitradores_Bitacora_Registrar(
                    codEmpresa,
                    request.usuario,
                    "Registra",
                    request.cod_administrador);

                return IVR_Cat_Adminsitradores_Obtener(
                    codEmpresa,
                    request.cod_administrador);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorData?>(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un administrador existente.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="request">
        /// Información del administrador.
        /// </param>
        /// <returns>
        /// Administrador actualizado.
        /// </returns>
        public ErrorDto<IvrCatAdministradorData?>
            IVR_Cat_Adminsitradores_Actualizar(
                int codEmpresa,
                IvrCatAdministradorGuardarRequest? request)
        {
            var validacion =
                IVR_Cat_Adminsitradores_Request_Validar(
                    codEmpresa,
                    request);

            if (!string.IsNullOrEmpty(validacion.mensaje))
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorData?>(
                        validacion.mensaje,
                        CodigoMensajeControlado);
            }

            const string sql = """
                UPDATE IVR_ADMINISTRADOR
                SET TIPO_ADMINISTRADOR =
                        @tipo_administrador,
                    IDENTIFICACION =
                        @identificacion,
                    DESCRIPCION =
                        @descripcion,
                    ESTADO =
                        @estado,
                    TELEFONO1 =
                        @telefono1,
                    TELEFONO2 =
                        @telefono2,
                    FAX =
                        @fax,
                    EMAIL_01 =
                        @email_01,
                    EMAIL_02 =
                        @email_02,
                    APTO_POSTAL =
                        @apto_postal,
                    WEBSITE =
                        @website,
                    DIRECCION =
                        @direccion,
                    COD_CUENTA_TRANSITO =
                        @cod_cuenta_transito,
                    TES_EMITIR =
                        @tes_emitir,
                    TES_BANCO =
                        @tes_banco
                WHERE COD_ADMINISTRADOR =
                    @cod_administrador;
                """;

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                if (IVR_Cat_Adminsitradores_Identificacion_Duplicada(
                        connection,
                        request!.identificacion,
                        request.cod_administrador))
                {
                    return DbHelper.CreateErrorResponse
                        <IvrCatAdministradorData?>(
                            "Existe un administrador registrado "
                            + "con la misma identificaci&oacute;n.",
                            CodigoMensajeControlado);
                }

                var parametros =
                    IVR_Cat_Adminsitradores_Parametros_Crear(
                        request,
                        validacion.cuenta);

                var afectados = connection.Execute(
                    sql,
                    parametros);

                if (afectados == 0)
                {
                    return DbHelper.CreateErrorResponse
                        <IvrCatAdministradorData?>(
                            MensajeNoEncontrado,
                            CodigoMensajeControlado);
                }

                IVR_Cat_Adminsitradores_Bitacora_Registrar(
                    codEmpresa,
                    request.usuario,
                    "Modifica",
                    request.cod_administrador);

                return IVR_Cat_Adminsitradores_Obtener(
                    codEmpresa,
                    request.cod_administrador);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorData?>(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codAdministrador">
        /// Código del administrador.
        /// </param>
        /// <param name="usuario">
        /// Usuario que realiza la eliminación.
        /// </param>
        /// <returns>
        /// Resultado de la eliminación.
        /// </returns>
        public ErrorDto IVR_Cat_Adminsitradores_Eliminar(
            int codEmpresa,
            string? codAdministrador,
            string? usuario)
        {
            var codigo = codAdministrador?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo del administrador "
                    + "es requerido.",
                    CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoMensajeControlado);
            }

            const string sql = """
                DELETE FROM IVR_ADMINISTRADOR
                WHERE COD_ADMINISTRADOR =
                    @cod_administrador;
                """;

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                var afectados = connection.Execute(
                    sql,
                    new
                    {
                        cod_administrador = codigo
                    });

                if (afectados == 0)
                {
                    return DbHelper.ErrorResponse(
                        MensajeNoEncontrado,
                        CodigoMensajeControlado);
                }

                IVR_Cat_Adminsitradores_Bitacora_Registrar(
                    codEmpresa,
                    usuario,
                    "Elimina",
                    codigo);

                return DbHelper.OkResponse(
                    "Administrador eliminado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

    }
}
