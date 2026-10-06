using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslTablasTiposDb
    {
        private const int ModuloFosol = 22;
        private const int CodigoValidacion = -2;

        private const string AccionGuardar =
            "GUARDAR";

        private const string AccionActualizar =
            "ACTUALIZAR";

        private const string AccionEliminar =
            "ELIMINAR";

        private const string MovimientoRegistrar =
            "Registra";

        private const string MovimientoModificar =
            "Modifica";

        private const string MovimientoEliminar =
            "Elimina";

        private const string MensajeTipoInvalido =
            "El tipo de cat&aacute;logo no es v&aacute;lido.";

        private const string MensajeCodigoRequerido =
            "El c&oacute;digo del tipo es requerido.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        private const string MensajeNoEncontrado =
            "No se encontr&oacute; el tipo indicado.";

        private const string MensajeErrorProceso =
            "Ocurri&oacute; un error al procesar el tipo.";

        private readonly PortalDB _portalDb;

        private readonly MSecurityMainDb
            _securityMainDb;

        public FrmFslTablasTiposDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);

            _securityMainDb =
                new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene los registros del cat&aacute;logo
        /// seleccionado.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="filtros">
        /// Filtros, ordenamiento y paginaci&oacute;n.
        /// </param>
        /// <returns>
        /// Lista paginada de tipos.
        /// </returns>
        public ErrorDto<
            FslListaPaginadaDto<FslTablaTipoDto>>
            FSL_TablasTipos_Lista_Obtener(
                int CodEmpresa,
                FslTablasTiposFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(filtros);

            var tipo =
                FSL_TablasTipos_Tipo_Normalizar(
                    filtros.tipo);

            if (!FSL_TablasTipos_Tipo_Valido(tipo))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeTipoInvalido,
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslTablaTipoDto>());
            }

            var filtro =
                FSL_TablasTipos_Texto_Normalizar(
                    filtros.filtro);

            var like =
                string.IsNullOrWhiteSpace(filtro)
                    ? null
                    : $"%{filtro}%";

            var offset = Math.Max(
                filtros.pagina,
                0);

            var fetch = Math.Clamp(
                filtros.paginacion,
                1,
                500);

            var sortField =
                FSL_TablasTipos_Orden_Campo_Obtener(
                    filtros.sort_field);

            var sortOrder =
                filtros.sort_order == -1
                    ? -1
                    : 1;

            const string sql = """
                SET NOCOUNT ON;

                DECLARE @catalogo TABLE
                (
                    codigo NVARCHAR(100) NOT NULL,
                    descripcion NVARCHAR(MAX) NOT NULL,
                    activa BIT NOT NULL
                );

                IF @tipo = 'G'
                BEGIN
                    INSERT INTO @catalogo
                    (
                        codigo,
                        descripcion,
                        activa
                    )
                    SELECT
                        RTRIM(
                            ISNULL(COD_GESTION, '')
                        ),
                        ISNULL(descripcion, ''),
                        CAST(
                            ISNULL(Activa, 0)
                            AS BIT
                        )
                    FROM FSL_TIPOS_GESTIONES;
                END
                ELSE IF @tipo = 'A'
                BEGIN
                    INSERT INTO @catalogo
                    (
                        codigo,
                        descripcion,
                        activa
                    )
                    SELECT
                        RTRIM(
                            ISNULL(COD_APELACION, '')
                        ),
                        ISNULL(descripcion, ''),
                        CAST(
                            ISNULL(Activa, 0)
                            AS BIT
                        )
                    FROM FSL_TIPOS_APELACIONES;
                END
                ELSE IF @tipo = 'E'
                BEGIN
                    INSERT INTO @catalogo
                    (
                        codigo,
                        descripcion,
                        activa
                    )
                    SELECT
                        RTRIM(
                            ISNULL(COD_ENFERMEDAD, '')
                        ),
                        ISNULL(descripcion, ''),
                        CAST(
                            ISNULL(Activa, 0)
                            AS BIT
                        )
                    FROM FSL_TIPOS_ENFERMEDADES;
                END;

                SELECT COUNT(*)
                FROM @catalogo C
                WHERE
                    @like IS NULL
                    OR C.codigo LIKE @like
                    OR C.descripcion LIKE @like;

                SELECT
                    C.codigo,
                    C.descripcion,
                    C.activa
                FROM @catalogo C
                WHERE
                    @like IS NULL
                    OR C.codigo LIKE @like
                    OR C.descripcion LIKE @like
                ORDER BY
                    CASE
                        WHEN @sortField = 'codigo'
                            AND @sortOrder = 1
                        THEN C.codigo
                    END ASC,
                    CASE
                        WHEN @sortField = 'codigo'
                            AND @sortOrder = -1
                        THEN C.codigo
                    END DESC,
                    CASE
                        WHEN @sortField = 'descripcion'
                            AND @sortOrder = 1
                        THEN C.descripcion
                    END ASC,
                    CASE
                        WHEN @sortField = 'descripcion'
                            AND @sortOrder = -1
                        THEN C.descripcion
                    END DESC,
                    CASE
                        WHEN @sortField = 'activa'
                            AND @sortOrder = 1
                        THEN C.activa
                    END ASC,
                    CASE
                        WHEN @sortField = 'activa'
                            AND @sortOrder = -1
                        THEN C.activa
                    END DESC,
                    C.codigo ASC
                OFFSET @offset ROWS
                FETCH NEXT @fetch ROWS ONLY;
                """;

            return DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    using var reader =
                        connection.QueryMultiple(
                            sql,
                            new
                            {
                                tipo,
                                like,
                                offset,
                                fetch,
                                sortField,
                                sortOrder
                            });

                    return new FslListaPaginadaDto<
                        FslTablaTipoDto>
                    {
                        total =
                            reader.ReadFirst<int>(),
                        lista = reader
                            .Read<FslTablaTipoDto>()
                            .ToList()
                    };
                });
        }

        /// <summary>
        /// Registra un tipo nuevo o actualiza el existente,
        /// conservando el comportamiento de fxGuardar en VB6.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n del tipo.
        /// </param>
        /// <returns>
        /// Resultado de la operaci&oacute;n.
        /// </returns>
        public ErrorDto
            FSL_TablasTipos_Tipo_Registrar(
                int CodEmpresa,
                FslTablaTipoGuardarRequest request)
        {
            return FSL_TablasTipos_Tipo_Guardar(
                CodEmpresa,
                request,
                AccionGuardar);
        }

        /// <summary>
        /// Actualiza un tipo existente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n del tipo.
        /// </param>
        /// <returns>
        /// Resultado de la actualizaci&oacute;n.
        /// </returns>
        public ErrorDto
            FSL_TablasTipos_Tipo_Actualizar(
                int CodEmpresa,
                FslTablaTipoGuardarRequest request)
        {
            return FSL_TablasTipos_Tipo_Guardar(
                CodEmpresa,
                request,
                AccionActualizar);
        }

        /// <summary>
        /// Elimina un tipo del cat&aacute;logo seleccionado.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="tipo">
        /// Tipo de cat&aacute;logo.
        /// </param>
        /// <param name="codigo">
        /// C&oacute;digo del registro.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <returns>
        /// Resultado de la eliminaci&oacute;n.
        /// </returns>
        public ErrorDto
            FSL_TablasTipos_Tipo_Eliminar(
                int CodEmpresa,
                string? tipo,
                string? codigo,
                string? usuario)
        {
            var operacion =
                new FslTablaTipoOperacion
                {
                    Accion = AccionEliminar,
                    Tipo =
                        FSL_TablasTipos_Tipo_Normalizar(
                            tipo),
                    Codigo =
                        FSL_TablasTipos_Texto_Normalizar(
                            codigo),
                    Usuario =
                        FSL_TablasTipos_Texto_Normalizar(
                            usuario)
                            .ToUpperInvariant()
                };

            var validacion =
                FSL_TablasTipos_Operacion_Validar(
                    operacion);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            return FSL_TablasTipos_Mantenimiento_Ejecutar(
                CodEmpresa,
                operacion);
        }

        /// <summary>
        /// Prepara el registro o actualizaci&oacute;n de un
        /// tipo.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="request">
        /// Informaci&oacute;n del tipo.
        /// </param>
        /// <param name="accion">
        /// Acci&oacute;n que debe ejecutarse.
        /// </param>
        /// <returns>
        /// Resultado de la operaci&oacute;n.
        /// </returns>
        private ErrorDto
            FSL_TablasTipos_Tipo_Guardar(
                int CodEmpresa,
                FslTablaTipoGuardarRequest request,
                string accion)
        {
            ArgumentNullException.ThrowIfNull(request);

            var operacion =
                new FslTablaTipoOperacion
                {
                    Accion = accion,
                    Tipo =
                        FSL_TablasTipos_Tipo_Normalizar(
                            request.tipo),
                    Codigo =
                        FSL_TablasTipos_Texto_Normalizar(
                            request.codigo),
                    Descripcion =
                        FSL_TablasTipos_Texto_Normalizar(
                            request.descripcion),
                    Activa = request.activa,
                    Usuario =
                        FSL_TablasTipos_Texto_Normalizar(
                            request.usuario)
                            .ToUpperInvariant()
                };

            var validacion =
                FSL_TablasTipos_Operacion_Validar(
                    operacion);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            return FSL_TablasTipos_Mantenimiento_Ejecutar(
                CodEmpresa,
                operacion);
        }

        /// <summary>
        /// Ejecuta el mantenimiento solicitado sobre el
        /// cat&aacute;logo seleccionado.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="operacion">
        /// Informaci&oacute;n normalizada de la operaci&oacute;n.
        /// </param>
        /// <returns>
        /// Resultado del mantenimiento.
        /// </returns>
        private ErrorDto
            FSL_TablasTipos_Mantenimiento_Ejecutar(
                int CodEmpresa,
                FslTablaTipoOperacion operacion)
        {
            const string sql = """
                SET NOCOUNT ON;
                SET XACT_ABORT ON;

                DECLARE @filas INT = 0;
                DECLARE @movimiento VARCHAR(10) = '';

                BEGIN TRANSACTION;

                IF @tipo = 'G'
                BEGIN
                    IF @accion = @accionGuardar
                    BEGIN
                        IF EXISTS
                        (
                            SELECT 1
                            FROM FSL_TIPOS_GESTIONES
                                WITH (UPDLOCK, HOLDLOCK)
                            WHERE COD_GESTION = @codigo
                        )
                        BEGIN
                            UPDATE FSL_TIPOS_GESTIONES
                            SET
                                descripcion = @descripcion,
                                Activa = @activa
                            WHERE COD_GESTION = @codigo;

                            SET @filas = @@ROWCOUNT;
                            SET @movimiento =
                                @movimientoModificar;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO FSL_TIPOS_GESTIONES
                            (
                                COD_GESTION,
                                descripcion,
                                Activa,
                                registro_fecha,
                                registro_usuario
                            )
                            VALUES
                            (
                                @codigo,
                                @descripcion,
                                @activa,
                                GETDATE(),
                                @usuario
                            );

                            SET @filas = @@ROWCOUNT;
                            SET @movimiento =
                                @movimientoRegistrar;
                        END;
                    END
                    ELSE IF @accion = @accionActualizar
                    BEGIN
                        UPDATE FSL_TIPOS_GESTIONES
                        SET
                            descripcion = @descripcion,
                            Activa = @activa
                        WHERE COD_GESTION = @codigo;

                        SET @filas = @@ROWCOUNT;
                        SET @movimiento =
                            @movimientoModificar;
                    END
                    ELSE IF @accion = @accionEliminar
                    BEGIN
                        DELETE FROM FSL_TIPOS_GESTIONES
                        WHERE COD_GESTION = @codigo;

                        SET @filas = @@ROWCOUNT;
                        SET @movimiento =
                            @movimientoEliminar;
                    END;
                END
                ELSE IF @tipo = 'A'
                BEGIN
                    IF @accion = @accionGuardar
                    BEGIN
                        IF EXISTS
                        (
                            SELECT 1
                            FROM FSL_TIPOS_APELACIONES
                                WITH (UPDLOCK, HOLDLOCK)
                            WHERE COD_APELACION = @codigo
                        )
                        BEGIN
                            UPDATE FSL_TIPOS_APELACIONES
                            SET
                                descripcion = @descripcion,
                                Activa = @activa
                            WHERE COD_APELACION = @codigo;

                            SET @filas = @@ROWCOUNT;
                            SET @movimiento =
                                @movimientoModificar;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO FSL_TIPOS_APELACIONES
                            (
                                COD_APELACION,
                                descripcion,
                                Activa,
                                registro_fecha,
                                registro_usuario
                            )
                            VALUES
                            (
                                @codigo,
                                @descripcion,
                                @activa,
                                GETDATE(),
                                @usuario
                            );

                            SET @filas = @@ROWCOUNT;
                            SET @movimiento =
                                @movimientoRegistrar;
                        END;
                    END
                    ELSE IF @accion = @accionActualizar
                    BEGIN
                        UPDATE FSL_TIPOS_APELACIONES
                        SET
                            descripcion = @descripcion,
                            Activa = @activa
                        WHERE COD_APELACION = @codigo;

                        SET @filas = @@ROWCOUNT;
                        SET @movimiento =
                            @movimientoModificar;
                    END
                    ELSE IF @accion = @accionEliminar
                    BEGIN
                        DELETE FROM FSL_TIPOS_APELACIONES
                        WHERE COD_APELACION = @codigo;

                        SET @filas = @@ROWCOUNT;
                        SET @movimiento =
                            @movimientoEliminar;
                    END;
                END
                ELSE IF @tipo = 'E'
                BEGIN
                    IF @accion = @accionGuardar
                    BEGIN
                        IF EXISTS
                        (
                            SELECT 1
                            FROM FSL_TIPOS_ENFERMEDADES
                                WITH (UPDLOCK, HOLDLOCK)
                            WHERE COD_ENFERMEDAD = @codigo
                        )
                        BEGIN
                            UPDATE FSL_TIPOS_ENFERMEDADES
                            SET
                                descripcion = @descripcion,
                                Activa = @activa
                            WHERE COD_ENFERMEDAD = @codigo;

                            SET @filas = @@ROWCOUNT;
                            SET @movimiento =
                                @movimientoModificar;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO FSL_TIPOS_ENFERMEDADES
                            (
                                COD_ENFERMEDAD,
                                descripcion,
                                Activa,
                                registro_fecha,
                                registro_usuario
                            )
                            VALUES
                            (
                                @codigo,
                                @descripcion,
                                @activa,
                                GETDATE(),
                                @usuario
                            );

                            SET @filas = @@ROWCOUNT;
                            SET @movimiento =
                                @movimientoRegistrar;
                        END;
                    END
                    ELSE IF @accion = @accionActualizar
                    BEGIN
                        UPDATE FSL_TIPOS_ENFERMEDADES
                        SET
                            descripcion = @descripcion,
                            Activa = @activa
                        WHERE COD_ENFERMEDAD = @codigo;

                        SET @filas = @@ROWCOUNT;
                        SET @movimiento =
                            @movimientoModificar;
                    END
                    ELSE IF @accion = @accionEliminar
                    BEGIN
                        DELETE FROM FSL_TIPOS_ENFERMEDADES
                        WHERE COD_ENFERMEDAD = @codigo;

                        SET @filas = @@ROWCOUNT;
                        SET @movimiento =
                            @movimientoEliminar;
                    END;
                END;

                COMMIT TRANSACTION;

                SELECT
                    @filas AS Filas,
                    @movimiento AS Movimiento;
                """;

            var response = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    connection.QuerySingle<
                        FslTablaTipoOperacionResultado>(
                            sql,
                            new
                            {
                                accion =
                                    operacion.Accion,
                                tipo =
                                    operacion.Tipo,
                                codigo =
                                    operacion.Codigo,
                                descripcion =
                                    operacion.Descripcion,
                                activa =
                                    operacion.Activa,
                                usuario =
                                    operacion.Usuario,
                                accionGuardar =
                                    AccionGuardar,
                                accionActualizar =
                                    AccionActualizar,
                                accionEliminar =
                                    AccionEliminar,
                                movimientoRegistrar =
                                    MovimientoRegistrar,
                                movimientoModificar =
                                    MovimientoModificar,
                                movimientoEliminar =
                                    MovimientoEliminar
                            }));

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    MensajeErrorProceso);
            }

            var resultado = response.Result;

            if (
                resultado is null ||
                resultado.Filas <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeNoEncontrado,
                    CodigoValidacion);
            }

            var movimiento =
                FSL_TablasTipos_Texto_Normalizar(
                    resultado.Movimiento);

            FSL_TablasTipos_Bitacora_Registrar(
                CodEmpresa,
                operacion,
                movimiento);

            return DbHelper.OkResponse(
                FSL_TablasTipos_Mensaje_Exito_Obtener(
                    movimiento));
        }

        /// <summary>
        /// Valida la operaci&oacute;n normalizada antes de
        /// acceder a la base de datos.
        /// </summary>
        /// <param name="operacion">
        /// Informaci&oacute;n de la operaci&oacute;n.
        /// </param>
        /// <returns>
        /// Mensaje de validaci&oacute;n o una cadena vac&iacute;a.
        /// </returns>
        private static string
            FSL_TablasTipos_Operacion_Validar(
                FslTablaTipoOperacion operacion)
        {
            if (!FSL_TablasTipos_Tipo_Valido(
                operacion.Tipo))
            {
                return MensajeTipoInvalido;
            }

            if (string.IsNullOrWhiteSpace(
                operacion.Codigo))
            {
                return MensajeCodigoRequerido;
            }

            return string.IsNullOrWhiteSpace(
                operacion.Usuario)
                    ? MensajeUsuarioRequerido
                    : string.Empty;
        }

        /// <summary>
        /// Determina si el tipo corresponde a uno de los
        /// cat&aacute;logos utilizados por el formulario VB6.
        /// </summary>
        /// <param name="tipo">
        /// Tipo normalizado.
        /// </param>
        /// <returns>
        /// Verdadero cuando el tipo es v&aacute;lido.
        /// </returns>
        private static bool
            FSL_TablasTipos_Tipo_Valido(
                string? tipo)
        {
            var valor =
                FSL_TablasTipos_Tipo_Normalizar(
                    tipo);

            return valor is "G" or "A" or "E";
        }

        /// <summary>
        /// Normaliza el tipo de cat&aacute;logo.
        /// </summary>
        /// <param name="tipo">
        /// Tipo recibido.
        /// </param>
        /// <returns>
        /// Tipo normalizado.
        /// </returns>
        private static string
            FSL_TablasTipos_Tipo_Normalizar(
                string? tipo)
        {
            return FSL_TablasTipos_Texto_Normalizar(
                tipo)
                .ToUpperInvariant();
        }

        /// <summary>
        /// Normaliza un valor textual recibido desde una fuente
        /// externa.
        /// </summary>
        /// <param name="valor">
        /// Valor recibido.
        /// </param>
        /// <returns>
        /// Texto sin espacios exteriores.
        /// </returns>
        private static string
            FSL_TablasTipos_Texto_Normalizar(
                string? valor)
        {
            return valor?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Obtiene el campo permitido para ordenar la lista.
        /// </summary>
        /// <param name="sortField">
        /// Campo solicitado.
        /// </param>
        /// <returns>
        /// Campo de ordenamiento permitido.
        /// </returns>
        private static string
            FSL_TablasTipos_Orden_Campo_Obtener(
                string? sortField)
        {
            return FSL_TablasTipos_Texto_Normalizar(
                sortField)
                .ToLowerInvariant() switch
            {
                "descripcion" => "descripcion",
                "activa" => "activa",
                _ => "codigo"
            };
        }

        /// <summary>
        /// Registra el movimiento en la bit&aacute;cora general.
        /// </summary>
        /// <param name="CodEmpresa">
        /// C&oacute;digo de empresa.
        /// </param>
        /// <param name="operacion">
        /// Informaci&oacute;n de la operaci&oacute;n.
        /// </param>
        /// <param name="movimiento">
        /// Movimiento ejecutado.
        /// </param>
        private void
            FSL_TablasTipos_Bitacora_Registrar(
                int CodEmpresa,
                FslTablaTipoOperacion operacion,
                string movimiento)
        {
            var descripcion =
                FSL_TablasTipos_Tipo_Descripcion_Obtener(
                    operacion.Tipo);

            _ = _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario =
                        operacion.Usuario,
                    Modulo = ModuloFosol,
                    Movimiento = movimiento,
                    DetalleMovimiento =
                        $"Tipos de {descripcion} Id.:{operacion.Codigo}"
                });
        }

        /// <summary>
        /// Obtiene la descripci&oacute;n mostrada por el
        /// formulario VB6 para el cat&aacute;logo.
        /// </summary>
        /// <param name="tipo">
        /// Tipo de cat&aacute;logo.
        /// </param>
        /// <returns>
        /// Descripci&oacute;n del cat&aacute;logo.
        /// </returns>
        private static string
            FSL_TablasTipos_Tipo_Descripcion_Obtener(
                string tipo)
        {
            return tipo switch
            {
                "A" => "Apelaciones",
                "E" => "Enfermedades",
                _ => "Gestiones"
            };
        }

        /// <summary>
        /// Obtiene el mensaje correspondiente al movimiento
        /// ejecutado.
        /// </summary>
        /// <param name="movimiento">
        /// Movimiento ejecutado.
        /// </param>
        /// <returns>
        /// Mensaje de resultado satisfactorio.
        /// </returns>
        private static string
            FSL_TablasTipos_Mensaje_Exito_Obtener(
                string movimiento)
        {
            return movimiento switch
            {
                MovimientoRegistrar =>
                    "Tipo registrado correctamente.",
                MovimientoEliminar =>
                    "Tipo eliminado correctamente.",
                _ =>
                    "Tipo actualizado correctamente."
            };
        }

        private sealed class FslTablaTipoOperacion
        {
            public string Accion { get; init; } =
                string.Empty;

            public string Tipo { get; init; } =
                string.Empty;

            public string Codigo { get; init; } =
                string.Empty;

            public string Descripcion { get; init; } =
                string.Empty;

            public bool Activa { get; init; }

            public string Usuario { get; init; } =
                string.Empty;
        }

        private sealed class
            FslTablaTipoOperacionResultado
        {
            public int Filas { get; init; }

            public string Movimiento { get; init; } =
                string.Empty;
        }
    }
}