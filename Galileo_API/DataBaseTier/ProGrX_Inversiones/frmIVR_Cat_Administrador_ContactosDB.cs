using System.Data;
using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Inversiones;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed class FrmIvrCatAdministradorContactosDb
    {
        private const int CodigoMensajeControlado = -2;
        private const int ModuloInversiones = 22;

        private const string MensajeNoEncontrado =
            "No se encontr&oacute; el contacto indicado.";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmIvrCatAdministradorContactosDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Busca los contactos asociados a un administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codAdministrador">
        /// Código del administrador.
        /// </param>
        /// <param name="filtro">
        /// Código, nombre, teléfono o correo utilizado como filtro.
        /// </param>
        /// <returns>
        /// Contactos que coinciden con el filtro.
        /// </returns>
        public ErrorDto<List<IvrCatAdministradorContactoData>>
            IVR_Cat_Administrador_Contactos_Buscar(
                int codEmpresa,
                string? codAdministrador,
                string? filtro)
        {
            var codigoAdministrador =
                codAdministrador?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(codigoAdministrador))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo del administrador es requerido.",
                    CodigoMensajeControlado,
                    new List<IvrCatAdministradorContactoData>());
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
                  AND
                  (
                      @filtro = ''
                      OR CONVERT(varchar(20), COD_CONTACTO)
                          LIKE '%' + @filtro + '%'
                      OR NOMBRE LIKE '%' + @filtro + '%'
                      OR CELULAR LIKE '%' + @filtro + '%'
                      OR TELEFONO LIKE '%' + @filtro + '%'
                      OR EMAIL_01 LIKE '%' + @filtro + '%'
                      OR EMAIL_02 LIKE '%' + @filtro + '%'
                  )
                ORDER BY COD_CONTACTO;
                """;

            return DbHelper.ExecuteListQuery
                <IvrCatAdministradorContactoData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        cod_administrador = codigoAdministrador,
                        filtro = filtro?.Trim() ?? string.Empty
                    });
        }

        /// <summary>
        /// Obtiene un contacto específico de un administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codAdministrador">
        /// Código del administrador.
        /// </param>
        /// <param name="codContacto">
        /// Identificador del contacto.
        /// </param>
        /// <returns>
        /// Información del contacto encontrado.
        /// </returns>
        public ErrorDto<IvrCatAdministradorContactoData?>
            IVR_Cat_Administrador_Contactos_Obtener(
                int codEmpresa,
                string? codAdministrador,
                int codContacto)
        {
            var validacion =
                IVR_Cat_Administrador_Contactos_Clave_Validar(
                    codAdministrador,
                    codContacto);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorContactoData?>(
                        validacion,
                        CodigoMensajeControlado);
            }

            const string sql = """
                SELECT TOP (1)
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
                  AND COD_CONTACTO = @cod_contacto;
                """;

            var response = DbHelper.ExecuteSingleQuery
                <IvrCatAdministradorContactoData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    defaultValue: null,
                    parameters: new
                    {
                        cod_administrador =
                            codAdministrador!.Trim(),
                        cod_contacto = codContacto
                    });

            if (response.Code != 0)
            {
                return response;
            }

            return response.Result is null
                ? DbHelper.CreateErrorResponse
                    <IvrCatAdministradorContactoData?>(
                        MensajeNoEncontrado,
                        CodigoMensajeControlado)
                : response;
        }

        /// <summary>
        /// Obtiene el contacto anterior o siguiente del
        /// administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="codAdministrador">
        /// Código del administrador.
        /// </param>
        /// <param name="codContacto">
        /// Contacto actual. Cero inicia desde un extremo.
        /// </param>
        /// <param name="siguiente">
        /// True para siguiente; false para anterior.
        /// </param>
        /// <returns>
        /// Código encontrado o null al llegar al límite.
        /// </returns>
        public ErrorDto
            <IvrCatAdministradorContactoNavegacionData?>
            IVR_Cat_Administrador_Contactos_Navegar(
                int codEmpresa,
                string? codAdministrador,
                int codContacto,
                bool siguiente)
        {
            var codigoAdministrador =
                codAdministrador?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(codigoAdministrador))
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorContactoNavegacionData?>(
                        "El c&oacute;digo del administrador "
                        + "es requerido.",
                        CodigoMensajeControlado);
            }

            const string sqlSiguiente = """
                SELECT TOP (1)
                    ISNULL(COD_CONTACTO, 0)
                        AS cod_contacto
                FROM IVR_CONTACTOS
                WHERE COD_ADMINISTRADOR =
                    @cod_administrador
                  AND
                  (
                      @cod_contacto <= 0
                      OR COD_CONTACTO > @cod_contacto
                  )
                ORDER BY COD_CONTACTO ASC;
                """;

            const string sqlAnterior = """
                SELECT TOP (1)
                    ISNULL(COD_CONTACTO, 0)
                        AS cod_contacto
                FROM IVR_CONTACTOS
                WHERE COD_ADMINISTRADOR =
                    @cod_administrador
                  AND
                  (
                      @cod_contacto <= 0
                      OR COD_CONTACTO < @cod_contacto
                  )
                ORDER BY COD_CONTACTO DESC;
                """;

            return DbHelper.ExecuteSingleQuery
                <IvrCatAdministradorContactoNavegacionData>(
                    _portalDb,
                    codEmpresa,
                    siguiente
                        ? sqlSiguiente
                        : sqlAnterior,
                    defaultValue: null,
                    parameters: new
                    {
                        cod_administrador = codigoAdministrador,
                        cod_contacto = codContacto
                    });
        }

        /// <summary>
        /// Registra un contacto nuevo para el administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="request">
        /// Información del contacto.
        /// </param>
        /// <returns>
        /// Contacto registrado.
        /// </returns>
        public ErrorDto<IvrCatAdministradorContactoData?>
            IVR_Cat_Administrador_Contactos_Registrar(
                int codEmpresa,
                IvrCatAdministradorContactoGuardarRequest? request)
        {
            var validacion =
                IVR_Cat_Administrador_Contactos_Request_Validar(
                    request,
                    requiereCodigo: false);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorContactoData?>(
                        validacion,
                        CodigoMensajeControlado);
            }

            const string sqlConsecutivo = """
                SELECT
                    ISNULL(MAX(COD_CONTACTO), 0) + 1
                FROM IVR_CONTACTOS WITH (UPDLOCK, HOLDLOCK);
                """;

            const string sqlInsertar = """
                INSERT INTO IVR_CONTACTOS
                (
                    COD_CONTACTO,
                    COD_ADMINISTRADOR,
                    NOMBRE,
                    CELULAR,
                    TELEFONO,
                    EMAIL_01,
                    EMAIL_02,
                    REGISTRO_USUARIO,
                    REGISTRO_FECHA
                )
                VALUES
                (
                    @cod_contacto,
                    @cod_administrador,
                    @nombre,
                    @celular,
                    @telefono,
                    @email_01,
                    @email_02,
                    @usuario,
                    dbo.MyGetdate()
                );
                """;

            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    codEmpresa);

                using var transaction = connection.BeginTransaction(
                    IsolationLevel.Serializable);

                var codContacto = connection.ExecuteScalar<int>(
                    sqlConsecutivo,
                    transaction: transaction);

                _ = connection.Execute(
                    sqlInsertar,
                    IVR_Cat_Administrador_Contactos_Parametros_Crear(
                        request!,
                        codContacto),
                    transaction);

                transaction.Commit();

                IVR_Cat_Administrador_Contactos_Bitacora_Registrar(
                    codEmpresa,
                    request!.usuario,
                    "Registra",
                    request.cod_administrador,
                    codContacto);

                return IVR_Cat_Administrador_Contactos_Obtener(
                    codEmpresa,
                    request.cod_administrador,
                    codContacto);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorContactoData?>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorContactoData?>(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un contacto existente del administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="request">
        /// Información actualizada del contacto.
        /// </param>
        /// <returns>
        /// Contacto actualizado.
        /// </returns>
        public ErrorDto<IvrCatAdministradorContactoData?>
            IVR_Cat_Administrador_Contactos_Actualizar(
                int codEmpresa,
                IvrCatAdministradorContactoGuardarRequest? request)
        {
            var validacion =
                IVR_Cat_Administrador_Contactos_Request_Validar(
                    request,
                    requiereCodigo: true);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorContactoData?>(
                        validacion,
                        CodigoMensajeControlado);
            }

            const string sql = """
                UPDATE IVR_CONTACTOS
                SET NOMBRE = @nombre,
                    CELULAR = @celular,
                    TELEFONO = @telefono,
                    EMAIL_01 = @email_01,
                    EMAIL_02 = @email_02
                WHERE COD_ADMINISTRADOR =
                    @cod_administrador
                  AND COD_CONTACTO = @cod_contacto;
                """;

            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    codEmpresa);

                var afectados = connection.Execute(
                    sql,
                    IVR_Cat_Administrador_Contactos_Parametros_Crear(
                        request!,
                        request!.cod_contacto));

                if (afectados == 0)
                {
                    return DbHelper.CreateErrorResponse
                        <IvrCatAdministradorContactoData?>(
                            MensajeNoEncontrado,
                            CodigoMensajeControlado);
                }

                IVR_Cat_Administrador_Contactos_Bitacora_Registrar(
                    codEmpresa,
                    request.usuario,
                    "Modifica",
                    request.cod_administrador,
                    request.cod_contacto);

                return IVR_Cat_Administrador_Contactos_Obtener(
                    codEmpresa,
                    request.cod_administrador,
                    request.cod_contacto);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorContactoData?>(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.CreateErrorResponse
                    <IvrCatAdministradorContactoData?>(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un contacto del administrador.
        /// </summary>
        /// <param name="codEmpresa">
        /// Código de la empresa.
        /// </param>
        /// <param name="request">
        /// Identificación del contacto y usuario responsable.
        /// </param>
        /// <returns>
        /// Resultado de la eliminación.
        /// </returns>
        public ErrorDto IVR_Cat_Administrador_Contactos_Eliminar(
            int codEmpresa,
            IvrCatAdministradorContactoEliminarRequest? request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "La solicitud es requerida.",
                    CodigoMensajeControlado);
            }

            var validacion =
                IVR_Cat_Administrador_Contactos_Clave_Validar(
                    request.cod_administrador,
                    request.cod_contacto);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoMensajeControlado);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoMensajeControlado);
            }

            const string sql = """
                DELETE FROM IVR_CONTACTOS
                WHERE COD_ADMINISTRADOR =
                    @cod_administrador
                  AND COD_CONTACTO = @cod_contacto;
                """;

            try
            {
                using var connection = DbHelper.OpenConnection(
                    _portalDb,
                    codEmpresa);

                var afectados = connection.Execute(
                    sql,
                    new
                    {
                        cod_administrador =
                            request.cod_administrador.Trim(),
                        cod_contacto = request.cod_contacto
                    });

                if (afectados == 0)
                {
                    return DbHelper.ErrorResponse(
                        MensajeNoEncontrado,
                        CodigoMensajeControlado);
                }

                IVR_Cat_Administrador_Contactos_Bitacora_Registrar(
                    codEmpresa,
                    request.usuario,
                    "Elimina",
                    request.cod_administrador,
                    request.cod_contacto);

                return DbHelper.OkResponse(
                    "Contacto eliminado satisfactoriamente.");
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static string
            IVR_Cat_Administrador_Contactos_Request_Validar(
                IvrCatAdministradorContactoGuardarRequest? request,
                bool requiereCodigo)
        {
            if (request is null)
            {
                return "La solicitud es requerida.";
            }

            if (string.IsNullOrWhiteSpace(
                    request.cod_administrador))
            {
                return "El c&oacute;digo del administrador "
                    + "es requerido.";
            }

            if (requiereCodigo && request.cod_contacto <= 0)
            {
                return "El c&oacute;digo del contacto es requerido.";
            }

            if (string.IsNullOrWhiteSpace(request.nombre))
            {
                return "El nombre del contacto no es v&aacute;lido.";
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return "El usuario es requerido.";
            }

            return string.Empty;
        }

        private static string
            IVR_Cat_Administrador_Contactos_Clave_Validar(
                string? codAdministrador,
                int codContacto)
        {
            if (string.IsNullOrWhiteSpace(codAdministrador))
            {
                return "El c&oacute;digo del administrador "
                    + "es requerido.";
            }

            return codContacto <= 0
                ? "El c&oacute;digo del contacto es requerido."
                : string.Empty;
        }

        private static object
            IVR_Cat_Administrador_Contactos_Parametros_Crear(
                IvrCatAdministradorContactoGuardarRequest request,
                int codContacto)
        {
            return new
            {
                cod_contacto = codContacto,
                cod_administrador =
                    request.cod_administrador.Trim(),
                nombre = request.nombre.Trim(),
                celular = request.celular?.Trim() ?? string.Empty,
                telefono = request.telefono?.Trim() ?? string.Empty,
                email_01 = request.email_01?.Trim() ?? string.Empty,
                email_02 = request.email_02?.Trim() ?? string.Empty,
                usuario = request.usuario
                    .Trim()
                    .ToUpperInvariant()
            };
        }

        private void
            IVR_Cat_Administrador_Contactos_Bitacora_Registrar(
                int codEmpresa,
                string usuario,
                string movimiento,
                string codAdministrador,
                int codContacto)
        {
            _ = _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = usuario
                        .Trim()
                        .ToUpperInvariant(),
                    Modulo = ModuloInversiones,
                    Movimiento = movimiento,
                    DetalleMovimiento =
                        $"Administrador: {codAdministrador.Trim()}, "
                        + $"Contacto: {codContacto}"
                });
        }
    }
}
