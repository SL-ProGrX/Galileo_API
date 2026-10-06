using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslExpedienteApelacionesDb
    {
        private const int CodigoValidacion = -2;

        private const string EstadoRechazado = "R";
        private const string EstadoAplicado = "X";
        private const string ResolucionPendiente = "P";
        private const string ResolucionAprobada = "A";
        private const string ResolucionRechazada = "R";

        private const string MensajeExpedienteRequerido =
            "El c&oacute;digo del expediente es requerido.";

        private const string MensajeExpedienteNoExiste =
            "El expediente indicado no existe.";

        private const string MensajeTipoApelacionRequerido =
            "El tipo de apelaci&oacute;n es requerido.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        private const string MensajeApelacionPendiente =
            "Ya se encuentra registrada una apelaci&oacute;n pendiente de resoluci&oacute;n para este expediente.";

        private const string MensajeExpedienteNoRechazado =
            "El expediente no se encuentra rechazado para registrar una apelaci&oacute;n.";

        private const string MensajeExpedienteAplicado =
            "Este expediente se encuentra aplicado y no se puede cambiar la resoluci&oacute;n.";

        private const string MensajeSinApelacionPendiente =
            "No existe ninguna apelaci&oacute;n pendiente de resoluci&oacute;n.";

        private const string MensajeNotasResolucion =
            "Debe indicar una nota v&aacute;lida para la resoluci&oacute;n.";

        private readonly PortalDB _portalDb;
        private readonly FrmFslExpedienteDB _expedienteDb;

        public FrmFslExpedienteApelacionesDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _expedienteDb =
                new FrmFslExpedienteDB(config);
        }

        /// <summary>
        /// Obtiene el encabezado del expediente seleccionado.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="codExpediente">
        /// C&#243;digo del expediente.
        /// </param>
        /// <returns>
        /// Informaci&#243;n general del expediente.
        /// </returns>
        public ErrorDto<FslExpedienteDatos>
            FSL_ExpedienteApelaciones_Expediente_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            return _expedienteDb.FSL_Expediente_Obtener(
                CodEmpresa,
                codExpediente);
        }

        /// <summary>
        /// Obtiene los tipos de apelaci&#243;n activos.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <returns>
        /// Tipos de apelaci&#243;n disponibles.
        /// </returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_ExpedienteApelaciones_Catalogo_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(COD_APELACION) AS item,
                    RTRIM(COD_APELACION) + ' - ' +
                    RTRIM(ISNULL(DESCRIPCION, ''))
                        AS descripcion
                FROM FSL_TIPOS_APELACIONES
                WHERE ACTIVA = 1
                ORDER BY COD_APELACION;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel>(
                    _portalDb,
                    CodEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene el hist&#243;rico de apelaciones del expediente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="codExpediente">
        /// C&#243;digo del expediente.
        /// </param>
        /// <returns>
        /// Apelaciones registradas.
        /// </returns>
        public ErrorDto<List<FslExpedienteApelacionData>>
            FSL_ExpedienteApelaciones_Historico_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            if (codExpediente <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeExpedienteRequerido,
                    CodigoValidacion,
                    new List<
                        FslExpedienteApelacionData>());
            }

            return _expedienteDb
                .FSL_Expediente_Apelaciones_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        /// <summary>
        /// Obtiene los miembros del comit&#233; asociado al expediente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="codExpediente">
        /// C&#243;digo del expediente.
        /// </param>
        /// <returns>
        /// Miembros activos del comit&#233;.
        /// </returns>
        public ErrorDto<
            List<FslExpedienteResolucionMiembroData>>
            FSL_ExpedienteApelaciones_Miembros_Obtener(
                int CodEmpresa,
                long codExpediente)
        {
            if (codExpediente <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeExpedienteRequerido,
                    CodigoValidacion,
                    new List<
                        FslExpedienteResolucionMiembroData>());
            }

            return _expedienteDb
                .FSL_Expediente_ResolucionMiembros_Obtener(
                    CodEmpresa,
                    codExpediente);
        }

        /// <summary>
        /// Obtiene el usuario vinculado al miembro del comit&#233;.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="cedula">
        /// Identificaci&#243;n del miembro.
        /// </param>
        /// <param name="codComite">
        /// C&#243;digo del comit&#233;.
        /// </param>
        /// <returns>
        /// Usuario vinculado.
        /// </returns>
        public ErrorDto<string?>
            FSL_ExpedienteApelaciones_UsuarioVinculado_Obtener(
                int CodEmpresa,
                string cedula,
                string codComite)
        {
            return _expedienteDb
                .FSL_Expediente_UsuarioVinculado_Obtener(
                    CodEmpresa,
                    cedula,
                    codComite);
        }

        /// <summary>
        /// Valida las credenciales de un miembro del comit&#233;.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Credenciales del miembro.
        /// </param>
        /// <returns>
        /// Resultado de la validaci&#243;n.
        /// </returns>
        public ErrorDto
            FSL_ExpedienteApelaciones_Miembro_Validar(
                int CodEmpresa,
                FslExpedienteMiembroValidarRequest? request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "Las credenciales del miembro son requeridas.",
                    CodigoValidacion);
            }

            return _expedienteDb
                .FSL_Expediente_Miembro_Validar(
                    CodEmpresa,
                    request);
        }

        /// <summary>
        /// Registra una nueva apelaci&#243;n para el expediente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&#243;n de la apelaci&#243;n.
        /// </param>
        /// <returns>
        /// Resultado del registro.
        /// </returns>
        public ErrorDto
            FSL_ExpedienteApelaciones_Apelacion_Agregar(
                int CodEmpresa,
                FslExpedienteApelacionAgregarRequest? request)
        {
            var validacion =
                FSL_ExpedienteApelaciones_Apelacion_Validar(
                    request);

            if (validacion is not null)
            {
                return validacion;
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        CodEmpresa);

                connection.Open();

                var estado =
                    FSL_ExpedienteApelaciones_Estado_Obtener(
                        connection,
                        request!.cod_expediente);

                var estadoValidacion =
                    FSL_ExpedienteApelaciones_Estado_Validar(
                        estado);

                if (estadoValidacion is not null)
                {
                    return estadoValidacion;
                }

                connection.Execute(
                    "[spFSL_ApelacionRegistra]",
                    new
                    {
                        Expediente =
                            request.cod_expediente,
                        Tipo =
                            request.cod_apelacion.Trim(),
                        PresentaCedula =
                            request.presenta_identificacion
                                .Trim(),
                        PresentaNombre =
                            request.presenta_nombre.Trim(),
                        PresentaNotas =
                            request.notas.Trim(),
                        Usuario =
                            request.usuario
                                .Trim()
                                .ToUpperInvariant()
                    },
                    commandType:
                        CommandType.StoredProcedure);

                return DbHelper.OkResponse(
                    "Apelaci&oacute;n registrada satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Guarda la resoluci&#243;n de la apelaci&#243;n pendiente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&#243;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Datos de la resoluci&#243;n.
        /// </param>
        /// <returns>
        /// Resultado de la actualizaci&#243;n.
        /// </returns>
        public ErrorDto
            FSL_ExpedienteApelaciones_Resolucion_Guardar(
                int CodEmpresa,
                FslExpedienteApelacionResolucionGuardarRequest?
                    request)
        {
            var validacion =
                FSL_ExpedienteApelaciones_Resolucion_Validar(
                    request);

            if (validacion is not null)
            {
                return validacion;
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        CodEmpresa);

                connection.Open();

                return
                    FSL_ExpedienteApelaciones_Resolucion_Ejecutar(
                        connection,
                        request!);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static ErrorDto
            FSL_ExpedienteApelaciones_Resolucion_Ejecutar(
                SqlConnection connection,
                FslExpedienteApelacionResolucionGuardarRequest
                    request)
        {
            using var transaction =
                connection.BeginTransaction();

            var contexto =
                FSL_ExpedienteApelaciones_Resolucion_Contexto_Obtener(
                    connection,
                    transaction,
                    request.cod_expediente);

            var contextoValidacion =
                FSL_ExpedienteApelaciones_Resolucion_Contexto_Validar(
                    contexto);

            if (contextoValidacion is not null)
            {
                return contextoValidacion;
            }

            var cedulas =
                FSL_ExpedienteApelaciones_Cedulas_Normalizar(
                    request.miembros);

            var miembrosValidos =
                FSL_ExpedienteApelaciones_Miembros_Validos_Obtener(
                    connection,
                    transaction,
                    contexto!.cod_comite,
                    cedulas);

            if (miembrosValidos.Count <
                contexto.numero_resolutores)
            {
                return DbHelper.ErrorResponse(
                    $"Debe indicar al menos ({contexto.numero_resolutores}) " +
                    "miembros del comit&eacute; validados que den la resoluci&oacute;n.",
                    CodigoValidacion);
            }

            FSL_ExpedienteApelaciones_Resolucion_Actualizar(
                connection,
                transaction,
                request,
                contexto.linea);

            FSL_ExpedienteApelaciones_Comite_Reasignar(
                connection,
                transaction,
                request,
                contexto,
                miembrosValidos);

            transaction.Commit();

            return DbHelper.OkResponse(
                "Expediente actualizado satisfactoriamente.");
        }

        private static FslExpedienteApelacionEstadoData?
            FSL_ExpedienteApelaciones_Estado_Obtener(
                SqlConnection connection,
                long codExpediente)
        {
            const string sql = """
                SELECT
                    RTRIM(ISNULL(Ex.ESTADO, '')) AS estado,
                    ISNULL(
                        (
                            SELECT MAX(Ea.LINEA)
                            FROM FSL_EXPEDIENTES_APELACIONES Ea
                            WHERE Ea.COD_EXPEDIENTE =
                                Ex.COD_EXPEDIENTE
                              AND Ea.RESOLUCION =
                                @resolucion_pendiente
                        ),
                        0
                    ) AS linea
                FROM FSL_EXPEDIENTES Ex
                WHERE Ex.COD_EXPEDIENTE =
                    @cod_expediente;
                """;

            return connection.QueryFirstOrDefault<
                FslExpedienteApelacionEstadoData>(
                    sql,
                    new
                    {
                        cod_expediente = codExpediente,
                        resolucion_pendiente =
                            ResolucionPendiente
                    });
        }

        private static FslExpedienteApelacionResolucionContextoData?
            FSL_ExpedienteApelaciones_Resolucion_Contexto_Obtener(
                SqlConnection connection,
                SqlTransaction transaction,
                long codExpediente)
        {
            const string sql = """
                SELECT
                    RTRIM(ISNULL(Ex.ESTADO, '')) AS estado,
                    RTRIM(ISNULL(Ex.COD_COMITE, ''))
                        AS cod_comite,
                    ISNULL(Co.NUMERO_RESOLUTORES, 0)
                        AS numero_resolutores,
                    ISNULL(
                        (
                            SELECT MAX(Ea.LINEA)
                            FROM FSL_EXPEDIENTES_APELACIONES Ea
                            WHERE Ea.COD_EXPEDIENTE =
                                Ex.COD_EXPEDIENTE
                              AND Ea.RESOLUCION =
                                @resolucion_pendiente
                        ),
                        0
                    ) AS linea
                FROM FSL_EXPEDIENTES Ex
                LEFT JOIN FSL_COMITES Co
                    ON Ex.COD_COMITE = Co.COD_COMITE
                WHERE Ex.COD_EXPEDIENTE =
                    @cod_expediente;
                """;

            return connection.QueryFirstOrDefault<
                FslExpedienteApelacionResolucionContextoData>(
                    sql,
                    new
                    {
                        cod_expediente = codExpediente,
                        resolucion_pendiente =
                            ResolucionPendiente
                    },
                    transaction);
        }

        private static List<string>
            FSL_ExpedienteApelaciones_Miembros_Validos_Obtener(
                SqlConnection connection,
                SqlTransaction transaction,
                string codComite,
                List<string> cedulas)
        {
            if (cedulas.Count == 0)
            {
                return [];
            }

            const string sql = """
                SELECT DISTINCT
                    RTRIM(CEDULA)
                FROM FSL_COMITES_MIEMBROS
                WHERE COD_COMITE = @cod_comite
                  AND ACTIVO = 1
                  AND CEDULA IN @cedulas;
                """;

            return connection.Query<string>(
                    sql,
                    new
                    {
                        cod_comite = codComite,
                        cedulas
                    },
                    transaction)
                .ToList();
        }

        private static void
            FSL_ExpedienteApelaciones_Resolucion_Actualizar(
                SqlConnection connection,
                SqlTransaction transaction,
                FslExpedienteApelacionResolucionGuardarRequest
                    request,
                int linea)
        {
            const string sqlExpediente = """
                UPDATE FSL_EXPEDIENTES
                SET
                    RESOLUCION_ESTADO = @resolucion,
                    ESTADO = @resolucion
                WHERE COD_EXPEDIENTE = @cod_expediente;
                """;

            connection.Execute(
                sqlExpediente,
                new
                {
                    resolucion =
                        request.resolucion.Trim(),
                    request.cod_expediente
                },
                transaction);

            const string sqlApelacion = """
                UPDATE FSL_EXPEDIENTES_APELACIONES
                SET
                    RESOLUCION_NOTAS =
                        @resolucion_notas,
                    RESOLUCION = @resolucion,
                    RESOLUCION_FECHA = GETDATE(),
                    RESOLUCION_USUARIO =
                        @resolucion_usuario
                WHERE COD_EXPEDIENTE =
                    @cod_expediente
                  AND LINEA = @linea;
                """;

            connection.Execute(
                sqlApelacion,
                new
                {
                    resolucion_notas =
                        request.resolucion_notas.Trim(),
                    resolucion =
                        request.resolucion.Trim(),
                    resolucion_usuario =
                        request.resolucion_usuario
                            .Trim()
                            .ToUpperInvariant(),
                    request.cod_expediente,
                    linea
                },
                transaction);
        }

        private static void
            FSL_ExpedienteApelaciones_Comite_Reasignar(
                SqlConnection connection,
                SqlTransaction transaction,
                FslExpedienteApelacionResolucionGuardarRequest
                    request,
                FslExpedienteApelacionResolucionContextoData
                    contexto,
                List<string> miembros)
        {
            const string sqlEliminar = """
                DELETE FROM
                    FSL_EXPEDIENTES_APELACIONES_COMITE
                WHERE COD_EXPEDIENTE =
                    @cod_expediente;
                """;

            connection.Execute(
                sqlEliminar,
                new
                {
                    request.cod_expediente
                },
                transaction);

            const string sqlInsertar = """
                INSERT INTO
                    FSL_EXPEDIENTES_APELACIONES_COMITE
                    (
                        LINEA,
                        COD_EXPEDIENTE,
                        COD_COMITE,
                        CEDULA,
                        ASIGNA_FECHA,
                        ASIGNA_USUARIO
                    )
                VALUES
                    (
                        @linea,
                        @cod_expediente,
                        @cod_comite,
                        @cedula,
                        GETDATE(),
                        @asigna_usuario
                    );
                """;

            foreach (var cedula in miembros)
            {
                connection.Execute(
                    sqlInsertar,
                    new
                    {
                        contexto.linea,
                        request.cod_expediente,
                        contexto.cod_comite,
                        cedula,
                        asigna_usuario =
                            request.resolucion_usuario
                                .Trim()
                                .ToUpperInvariant()
                    },
                    transaction);
            }
        }

        private static List<string>
            FSL_ExpedienteApelaciones_Cedulas_Normalizar(
                IEnumerable<
                    FslExpedienteResolucionMiembroRequest>
                    miembros)
        {
            return miembros
                .Select(miembro =>
                    miembro.cedula?.Trim() ??
                    string.Empty)
                .Where(cedula =>
                    !string.IsNullOrWhiteSpace(cedula))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static ErrorDto?
            FSL_ExpedienteApelaciones_Apelacion_Validar(
                FslExpedienteApelacionAgregarRequest?
                    request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "La informaci&oacute;n de la apelaci&oacute;n es requerida.",
                    CodigoValidacion);
            }

            if (request.cod_expediente <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeExpedienteRequerido,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                request.cod_apelacion))
            {
                return DbHelper.ErrorResponse(
                    MensajeTipoApelacionRequerido,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                request.usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            return null;
        }

        private static ErrorDto?
            FSL_ExpedienteApelaciones_Estado_Validar(
                FslExpedienteApelacionEstadoData?
                    estado)
        {
            if (estado is null)
            {
                return DbHelper.ErrorResponse(
                    MensajeExpedienteNoExiste,
                    CodigoValidacion);
            }

            if (!string.Equals(
                estado.estado,
                EstadoRechazado,
                StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.ErrorResponse(
                    MensajeExpedienteNoRechazado,
                    CodigoValidacion);
            }

            if (estado.linea > 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeApelacionPendiente,
                    CodigoValidacion);
            }

            return null;
        }

        private static ErrorDto?
            FSL_ExpedienteApelaciones_Resolucion_Validar(
                FslExpedienteApelacionResolucionGuardarRequest?
                    request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "La informaci&oacute;n de la resoluci&oacute;n es requerida.",
                    CodigoValidacion);
            }

            if (request.cod_expediente <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeExpedienteRequerido,
                    CodigoValidacion);
            }

            if (!FSL_ExpedienteApelaciones_Resolucion_EsValida(
                request.resolucion))
            {
                return DbHelper.ErrorResponse(
                    "La resoluci&oacute;n indicada no es v&aacute;lida.",
                    CodigoValidacion);
            }

            if (request.resolucion_notas.Trim().Length < 10)
            {
                return DbHelper.ErrorResponse(
                    MensajeNotasResolucion,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                request.resolucion_usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            return null;
        }

        private static ErrorDto?
            FSL_ExpedienteApelaciones_Resolucion_Contexto_Validar(
                FslExpedienteApelacionResolucionContextoData?
                    contexto)
        {
            if (contexto is null)
            {
                return DbHelper.ErrorResponse(
                    MensajeExpedienteNoExiste,
                    CodigoValidacion);
            }

            if (string.Equals(
                contexto.estado,
                EstadoAplicado,
                StringComparison.OrdinalIgnoreCase))
            {
                return DbHelper.ErrorResponse(
                    MensajeExpedienteAplicado,
                    CodigoValidacion);
            }

            if (contexto.linea <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeSinApelacionPendiente,
                    CodigoValidacion);
            }

            return null;
        }

        private static bool
            FSL_ExpedienteApelaciones_Resolucion_EsValida(
                string resolucion)
        {
            var valor = resolucion.Trim();

            return string.Equals(
                       valor,
                       ResolucionAprobada,
                       StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(
                       valor,
                       ResolucionRechazada,
                       StringComparison.OrdinalIgnoreCase);
        }

        private sealed class
            FslExpedienteApelacionEstadoData
        {
            public string estado { get; set; } =
                string.Empty;

            public int linea { get; set; } = 0;
        }

        private sealed class
            FslExpedienteApelacionResolucionContextoData
        {
            public string estado { get; set; } =
                string.Empty;

            public string cod_comite { get; set; } =
                string.Empty;

            public int numero_resolutores { get; set; } = 0;

            public int linea { get; set; } = 0;
        }
    }
}
