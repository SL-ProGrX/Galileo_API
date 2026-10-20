using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed partial class FrmIvrCatAdminsitradoresDb
    {
        /// <summary>
        /// Obtiene las cuentas bancarias relacionadas
        /// con la identificación del administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="identificacion">
        /// Identificación del administrador.
        /// </param>
        /// <returns>
        /// Cuentas bancarias encontradas.
        /// </returns>
        public ErrorDto
            <List<IvrCatAdministradorCuentaBancariaData>>
            IVR_Cat_Adminsitradores_Cuentas_Bancarias_Obtener(
                int codEmpresa,
                string? identificacion)
        {
            var identificacionNormalizada =
                identificacion?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(
                    identificacionNormalizada))
            {
                return DbHelper.CreateOkResponse(
                    new List
                        <IvrCatAdministradorCuentaBancariaData>());
            }

            const string sql = """
                SELECT
                    RTRIM(ISNULL(C.CUENTA_INTERNA, ''))
                        AS cuenta_interna,
                    RTRIM(ISNULL(B.DESCRIPCION, ''))
                        AS banco,
                    CASE
                        WHEN C.TIPO = 'A'
                            THEN 'Ahorros'
                        ELSE 'Corriente'
                    END AS tipo_desc,
                    RTRIM(ISNULL(C.COD_DIVISA, ''))
                        AS cod_divisa,
                    CONVERT(bit, ISNULL(
                        C.CUENTA_INTERBANCA,
                        0))
                        AS cuenta_interbanca,
                    CASE
                        WHEN ISNULL(
                            C.CUENTA_INTERBANCA,
                            0) = 1
                            THEN 'S&iacute;'
                        ELSE 'No'
                    END AS cuenta_interbanca_desc,
                    RTRIM(ISNULL(C.DESTINO, ''))
                        AS destino,
                    CONVERT(bit, ISNULL(C.ACTIVA, 0))
                        AS activa,
                    CASE
                        WHEN ISNULL(C.ACTIVA, 0) = 1
                            THEN 'Activa'
                        ELSE 'Cerrada'
                    END AS estado_desc,
                    C.REGISTRO_FECHA
                        AS registro_fecha,
                    RTRIM(ISNULL(
                        C.REGISTRO_USUARIO,
                        ''))
                        AS registro_usuario
                FROM SYS_CUENTAS_BANCARIAS C
                INNER JOIN TES_BANCOS_GRUPOS B
                    ON C.COD_BANCO = B.COD_GRUPO
                WHERE C.IDENTIFICACION =
                    @identificacion
                ORDER BY C.CUENTA_INTERNA;
                """;

            return DbHelper.ExecuteListQuery
                <IvrCatAdministradorCuentaBancariaData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        identificacion =
                            identificacionNormalizada
                    });
        }

        /// <summary>
        /// Obtiene los portafolios y su estado de asignación
        /// para el administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codAdministrador">
        /// Código del administrador.
        /// </param>
        /// <returns>
        /// Portafolios disponibles.
        /// </returns>
        public ErrorDto<List<IvrCatAdministradorPortafolioData>>
            IVR_Cat_Adminsitradores_Portafolios_Obtener(
                int codEmpresa,
                string? codAdministrador)
        {
            var codigo = codAdministrador?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo del administrador "
                    + "es requerido.",
                    CodigoMensajeControlado,
                    new List
                        <IvrCatAdministradorPortafolioData>());
            }

            const string sql = """
                EXEC spIVR_ADMINISTRADORES_PORTAFOLIO
                    @cod_administrador;
                """;

            return DbHelper.ExecuteListQuery
                <IvrCatAdministradorPortafolioData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        cod_administrador = codigo
                    });
        }

        /// <summary>
        /// Actualiza la asignación de un portafolio
        /// al administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="request">
        /// Información de la asignación.
        /// </param>
        /// <returns>
        /// Resultado de la actualización.
        /// </returns>
        public ErrorDto
            IVR_Cat_Adminsitradores_Portafolio_Actualizar(
                int codEmpresa,
                IvrCatAdministradorPortafolioActualizarRequest?
                    request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "La solicitud es requerida.",
                    CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(
                    request.cod_portafolio) ||
                string.IsNullOrWhiteSpace(
                    request.cod_administrador))
            {
                return DbHelper.ErrorResponse(
                    "El administrador y el portafolio "
                    + "son requeridos.",
                    CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoMensajeControlado);
            }

            const string sql = """
                EXEC spIVR_PORTAFOLIO_ADMINISTRADORES_REGISTRO
                    @cod_portafolio,
                    @cod_administrador,
                    @asignado,
                    @usuario;
                """;

            var response = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    cod_portafolio =
                        request.cod_portafolio.Trim(),
                    cod_administrador =
                        request.cod_administrador.Trim(),
                    asignado = request.asignado,
                    usuario = request.usuario
                        .Trim()
                        .ToUpperInvariant()
                });

            return response.Code == 0
                ? DbHelper.OkResponse(
                    "Asignaci&oacute;n del portafolio "
                    + "actualizada satisfactoriamente.")
                : response;
        }

        /// <summary>
        /// Obtiene los instrumentos y su estado de asignación
        /// para el administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codAdministrador">
        /// Código del administrador.
        /// </param>
        /// <returns>
        /// Instrumentos disponibles.
        /// </returns>
        public ErrorDto<List<IvrCatAdministradorInstrumentoData>>
            IVR_Cat_Adminsitradores_Instrumentos_Obtener(
                int codEmpresa,
                string? codAdministrador)
        {
            var codigo = codAdministrador?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo del administrador "
                    + "es requerido.",
                    CodigoMensajeControlado,
                    new List
                        <IvrCatAdministradorInstrumentoData>());
            }

            const string sql = """
                EXEC spIVR_ADMINISTRADORES_INSTRUMENTOS
                    @cod_administrador;
                """;

            return DbHelper.ExecuteListQuery
                <IvrCatAdministradorInstrumentoData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        cod_administrador = codigo
                    });
        }

        /// <summary>
        /// Agrega o elimina la asignación de un instrumento
        /// al administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="request">
        /// Información de la asignación.
        /// </param>
        /// <returns>
        /// Resultado de la actualización.
        /// </returns>
        public ErrorDto
            IVR_Cat_Adminsitradores_Instrumento_Actualizar(
                int codEmpresa,
                IvrCatAdministradorInstrumentoActualizarRequest?
                    request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "La solicitud es requerida.",
                    CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(
                    request.cod_administrador) ||
                string.IsNullOrWhiteSpace(
                    request.cod_instrumento))
            {
                return DbHelper.ErrorResponse(
                    "El administrador y el instrumento "
                    + "son requeridos.",
                    CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoMensajeControlado);
            }

            const string sqlAsignar = """
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM IVR_ADMINISTRADOR_INST
                    WHERE COD_ADMINISTRADOR =
                        @cod_administrador
                      AND COD_INSTRUMENTO =
                        @cod_instrumento
                )
                BEGIN
                    INSERT INTO IVR_ADMINISTRADOR_INST
                    (
                        COD_ADMINISTRADOR,
                        COD_INSTRUMENTO,
                        REGISTRO_USUARIO,
                        REGISTRO_FECHA
                    )
                    VALUES
                    (
                        @cod_administrador,
                        @cod_instrumento,
                        @usuario,
                        dbo.MyGetdate()
                    );
                END;
                """;

            const string sqlDesasignar = """
                DELETE FROM IVR_ADMINISTRADOR_INST
                WHERE COD_ADMINISTRADOR =
                    @cod_administrador
                  AND COD_INSTRUMENTO =
                    @cod_instrumento;
                """;

            var response = DbHelper.ExecuteNonQuery(
                _portalDb,
                codEmpresa,
                request.asignado
                    ? sqlAsignar
                    : sqlDesasignar,
                new
                {
                    cod_administrador =
                        request.cod_administrador.Trim(),
                    cod_instrumento =
                        request.cod_instrumento.Trim(),
                    usuario = request.usuario
                        .Trim()
                        .ToUpperInvariant()
                });

            return response.Code == 0
                ? DbHelper.OkResponse(
                    "Asignaci&oacute;n del instrumento "
                    + "actualizada satisfactoriamente.")
                : response;
        }

        /// <summary>
        /// Obtiene los contactos relacionados
        /// con el administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codAdministrador">
        /// Código del administrador.
        /// </param>
        /// <returns>
        /// Contactos encontrados.
        /// </returns>
        public ErrorDto<List<IvrCatAdministradorContactoData>>
            IVR_Cat_Adminsitradores_Contactos_Obtener(
                int codEmpresa,
                string? codAdministrador)
        {
            var codigo = codAdministrador?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo del administrador "
                    + "es requerido.",
                    CodigoMensajeControlado,
                    new List
                        <IvrCatAdministradorContactoData>());
            }

            const string sql = """
                SELECT
                    ISNULL(COD_CONTACTO, 0)
                        AS cod_contacto,
                    RTRIM(ISNULL(NOMBRE, ''))
                        AS nombre,
                    RTRIM(ISNULL(CELULAR, ''))
                        AS celular,
                    RTRIM(ISNULL(TELEFONO, ''))
                        AS telefono,
                    RTRIM(ISNULL(EMAIL_01, ''))
                        AS email_01,
                    RTRIM(ISNULL(EMAIL_02, ''))
                        AS email_02
                FROM IVR_CONTACTOS
                WHERE COD_ADMINISTRADOR =
                    @cod_administrador
                ORDER BY COD_CONTACTO;
                """;

            return DbHelper.ExecuteListQuery
                <IvrCatAdministradorContactoData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        cod_administrador = codigo
                    });
        }
    }
}