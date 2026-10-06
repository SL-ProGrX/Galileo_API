using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslTablaDevolucionesDb
    {
        private const int ModuloFosol = 22;
        private const int CodigoValidacion = -2;

        private const string MensajeCodigoRequerido =
            "El código de la devolución es requerido.";

        private const string MensajeFechaInicioRequerida =
            "La fecha inicial es requerida.";

        private const string MensajeFechaCorteRequerida =
            "La fecha de corte es requerida.";

        private const string MensajeGarantiaRequerida =
            "La garantía es requerida.";

        private const string MensajeBaseInvalida =
            "La base de aplicación no es válida.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmFslTablaDevolucionesDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene las garantías utilizadas por la tabla
        /// de devoluciones.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <returns>
        /// Catálogo de garantías.
        /// </returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            FSL_TablaDevoluciones_Garantias_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    RTRIM(ISNULL(G.Garantia, '')) AS item,
                    RTRIM(ISNULL(G.Garantia, ''))
                        + ' - '
                        + RTRIM(ISNULL(G.descripcion, ''))
                        AS descripcion
                FROM CRD_Garantia_Tipos G
                ORDER BY G.Garantia;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel>(
                    _portalDb,
                    CodEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene la tabla de devoluciones aplicando filtros,
        /// ordenamiento y paginación.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <param name="filtros">
        /// Filtros y configuración de la tabla.
        /// </param>
        /// <returns>
        /// Lista paginada de devoluciones.
        /// </returns>
        public ErrorDto<
            FslListaPaginadaDto<FslTablaDevolucionDto>>
            FSL_TablaDevoluciones_Lista_Obtener(
                int CodEmpresa,
                FslTablaDevolucionesFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(filtros);

            var textoFiltro = filtros.filtro.Trim();

            var like =
                string.IsNullOrWhiteSpace(textoFiltro)
                    ? null
                    : $"%{textoFiltro}%";

            var offset = Math.Max(filtros.pagina, 0);

            var fetch = Math.Clamp(
                filtros.paginacion,
                1,
                500);

            var sortField =
                FSL_TablaDevoluciones_Orden_Campo_Obtener(
                    filtros.sort_field);

            var sortOrder =
                filtros.sort_order == -1
                    ? -1
                    : 1;

            const string sql = """
                SELECT COUNT(*)
                FROM FSL_TABLA_DEVOLUCIONES F
                INNER JOIN CRD_Garantia_Tipos G
                    ON F.Garantia = G.Garantia
                WHERE
                    @like IS NULL
                    OR CONVERT(
                        VARCHAR(20),
                        F.COD_DEVOLUCION
                    ) LIKE @like
                    OR CONVERT(
                        VARCHAR(10),
                        F.Fecha_Inicio,
                        103
                    ) LIKE @like
                    OR CONVERT(
                        VARCHAR(10),
                        F.Fecha_Corte,
                        103
                    ) LIKE @like
                    OR RTRIM(
                        ISNULL(F.Garantia, '')
                    ) LIKE @like
                    OR RTRIM(
                        ISNULL(G.descripcion, '')
                    ) LIKE @like
                    OR CASE
                        WHEN F.BASE_APLICACION = 'S'
                            THEN 'Saldo'
                        ELSE 'Formalizado'
                    END LIKE @like
                    OR CONVERT(
                        VARCHAR(40),
                        F.Porcentaje
                    ) LIKE @like;

                SELECT
                    ISNULL(
                        F.COD_DEVOLUCION,
                        0
                    ) AS cod_devolucion,
                    F.Fecha_Inicio AS fecha_inicio,
                    F.Fecha_Corte AS fecha_corte,
                    RTRIM(
                        ISNULL(F.Garantia, '')
                    ) AS garantia,
                    RTRIM(
                        ISNULL(F.Garantia, '')
                    )
                        + ' - '
                        + RTRIM(
                            ISNULL(G.descripcion, '')
                        ) AS garantia_descripcion,
                    RTRIM(
                        ISNULL(F.BASE_APLICACION, '')
                    ) AS _base,
                    CASE
                        WHEN F.BASE_APLICACION = 'S'
                            THEN 'Saldo'
                        ELSE 'Formalizado'
                    END AS base_descripcion,
                    CAST(
                        ISNULL(F.Porcentaje, 0)
                        AS DECIMAL(18, 4)
                    ) AS porcentaje,
                    F.Registro_Fecha AS registro_fecha,
                    RTRIM(
                        ISNULL(F.Registro_Usuario, '')
                    ) AS registro_usuario
                FROM FSL_TABLA_DEVOLUCIONES F
                INNER JOIN CRD_Garantia_Tipos G
                    ON F.Garantia = G.Garantia
                WHERE
                    @like IS NULL
                    OR CONVERT(
                        VARCHAR(20),
                        F.COD_DEVOLUCION
                    ) LIKE @like
                    OR CONVERT(
                        VARCHAR(10),
                        F.Fecha_Inicio,
                        103
                    ) LIKE @like
                    OR CONVERT(
                        VARCHAR(10),
                        F.Fecha_Corte,
                        103
                    ) LIKE @like
                    OR RTRIM(
                        ISNULL(F.Garantia, '')
                    ) LIKE @like
                    OR RTRIM(
                        ISNULL(G.descripcion, '')
                    ) LIKE @like
                    OR CASE
                        WHEN F.BASE_APLICACION = 'S'
                            THEN 'Saldo'
                        ELSE 'Formalizado'
                    END LIKE @like
                    OR CONVERT(
                        VARCHAR(40),
                        F.Porcentaje
                    ) LIKE @like
                ORDER BY
                    CASE
                        WHEN @sortField = 'cod_devolucion'
                            AND @sortOrder = 1
                        THEN F.COD_DEVOLUCION
                    END ASC,
                    CASE
                        WHEN @sortField = 'cod_devolucion'
                            AND @sortOrder = -1
                        THEN F.COD_DEVOLUCION
                    END DESC,
                    CASE
                        WHEN @sortField = 'fecha_inicio'
                            AND @sortOrder = 1
                        THEN F.Fecha_Inicio
                    END ASC,
                    CASE
                        WHEN @sortField = 'fecha_inicio'
                            AND @sortOrder = -1
                        THEN F.Fecha_Inicio
                    END DESC,
                    CASE
                        WHEN @sortField = 'fecha_corte'
                            AND @sortOrder = 1
                        THEN F.Fecha_Corte
                    END ASC,
                    CASE
                        WHEN @sortField = 'fecha_corte'
                            AND @sortOrder = -1
                        THEN F.Fecha_Corte
                    END DESC,
                    CASE
                        WHEN @sortField = 'garantia'
                            AND @sortOrder = 1
                        THEN F.Garantia
                    END ASC,
                    CASE
                        WHEN @sortField = 'garantia'
                            AND @sortOrder = -1
                        THEN F.Garantia
                    END DESC,
                    CASE
                        WHEN @sortField = '_base'
                            AND @sortOrder = 1
                        THEN F.BASE_APLICACION
                    END ASC,
                    CASE
                        WHEN @sortField = '_base'
                            AND @sortOrder = -1
                        THEN F.BASE_APLICACION
                    END DESC,
                    CASE
                        WHEN @sortField = 'porcentaje'
                            AND @sortOrder = 1
                        THEN F.Porcentaje
                    END ASC,
                    CASE
                        WHEN @sortField = 'porcentaje'
                            AND @sortOrder = -1
                        THEN F.Porcentaje
                    END DESC,
                    F.Fecha_Inicio ASC,
                    F.COD_DEVOLUCION ASC
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
                                like,
                                offset,
                                fetch,
                                sortField,
                                sortOrder
                            });

                    return new FslListaPaginadaDto<
                        FslTablaDevolucionDto>
                    {
                        total = reader.ReadFirst<int>(),
                        lista = reader
                            .Read<FslTablaDevolucionDto>()
                            .ToList()
                    };
                });
        }

        /// <summary>
        /// Registra una nueva devolución y obtiene el
        /// siguiente consecutivo disponible.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <param name="request">
        /// Información de la devolución.
        /// </param>
        /// <returns>
        /// Resultado del registro.
        /// </returns>
        public ErrorDto
            FSL_TablaDevoluciones_Devolucion_Registrar(
                int CodEmpresa,
                FslTablaDevolucionGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validacion =
                FSL_TablaDevoluciones_Request_Validar(
                    request,
                    false);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var garantia =
                FSL_TablaDevoluciones_Garantia_Normalizar(
                    request.garantia);

            var baseAplicacion =
                FSL_TablaDevoluciones_Base_Normalizar(
                    request._base);

            var usuario = request.usuario
                .Trim()
                .ToUpperInvariant();

            const string sql = """
                DECLARE @codigo INT;

                SELECT
                    @codigo =
                        ISNULL(MAX(COD_DEVOLUCION), 0) + 1
                FROM FSL_TABLA_DEVOLUCIONES
                    WITH (UPDLOCK, HOLDLOCK);

                INSERT INTO FSL_TABLA_DEVOLUCIONES
                (
                    COD_DEVOLUCION,
                    Fecha_Inicio,
                    Fecha_Corte,
                    Garantia,
                    Base_Aplicacion,
                    Porcentaje,
                    Registro_Fecha,
                    Registro_Usuario
                )
                VALUES
                (
                    @codigo,
                    @fechaInicio,
                    @fechaCorte,
                    @garantia,
                    @baseAplicacion,
                    @porcentaje,
                    GETDATE(),
                    @usuario
                );

                SELECT @codigo;
                """;

            using var connection =
                DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

            connection.Open();

            using var transaction =
                connection.BeginTransaction();

            try
            {
                var codigo =
                    connection.QuerySingle<int>(
                        sql,
                        new
                        {
                            fechaInicio =
                                request.fecha_inicio,
                            fechaCorte =
                                request.fecha_corte,
                            garantia,
                            baseAplicacion,
                            request.porcentaje,
                            usuario
                        },
                        transaction);

                transaction.Commit();

                FSL_TablaDevoluciones_Bitacora_Registrar(
                    CodEmpresa,
                    usuario,
                    "Registra",
                    codigo);

                return DbHelper.OkResponse(
                    "Devolución registrada correctamente.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                return DbHelper.ErrorResponse(
                    ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una devolución existente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <param name="request">
        /// Información de la devolución.
        /// </param>
        /// <returns>
        /// Resultado de la actualización.
        /// </returns>
        public ErrorDto
            FSL_TablaDevoluciones_Devolucion_Actualizar(
                int CodEmpresa,
                FslTablaDevolucionGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validacion =
                FSL_TablaDevoluciones_Request_Validar(
                    request,
                    true);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var garantia =
                FSL_TablaDevoluciones_Garantia_Normalizar(
                    request.garantia);

            var baseAplicacion =
                FSL_TablaDevoluciones_Base_Normalizar(
                    request._base);

            var usuario = request.usuario
                .Trim()
                .ToUpperInvariant();

            const string sql = """
                UPDATE FSL_TABLA_DEVOLUCIONES
                SET
                    Fecha_Inicio = @fechaInicio,
                    Fecha_Corte = @fechaCorte,
                    Garantia = @garantia,
                    Base_Aplicacion = @baseAplicacion,
                    Porcentaje = @porcentaje
                WHERE COD_DEVOLUCION = @codigo;
                """;

            var operacion =
                new FslTablaDevolucionOperacion
                {
                    Sql = sql,
                    Parametros = new
                    {
                        fechaInicio =
                            request.fecha_inicio,
                        fechaCorte =
                            request.fecha_corte,
                        garantia,
                        baseAplicacion,
                        request.porcentaje,
                        codigo =
                            request.cod_devolucion
                    },
                    Usuario = usuario,
                    Movimiento = "Modifica",
                    Codigo = request.cod_devolucion,
                    MensajeExito =
                        "Devolución actualizada correctamente.",
                    MensajeSinCambios =
                        "No se encontró la devolución indicada."
                };

            return FSL_TablaDevoluciones_Operacion_Ejecutar(
                CodEmpresa,
                operacion);
        }

        /// <summary>
        /// Elimina una devolución existente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <param name="codDevolucion">
        /// Código de la devolución.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <returns>
        /// Resultado de la eliminación.
        /// </returns>
        public ErrorDto
            FSL_TablaDevoluciones_Devolucion_Eliminar(
                int CodEmpresa,
                int codDevolucion,
                string usuario)
        {
            var usuarioRegistro = usuario.Trim();

            if (codDevolucion <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeCodigoRequerido,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(usuarioRegistro))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            const string sql = """
                DELETE FROM FSL_TABLA_DEVOLUCIONES
                WHERE COD_DEVOLUCION = @codDevolucion;
                """;

            var operacion =
                new FslTablaDevolucionOperacion
                {
                    Sql = sql,
                    Parametros = new
                    {
                        codDevolucion
                    },
                    Usuario = usuarioRegistro
                        .ToUpperInvariant(),
                    Movimiento = "Elimina",
                    Codigo = codDevolucion,
                    MensajeExito =
                        "Devolución eliminada correctamente.",
                    MensajeSinCambios =
                        "No se encontró la devolución indicada."
                };

            return FSL_TablaDevoluciones_Operacion_Ejecutar(
                CodEmpresa,
                operacion);
        }

        /// <summary>
        /// Ejecuta una operación de mantenimiento y
        /// registra el movimiento en bitácora.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <param name="operacion">
        /// Información de la operación.
        /// </param>
        /// <returns>
        /// Resultado de la operación.
        /// </returns>
        private ErrorDto
            FSL_TablaDevoluciones_Operacion_Ejecutar(
                int CodEmpresa,
                FslTablaDevolucionOperacion operacion)
        {
            var response =
                DbHelper.ExecuteNonQueryWithResult(
                    _portalDb,
                    CodEmpresa,
                    operacion.Sql,
                    operacion.Parametros);

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurrió un error al procesar la devolución.");
            }

            if (response.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    operacion.MensajeSinCambios,
                    CodigoValidacion);
            }

            FSL_TablaDevoluciones_Bitacora_Registrar(
                CodEmpresa,
                operacion.Usuario,
                operacion.Movimiento,
                operacion.Codigo);

            return DbHelper.OkResponse(
                operacion.MensajeExito);
        }

        /// <summary>
        /// Valida la información requerida para registrar
        /// o actualizar una devolución.
        /// </summary>
        /// <param name="request">
        /// Información de la devolución.
        /// </param>
        /// <param name="requiereCodigo">
        /// Indica si se requiere un código existente.
        /// </param>
        /// <returns>
        /// Mensaje de validación o una cadena vacía.
        /// </returns>
        private static string
            FSL_TablaDevoluciones_Request_Validar(
                FslTablaDevolucionGuardarRequest request,
                bool requiereCodigo)
        {
            if (
                requiereCodigo &&
                request.cod_devolucion <= 0)
            {
                return MensajeCodigoRequerido;
            }

            if (!request.fecha_inicio.HasValue)
            {
                return MensajeFechaInicioRequerida;
            }

            if (!request.fecha_corte.HasValue)
            {
                return MensajeFechaCorteRequerida;
            }

            if (string.IsNullOrWhiteSpace(request.garantia))
            {
                return MensajeGarantiaRequerida;
            }

            if (
                FSL_TablaDevoluciones_Base_Normalizar(
                    request._base) is not ("S" or "F"))
            {
                return MensajeBaseInvalida;
            }

            return string.IsNullOrWhiteSpace(request.usuario)
                ? MensajeUsuarioRequerido
                : string.Empty;
        }

        /// <summary>
        /// Extrae el código de garantía cuando el
        /// cliente envía código y descripción,
        /// conservando el comportamiento de fxCodText en VB6.
        /// </summary>
        /// <param name="garantia">
        /// Garantía recibida.
        /// </param>
        /// <returns>
        /// Código de garantía normalizado.
        /// </returns>
        private static string
            FSL_TablaDevoluciones_Garantia_Normalizar(
                string garantia)
        {
            var valor = garantia.Trim();

            var separador = valor.IndexOf(
                " - ",
                StringComparison.Ordinal);

            return separador > 0
                ? valor[..separador].Trim()
                : valor;
        }

        /// <summary>
        /// Obtiene el primer carácter de la base de
        /// aplicación, conservando el comportamiento de
        /// Mid utilizado por VB6.
        /// </summary>
        /// <param name="baseAplicacion">
        /// Base de aplicación recibida.
        /// </param>
        /// <returns>
        /// Código normalizado de la base.
        /// </returns>
        private static string
            FSL_TablaDevoluciones_Base_Normalizar(
                string baseAplicacion)
        {
            var valor = baseAplicacion
                .Trim()
                .ToUpperInvariant();

            return valor.Length > 0
                ? valor[..1]
                : string.Empty;
        }

        /// <summary>
        /// Obtiene el campo permitido para ordenar la tabla.
        /// </summary>
        /// <param name="sortField">
        /// Campo solicitado por el cliente.
        /// </param>
        /// <returns>
        /// Campo permitido para el ordenamiento.
        /// </returns>
        private static string
            FSL_TablaDevoluciones_Orden_Campo_Obtener(
                string sortField)
        {
            return sortField
                .Trim()
                .ToLowerInvariant() switch
            {
                "cod_devolucion" => "cod_devolucion",
                "fecha_corte" => "fecha_corte",
                "garantia" => "garantia",
                "_base" => "_base",
                "porcentaje" => "porcentaje",
                _ => "fecha_inicio"
            };
        }

        /// <summary>
        /// Registra el movimiento realizado en la bitácora
        /// general, igual que el formulario VB6.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <param name="movimiento">
        /// Movimiento realizado.
        /// </param>
        /// <param name="codigo">
        /// Código de la devolución.
        /// </param>
        private void
            FSL_TablaDevoluciones_Bitacora_Registrar(
                int CodEmpresa,
                string usuario,
                string movimiento,
                int codigo)
        {
            _ = _securityMainDb.Bitacora(
                new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario
                        .Trim()
                        .ToUpperInvariant(),
                    Modulo = ModuloFosol,
                    Movimiento = movimiento,
                    DetalleMovimiento =
                        $"Tabla Devolución Id.:{codigo}"
                });
        }

        private sealed class
            FslTablaDevolucionOperacion
        {
            public string Sql { get; init; } =
                string.Empty;

            public object Parametros { get; init; } =
                new();

            public string Usuario { get; init; } =
                string.Empty;

            public string Movimiento { get; init; } =
                string.Empty;

            public int Codigo { get; init; } = 0;

            public string MensajeExito { get; init; } =
                string.Empty;

            public string MensajeSinCambios { get; init; } =
                string.Empty;
        }
    }
}