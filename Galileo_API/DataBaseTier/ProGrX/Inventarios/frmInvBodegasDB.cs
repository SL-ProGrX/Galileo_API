using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmInvBodegasDb
    {
        private const string MensajeOk = "Ok";
        private const string ErrorConsultarPermisos = "Error al consultar los permisos de la bodega.";
        private const string ErrorConsultarBodegas = "Error al consultar las bodegas.";
        private const string ErrorConsultarBodega = "Error al consultar la bodega.";
        private const string ErrorNavegarBodegas = "Error al navegar entre las bodegas.";
        private const string ErrorRegistrarBodega = "Error al registrar la bodega.";
        private const string ErrorActualizarBodega = "Error al actualizar la bodega.";
        private const string ErrorEliminarBodega = "Error al eliminar la bodega.";
        private const string ErrorActualizarPermiso = "Error al actualizar los permisos de la bodega.";

        private const string CamposBodega = """
            cod_bodega,
            descripcion,
            observacion,
            cod_cuenta,
            cod_cta_gastosTF AS cod_cta_gastostf,
            cod_cta_ingresosTF AS cod_cta_ingresostf,
            permite_entradas,
            permite_salidas,
            utiliza_permisos,
            estado
            """;

        private const string QueryObtenerBodegas = $"""
            SELECT
                {CamposBodega}
            FROM PV_BODEGAS
            ORDER BY cod_bodega ASC;
            """;

        private const string QueryObtenerBodega = $"""
            SELECT
                {CamposBodega}
            FROM PV_BODEGAS
            WHERE cod_bodega = @CodBodega;
            """;

        private const string QueryNavegarBodegas = $"""
            SELECT TOP 1
                {CamposBodega}
            FROM PV_BODEGAS
            WHERE
                (@Tipo = 'asc' AND cod_bodega > @CodBodega)
                OR
                (@Tipo = 'desc' AND cod_bodega < @CodBodega)
            ORDER BY
                CASE
                    WHEN @Tipo = 'asc' THEN cod_bodega
                END ASC,
                CASE
                    WHEN @Tipo = 'desc' THEN cod_bodega
                END DESC;
            """;

        private const string QueryExisteBodega = """
            SELECT COUNT(1)
            FROM PV_BODEGAS
            WHERE cod_bodega = @CodBodega;
            """;

        private const string QueryRegistrarBodega = """
            INSERT INTO PV_BODEGAS
            (
                cod_bodega,
                descripcion,
                observacion,
                estado,
                fecha_inclusion,
                permite_entradas,
                permite_salidas,
                cod_cuenta,
                cod_cta_ingresosTF,
                cod_cta_gastosTF,
                utiliza_permisos
            )
            VALUES
            (
                @CodBodega,
                @Descripcion,
                @Observacion,
                @Estado,
                GETDATE(),
                @PermiteEntradas,
                @PermiteSalidas,
                @CodCuenta,
                @CodCtaIngresosTf,
                @CodCtaGastosTf,
                @UtilizaPermisos
            );
            """;

        private const string QueryActualizarBodega = """
            UPDATE PV_BODEGAS
            SET descripcion = @Descripcion,
                observacion = @Observacion,
                estado = @Estado,
                permite_entradas = @PermiteEntradas,
                permite_salidas = @PermiteSalidas,
                cod_cuenta = @CodCuenta,
                cod_cta_ingresosTF = @CodCtaIngresosTf,
                cod_cta_gastosTF = @CodCtaGastosTf,
                utiliza_permisos = @UtilizaPermisos
            WHERE cod_bodega = @CodBodega;
            """;

        private const string QueryEliminarPermisos = """
            DELETE FROM PV_BODEGAS_PERMISOS
            WHERE cod_bodega = @CodBodega;
            """;

        private const string QueryEliminarBodega = """
            DELETE FROM PV_BODEGAS
            WHERE cod_bodega = @CodBodega;
            """;

        private const string QueryObtenerPermisos = """
            SELECT
                U.nombre,
                U.descripcion,
                CONVERT
                (
                    bit,
                    CASE @TipoTransaccion
                        WHEN 'E' THEN ISNULL(C.E_Modifica, 0)
                        WHEN 'S' THEN ISNULL(C.S_Modifica, 0)
                        WHEN 'T' THEN ISNULL(C.T_Modifica, 0)
                        WHEN 'F' THEN ISNULL(C.F_Modifica, 0)
                        ELSE 0
                    END
                ) AS modifica,
                CONVERT
                (
                    bit,
                    CASE @TipoTransaccion
                        WHEN 'E' THEN ISNULL(C.E_Autoriza, 0)
                        WHEN 'S' THEN ISNULL(C.S_Autoriza, 0)
                        WHEN 'T' THEN ISNULL(C.T_Autoriza, 0)
                        ELSE 0
                    END
                ) AS autoriza,
                CONVERT
                (
                    bit,
                    CASE @TipoTransaccion
                        WHEN 'E' THEN ISNULL(C.E_Procesa, 0)
                        WHEN 'S' THEN ISNULL(C.S_Procesa, 0)
                        WHEN 'T' THEN ISNULL(C.T_Procesa, 0)
                        WHEN 'F' THEN ISNULL(C.F_Procesa, 0)
                        ELSE 0
                    END
                ) AS procesa
            FROM USUARIOS U
            LEFT JOIN PV_BODEGAS_PERMISOS C
                ON U.nombre = C.usuario
               AND C.cod_bodega = @CodBodega
            WHERE U.estado = 'A'
            ORDER BY U.nombre ASC;
            """;

        private const string QueryActualizarPermiso = """
            IF NOT EXISTS
            (
                SELECT 1
                FROM PV_BODEGAS_PERMISOS WITH (UPDLOCK, HOLDLOCK)
                WHERE usuario = @Usuario
                  AND cod_bodega = @CodBodega
            )
            BEGIN
                INSERT INTO PV_BODEGAS_PERMISOS
                (
                    usuario,
                    cod_bodega,
                    E_Modifica,
                    E_Autoriza,
                    E_Procesa,
                    S_Modifica,
                    S_Autoriza,
                    S_Procesa,
                    T_Modifica,
                    T_Autoriza,
                    T_Procesa,
                    F_Modifica,
                    F_Procesa
                )
                VALUES
                (
                    @Usuario,
                    @CodBodega,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0,
                    0
                );
            END;

            UPDATE PV_BODEGAS_PERMISOS
            SET
                E_Modifica =
                    CASE
                        WHEN @TipoTransaccion = 'E'
                         AND @Permiso = 'MODIFICA'
                            THEN @Valor
                        ELSE E_Modifica
                    END,
                E_Autoriza =
                    CASE
                        WHEN @TipoTransaccion = 'E'
                         AND @Permiso = 'AUTORIZA'
                            THEN @Valor
                        ELSE E_Autoriza
                    END,
                E_Procesa =
                    CASE
                        WHEN @TipoTransaccion = 'E'
                         AND @Permiso = 'PROCESA'
                            THEN @Valor
                        ELSE E_Procesa
                    END,
                S_Modifica =
                    CASE
                        WHEN @TipoTransaccion = 'S'
                         AND @Permiso = 'MODIFICA'
                            THEN @Valor
                        ELSE S_Modifica
                    END,
                S_Autoriza =
                    CASE
                        WHEN @TipoTransaccion = 'S'
                         AND @Permiso = 'AUTORIZA'
                            THEN @Valor
                        ELSE S_Autoriza
                    END,
                S_Procesa =
                    CASE
                        WHEN @TipoTransaccion = 'S'
                         AND @Permiso = 'PROCESA'
                            THEN @Valor
                        ELSE S_Procesa
                    END,
                T_Modifica =
                    CASE
                        WHEN @TipoTransaccion = 'T'
                         AND @Permiso = 'MODIFICA'
                            THEN @Valor
                        ELSE T_Modifica
                    END,
                T_Autoriza =
                    CASE
                        WHEN @TipoTransaccion = 'T'
                         AND @Permiso = 'AUTORIZA'
                            THEN @Valor
                        ELSE T_Autoriza
                    END,
                T_Procesa =
                    CASE
                        WHEN @TipoTransaccion = 'T'
                         AND @Permiso = 'PROCESA'
                            THEN @Valor
                        ELSE T_Procesa
                    END,
                F_Modifica =
                    CASE
                        WHEN @TipoTransaccion = 'F'
                         AND @Permiso = 'MODIFICA'
                            THEN @Valor
                        ELSE F_Modifica
                    END,
                F_Procesa =
                    CASE
                        WHEN @TipoTransaccion = 'F'
                         AND @Permiso = 'PROCESA'
                            THEN @Valor
                        ELSE F_Procesa
                    END
            WHERE usuario = @Usuario
              AND cod_bodega = @CodBodega;
            """;

        private readonly IConfiguration _config;

        public FrmInvBodegasDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene el listado de bodegas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de bodegas.</returns>
        public ErrorDto<List<BodegasDto>> INV_Bodegas_Lista_Obtener(
            int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<BodegasDto>(
                CreatePortalDb(),
                CodEmpresa,
                QueryObtenerBodegas);

            return result.Code == 0
                ? result
                : DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarBodegas,
                    result.Code.GetValueOrDefault(-1),
                    new List<BodegasDto>());
        }

        /// <summary>
        /// Obtiene una bodega mediante su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_bodega">Código de bodega.</param>
        /// <returns>Información de la bodega.</returns>
        public ErrorDto<BodegasDto> INV_Bodegas_Codigo_Obtener(
            int CodEmpresa,
            string cod_bodega)
        {
            if (string.IsNullOrWhiteSpace(cod_bodega))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo de la bodega es requerido.",
                    -2,
                    (BodegasDto)null);
            }

            var result = DbHelper.ExecuteSingleQuery<BodegasDto>(
                CreatePortalDb(),
                CodEmpresa,
                QueryObtenerBodega,
                null,
                new
                {
                    CodBodega = cod_bodega.Trim()
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarBodega,
                    result.Code.GetValueOrDefault(-1),
                    (BodegasDto)null);
        }

        /// <summary>
        /// Obtiene la bodega anterior o siguiente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="consecutivo">Código actual de la bodega.</param>
        /// <param name="tipo">Dirección ascendente o descendente.</param>
        /// <returns>Bodega encontrada.</returns>
        public ErrorDto<BodegasDto> INV_Bodegas_Navegacion_Obtener(
            int CodEmpresa,
            string consecutivo,
            string tipo)
        {
            if (string.IsNullOrWhiteSpace(consecutivo))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo de la bodega es requerido.",
                    -2,
                    (BodegasDto)null);
            }

            string direccion = tipo?.Trim().ToLowerInvariant() ?? string.Empty;

            if (direccion is not ("asc" or "desc"))
            {
                return DbHelper.CreateErrorResponse(
                    "La direcci&oacute;n de navegaci&oacute;n no es v&aacute;lida.",
                    -2,
                    (BodegasDto)null);
            }

            var result = DbHelper.ExecuteSingleQuery<BodegasDto>(
                CreatePortalDb(),
                CodEmpresa,
                QueryNavegarBodegas,
                null,
                new
                {
                    CodBodega = consecutivo.Trim(),
                    Tipo = direccion
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorNavegarBodegas,
                    result.Code.GetValueOrDefault(-1),
                    (BodegasDto)null);
        }

        /// <summary>
        /// Obtiene los permisos de los usuarios para una bodega.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_bodega">Código de bodega.</param>
        /// <param name="tipo_transaccion">Tipo de transacción E, S, T o F.</param>
        /// <returns>Listado de usuarios y permisos.</returns>
        public ErrorDto<List<PermisosBodegasDto>> INV_Bodegas_Permisos_Obtener(
            int CodEmpresa,
            string cod_bodega,
            string tipo_transaccion)
        {
            if (string.IsNullOrWhiteSpace(cod_bodega))
            {
                return DbHelper.CreateErrorResponse(
                    "El c&oacute;digo de la bodega es requerido.",
                    -2,
                    new List<PermisosBodegasDto>());
            }

            string tipoTransaccion = TipoTransaccionNormalizar(
                tipo_transaccion);

            if (string.IsNullOrEmpty(tipoTransaccion))
            {
                return DbHelper.CreateErrorResponse(
                    "El tipo de transacci&oacute;n no es v&aacute;lido.",
                    -2,
                    new List<PermisosBodegasDto>());
            }

            var result = DbHelper.ExecuteListQuery<PermisosBodegasDto>(
                CreatePortalDb(),
                CodEmpresa,
                QueryObtenerPermisos,
                new
                {
                    CodBodega = cod_bodega.Trim(),
                    TipoTransaccion = tipoTransaccion
                });

            return result.Code == 0
                ? result
                : DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarPermisos,
                    result.Code.GetValueOrDefault(-1),
                    new List<PermisosBodegasDto>());
        }

        /// <summary>
        /// Registra una bodega.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Información de la bodega.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto INV_Bodegas_Registrar(
            int CodEmpresa,
            BodegasDto request)
        {
            string validacion = BodegaValidar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(validacion, -2);
            }

            var parametros = BodegaParametrosObtener(request);

            var result = DbHelper.WithConn(
                CreatePortalDb(),
                CodEmpresa,
                connection =>
                {
                    int existe = connection.QueryFirstOrDefault<int>(
                        QueryExisteBodega,
                        parametros);

                    if (existe > 0)
                    {
                        return DbHelper.ErrorResponse(
                            "El c&oacute;digo de la bodega ya existe.",
                            -2);
                    }

                    connection.Execute(
                        QueryRegistrarBodega,
                        parametros);

                    return DbHelper.OkResponse(MensajeOk);
                });

            return ResultObtener(
                result,
                ErrorRegistrarBodega);
        }

        /// <summary>
        /// Actualiza una bodega.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Información de la bodega.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto INV_Bodegas_Actualizar(
            int CodEmpresa,
            BodegasDto request)
        {
            string validacion = BodegaValidar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(validacion, -2);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                QueryActualizarBodega,
                BodegaParametrosObtener(request));

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOk)
                : DbHelper.ErrorResponse(
                    result.Description ?? ErrorActualizarBodega,
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina una bodega y sus permisos asociados.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_bodega">Código de bodega.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto INV_Bodegas_Eliminar(
            int CodEmpresa,
            string cod_bodega)
        {
            if (string.IsNullOrWhiteSpace(cod_bodega))
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo de la bodega es requerido.",
                    -2);
            }

            var result = DbHelper.WithConn(
                CreatePortalDb(),
                CodEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    try
                    {
                        var parametros = new
                        {
                            CodBodega =
                                cod_bodega.Trim()
                        };

                        connection.Execute(
                            QueryEliminarPermisos,
                            parametros,
                            transaction);

                        int registros =
                            connection.Execute(
                                QueryEliminarBodega,
                                parametros,
                                transaction);

                        if (registros == 0)
                        {
                            transaction.Rollback();

                            return DbHelper.ErrorResponse(
                                "La bodega indicada no existe.",
                                -2);
                        }

                        transaction.Commit();

                        return DbHelper.OkResponse(
                            MensajeOk);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            return ResultObtener(
                result,
                ErrorEliminarBodega);
        }

        /// <summary>
        /// Actualiza un permiso de usuario para una bodega.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Información del permiso que se actualizará.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto INV_Bodegas_Permiso_Actualizar(
            int CodEmpresa,
            InvBodegasPermisoActualizarRequest request)
        {
            string validacion = PermisoValidar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    -2);
            }

            string tipoTransaccion =
                TipoTransaccionNormalizar(
                    request.tipo_transaccion);

            string permiso = request.permiso
                .Trim()
                .ToUpperInvariant();

            if (
                tipoTransaccion == "F" &&
                permiso == "AUTORIZA"
            )
            {
                return DbHelper.OkResponse(MensajeOk);
            }

            var parametros = new
            {
                CodBodega =
                    request.cod_bodega.Trim(),
                Usuario =
                    request.usuario.Trim(),
                TipoTransaccion =
                    tipoTransaccion,
                Permiso =
                    permiso,
                Valor =
                    request.valor ? 1 : 0
            };

            var result = DbHelper.WithConn(
                CreatePortalDb(),
                CodEmpresa,
                connection =>
                {
                    connection.Open();

                    using var transaction =
                        connection.BeginTransaction();

                    try
                    {
                        connection.Execute(
                            QueryActualizarPermiso,
                            parametros,
                            transaction);

                        transaction.Commit();

                        return DbHelper.OkResponse(
                            MensajeOk);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            return ResultObtener(
                result,
                ErrorActualizarPermiso);
        }

        /// <summary>
        /// Crea la instancia de acceso a la configuración de base de datos.
        /// </summary>
        /// <returns>Instancia de PortalDB.</returns>
        private PortalDB CreatePortalDb()
        {
            return new PortalDB(_config);
        }

        /// <summary>
        /// Obtiene los parámetros normalizados de una bodega.
        /// </summary>
        /// <param name="request">Información de la bodega.</param>
        /// <returns>Parámetros utilizados por las consultas.</returns>
        private static object BodegaParametrosObtener(
            BodegasDto request)
        {
            return new
            {
                CodBodega = request.cod_bodega.Trim(),
                Descripcion = request.descripcion.Trim().ToUpperInvariant(),
                Observacion = request.observacion?.Trim() ?? string.Empty,
                Estado = request.estado.Trim(),
                PermiteEntradas = request.permite_entradas,
                PermiteSalidas = request.permite_salidas,
                CodCuenta = request.cod_cuenta?.Trim() ?? string.Empty,
                CodCtaIngresosTf = request.cod_cta_ingresostf?.Trim() ?? string.Empty,
                CodCtaGastosTf = request.cod_cta_gastostf?.Trim() ?? string.Empty,
                UtilizaPermisos = request.utiliza_permisos
            };
        }

        /// <summary>
        /// Valida la información requerida de una bodega.
        /// </summary>
        /// <param name="request">Información de la bodega.</param>
        /// <returns>Mensaje de validación.</returns>
        private static string BodegaValidar(
            BodegasDto request)
        {
            if (request is null)
            {
                return "La informaci&oacute;n de la bodega es requerida.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_bodega))
            {
                return "El c&oacute;digo de la bodega es requerido.";
            }

            if (string.IsNullOrWhiteSpace(request.descripcion))
            {
                return "La descripci&oacute;n de la bodega es requerida.";
            }

            if (string.IsNullOrWhiteSpace(request.estado))
            {
                return "El estado de la bodega es requerido.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Valida la información requerida para actualizar un permiso.
        /// </summary>
        /// <param name="request">Información del permiso.</param>
        /// <returns>Mensaje de validación.</returns>
        private static string PermisoValidar(
            InvBodegasPermisoActualizarRequest request)
        {
            if (request is null)
            {
                return "La informaci&oacute;n del permiso es requerida.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_bodega))
            {
                return "El c&oacute;digo de la bodega es requerido.";
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return "El usuario es requerido.";
            }

            string tipoTransaccion = TipoTransaccionNormalizar(
                request.tipo_transaccion);

            if (string.IsNullOrEmpty(tipoTransaccion))
            {
                return "El tipo de transacci&oacute;n no es v&aacute;lido.";
            }

            string permiso = request.permiso?
                .Trim()
                .ToUpperInvariant() ?? string.Empty;

            if (permiso is not ("MODIFICA" or "AUTORIZA" or "PROCESA"))
            {
                return "El permiso indicado no es v&aacute;lido.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Normaliza el tipo de transacción utilizado por el formulario.
        /// </summary>
        /// <param name="tipoTransaccion">Tipo de transacción recibido.</param>
        /// <returns>Código E, S, T o F.</returns>
        private static string TipoTransaccionNormalizar(
            string tipoTransaccion)
        {
            string valor = tipoTransaccion?
                .Trim()
                .ToUpperInvariant() ?? string.Empty;

            return valor switch
            {
                "E" or "ENTRADA" or "ENTRADAS" => "E",
                "S" or "SALIDA" or "SALIDAS" => "S",
                "T" or "TRASLADO" or "TRASLADOS" => "T",
                "F" or "TOMA FISICA" or "TOMA FÍSICA" => "F",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Obtiene el resultado interno generado por DbHelper.
        /// </summary>
        /// <param name="result">Resultado interno de la ejecución.</param>
        /// <param name="mensajeError">Mensaje utilizado cuando ocurre un error.</param>
        /// <returns>Resultado final de la operación.</returns>
        private static ErrorDto ResultObtener(
            ErrorDto<ErrorDto> result,
            string mensajeError)
        {
            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(
                    result.Description ?? mensajeError,
                    result.Code.GetValueOrDefault(-1));
        }
    }
}