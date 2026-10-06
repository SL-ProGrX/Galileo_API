using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed class FrmFslTablaCoberturasDb
    {
        private const int ModuloFosol = 22;
        private const int CodigoValidacion = -2;

        private const string MensajeTipoInvalido =
            "El tipo de tabla de cobertura no es válido.";

        private const string MensajeLineaRequerida =
            "La línea de la cobertura es requerida.";

        private const string MensajeUsuarioRequerido =
            "El usuario es requerido.";

        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _securityMainDb;

        public FrmFslTablaCoberturasDb(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene las coberturas correspondientes al tipo de
        /// tabla seleccionado.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <param name="filtros">
        /// Filtros, ordenamiento y paginación.
        /// </param>
        /// <returns>
        /// Lista paginada de coberturas.
        /// </returns>
        public ErrorDto<
            FslListaPaginadaDto<FslTablaCoberturaDto>>
            FSL_TablaCoberturas_Lista_Obtener(
                int CodEmpresa,
                FslTablaCoberturasFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(filtros);

            var tipo = filtros.tipo
                .Trim()
                .ToUpperInvariant();

            if (!FSL_TablaCoberturas_Tipo_Valido(tipo))
            {
                return DbHelper.CreateErrorResponse(
                    MensajeTipoInvalido,
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslTablaCoberturaDto>());
            }

            var filtro = filtros.filtro.Trim();

            var like =
                string.IsNullOrWhiteSpace(filtro)
                    ? null
                    : $"%{filtro}%";

            var offset = Math.Max(filtros.pagina, 0);

            var fetch = Math.Clamp(
                filtros.paginacion,
                1,
                500);

            var sortField =
                FSL_TablaCoberturas_Orden_Campo_Obtener(
                    filtros.sort_field);

            var sortOrder =
                filtros.sort_order == -1
                    ? -1
                    : 1;

            const string sql = """
                SELECT COUNT(*)
                FROM FSL_TABLAS_APLICACION T
                WHERE T.TIPO = @tipo
                  AND
                  (
                      @like IS NULL
                      OR CONVERT(
                          VARCHAR(20),
                          T.LINEA
                      ) LIKE @like
                      OR CONVERT(
                          VARCHAR(20),
                          T.MES_INICIO
                      ) LIKE @like
                      OR CONVERT(
                          VARCHAR(20),
                          T.MES_CORTE
                      ) LIKE @like
                      OR CONVERT(
                          VARCHAR(40),
                          T.COBERTURA
                      ) LIKE @like
                  );

                SELECT
                    ISNULL(T.LINEA, 0) AS linea,
                    ISNULL(T.MES_INICIO, 0) AS mes_inicio,
                    ISNULL(T.MES_CORTE, 0) AS mes_corte,
                    CAST(
                        ISNULL(T.COBERTURA, 0)
                        AS DECIMAL(18, 4)
                    ) AS cobertura
                FROM FSL_TABLAS_APLICACION T
                WHERE T.TIPO = @tipo
                  AND
                  (
                      @like IS NULL
                      OR CONVERT(
                          VARCHAR(20),
                          T.LINEA
                      ) LIKE @like
                      OR CONVERT(
                          VARCHAR(20),
                          T.MES_INICIO
                      ) LIKE @like
                      OR CONVERT(
                          VARCHAR(20),
                          T.MES_CORTE
                      ) LIKE @like
                      OR CONVERT(
                          VARCHAR(40),
                          T.COBERTURA
                      ) LIKE @like
                  )
                ORDER BY
                    CASE
                        WHEN @sortField = 'linea'
                            AND @sortOrder = 1
                        THEN T.LINEA
                    END ASC,
                    CASE
                        WHEN @sortField = 'linea'
                            AND @sortOrder = -1
                        THEN T.LINEA
                    END DESC,
                    CASE
                        WHEN @sortField = 'mes_inicio'
                            AND @sortOrder = 1
                        THEN T.MES_INICIO
                    END ASC,
                    CASE
                        WHEN @sortField = 'mes_inicio'
                            AND @sortOrder = -1
                        THEN T.MES_INICIO
                    END DESC,
                    CASE
                        WHEN @sortField = 'mes_corte'
                            AND @sortOrder = 1
                        THEN T.MES_CORTE
                    END ASC,
                    CASE
                        WHEN @sortField = 'mes_corte'
                            AND @sortOrder = -1
                        THEN T.MES_CORTE
                    END DESC,
                    CASE
                        WHEN @sortField = 'cobertura'
                            AND @sortOrder = 1
                        THEN T.COBERTURA
                    END ASC,
                    CASE
                        WHEN @sortField = 'cobertura'
                            AND @sortOrder = -1
                        THEN T.COBERTURA
                    END DESC,
                    T.MES_INICIO ASC,
                    T.LINEA ASC
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
                        FslTablaCoberturaDto>
                    {
                        total = reader.ReadFirst<int>(),
                        lista = reader
                            .Read<FslTablaCoberturaDto>()
                            .ToList()
                    };
                });
        }

        /// <summary>
        /// Registra una nueva cobertura y calcula la siguiente
        /// línea disponible para el tipo seleccionado.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <param name="request">
        /// Información de la cobertura.
        /// </param>
        /// <returns>
        /// Resultado del registro.
        /// </returns>
        public ErrorDto
            FSL_TablaCoberturas_Cobertura_Registrar(
                int CodEmpresa,
                FslTablaCoberturaGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validacion =
                FSL_TablaCoberturas_Request_Validar(
                    request,
                    false);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var tipo = request.tipo
                .Trim()
                .ToUpperInvariant();

            var usuario = request.usuario.Trim();

            const string sql = """
                SET XACT_ABORT ON;

                BEGIN TRANSACTION;

                DECLARE @linea INT;

                SELECT
                    @linea = ISNULL(MAX(LINEA), 0) + 1
                FROM FSL_TABLAS_APLICACION
                    WITH (UPDLOCK, HOLDLOCK)
                WHERE TIPO = @tipo;

                INSERT INTO FSL_TABLAS_APLICACION
                (
                    TIPO,
                    LINEA,
                    MES_INICIO,
                    MES_CORTE,
                    COBERTURA,
                    REGISTRA_FECHA,
                    REGISTRA_USUARIO
                )
                VALUES
                (
                    @tipo,
                    @linea,
                    @mesInicio,
                    @mesCorte,
                    @cobertura,
                    GETDATE(),
                    @usuario
                );

                COMMIT TRANSACTION;

                SELECT @linea;
                """;

            var response = DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                    connection.QuerySingle<int>(
                        sql,
                        new
                        {
                            tipo,
                            mesInicio =
                                request.mes_inicio,
                            mesCorte =
                                request.mes_corte,
                            request.cobertura,
                            usuario
                        }));

            if (response.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    response.Description ??
                    "Ocurrió un error al registrar la cobertura.");
            }

            var linea = response.Result;

            FSL_TablaCoberturas_Bitacora_Registrar(
                CodEmpresa,
                usuario,
                "Registra",
                tipo,
                linea);

            return DbHelper.OkResponse(
                "Cobertura registrada correctamente.");
        }

        /// <summary>
        /// Actualiza una cobertura existente.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <param name="request">
        /// Información de la cobertura.
        /// </param>
        /// <returns>
        /// Resultado de la actualización.
        /// </returns>
        public ErrorDto
            FSL_TablaCoberturas_Cobertura_Actualizar(
                int CodEmpresa,
                FslTablaCoberturaGuardarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var validacion =
                FSL_TablaCoberturas_Request_Validar(
                    request,
                    true);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.ErrorResponse(
                    validacion,
                    CodigoValidacion);
            }

            var tipo = request.tipo
                .Trim()
                .ToUpperInvariant();

            var usuario = request.usuario.Trim();

            const string sql = """
                UPDATE FSL_TABLAS_APLICACION
                SET MES_INICIO = @mesInicio,
                    MES_CORTE = @mesCorte,
                    COBERTURA = @cobertura
                WHERE TIPO = @tipo
                  AND LINEA = @linea;
                """;

            var operacion =
                new FslTablaCoberturaOperacion
                {
                    Sql = sql,
                    Parametros = new
                    {
                        tipo,
                        linea = request.linea,
                        mesInicio =
                            request.mes_inicio,
                        mesCorte =
                            request.mes_corte,
                        request.cobertura
                    },
                    Usuario = usuario,
                    Movimiento = "Modifica",
                    Tipo = tipo,
                    Linea = request.linea,
                    MensajeExito =
                        "Cobertura actualizada correctamente.",
                    MensajeSinCambios =
                        "No se encontró la cobertura indicada."
                };

            return FSL_TablaCoberturas_Operacion_Ejecutar(
                CodEmpresa,
                operacion);
        }

        /// <summary>
        /// Elimina una cobertura por tipo y línea.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <param name="tipo">
        /// Tipo de tabla.
        /// </param>
        /// <param name="linea">
        /// Línea de la cobertura.
        /// </param>
        /// <param name="usuario">
        /// Usuario responsable.
        /// </param>
        /// <returns>
        /// Resultado de la eliminación.
        /// </returns>
        public ErrorDto
            FSL_TablaCoberturas_Cobertura_Eliminar(
                int CodEmpresa,
                string tipo,
                int linea,
                string usuario)
        {
            var tipoTabla = tipo
                .Trim()
                .ToUpperInvariant();

            var usuarioRegistro = usuario.Trim();

            if (!FSL_TablaCoberturas_Tipo_Valido(
                tipoTabla))
            {
                return DbHelper.ErrorResponse(
                    MensajeTipoInvalido,
                    CodigoValidacion);
            }

            if (linea <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeLineaRequerida,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(
                usuarioRegistro))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            const string sql = """
                DELETE FROM FSL_TABLAS_APLICACION
                WHERE TIPO = @tipoTabla
                  AND LINEA = @linea;
                """;

            var operacion =
                new FslTablaCoberturaOperacion
                {
                    Sql = sql,
                    Parametros = new
                    {
                        tipoTabla,
                        linea
                    },
                    Usuario = usuarioRegistro,
                    Movimiento = "Elimina",
                    Tipo = tipoTabla,
                    Linea = linea,
                    MensajeExito =
                        "Cobertura eliminada correctamente.",
                    MensajeSinCambios =
                        "No se encontró la cobertura indicada."
                };

            return FSL_TablaCoberturas_Operacion_Ejecutar(
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
            FSL_TablaCoberturas_Operacion_Ejecutar(
                int CodEmpresa,
                FslTablaCoberturaOperacion operacion)
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
                    "Ocurrió un error al procesar la cobertura.");
            }

            if (response.Result <= 0)
            {
                return DbHelper.ErrorResponse(
                    operacion.MensajeSinCambios,
                    CodigoValidacion);
            }

            FSL_TablaCoberturas_Bitacora_Registrar(
                CodEmpresa,
                operacion.Usuario,
                operacion.Movimiento,
                operacion.Tipo,
                operacion.Linea);

            return DbHelper.OkResponse(
                operacion.MensajeExito);
        }

        /// <summary>
        /// Valida la información enviada para registrar o
        /// actualizar una cobertura.
        /// </summary>
        /// <param name="request">
        /// Información de la cobertura.
        /// </param>
        /// <param name="requiereLinea">
        /// Indica si la operación requiere una línea
        /// existente.
        /// </param>
        /// <returns>
        /// Mensaje de validación o una cadena vacía.
        /// </returns>
        private static string
            FSL_TablaCoberturas_Request_Validar(
                FslTablaCoberturaGuardarRequest request,
                bool requiereLinea)
        {
            if (!FSL_TablaCoberturas_Tipo_Valido(
                request.tipo))
            {
                return MensajeTipoInvalido;
            }

            if (requiereLinea && request.linea <= 0)
            {
                return MensajeLineaRequerida;
            }

            return string.IsNullOrWhiteSpace(
                request.usuario)
                    ? MensajeUsuarioRequerido
                    : string.Empty;
        }

        /// <summary>
        /// Determina si el tipo de tabla corresponde a uno de
        /// los valores utilizados por el formulario VB6.
        /// </summary>
        /// <param name="tipo">
        /// Tipo de tabla.
        /// </param>
        /// <returns>
        /// Verdadero cuando el tipo es válido.
        /// </returns>
        private static bool
            FSL_TablaCoberturas_Tipo_Valido(
                string tipo)
        {
            var valor = tipo
                .Trim()
                .ToUpperInvariant();

            return valor is "F" or "I" or "S" or "X";
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
            FSL_TablaCoberturas_Orden_Campo_Obtener(
                string sortField)
        {
            return sortField
                .Trim()
                .ToLowerInvariant() switch
            {
                "linea" => "linea",
                "mes_corte" => "mes_corte",
                "cobertura" => "cobertura",
                _ => "mes_inicio"
            };
        }

        /// <summary>
        /// Registra un movimiento del formulario en la
        /// bitácora general.
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
        /// <param name="tipo">
        /// Tipo de tabla.
        /// </param>
        /// <param name="linea">
        /// Línea afectada.
        /// </param>
        private void
            FSL_TablaCoberturas_Bitacora_Registrar(
                int CodEmpresa,
                string usuario,
                string movimiento,
                string tipo,
                int linea)
        {
            var descripcion =
                FSL_TablaCoberturas_Tipo_Descripcion_Obtener(
                    tipo);

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
                        $"{descripcion} Id.:{linea}"
                });
        }

        /// <summary>
        /// Obtiene la descripción utilizada por VB6 para
        /// el tipo de tabla.
        /// </summary>
        /// <param name="tipo">
        /// Tipo de tabla.
        /// </param>
        /// <returns>
        /// Descripción del tipo.
        /// </returns>
        private static string
            FSL_TablaCoberturas_Tipo_Descripcion_Obtener(
                string tipo)
        {
            return tipo
                .Trim()
                .ToUpperInvariant() switch
            {
                "I" => "Tabla de Incapacidad",
                "S" => "Tabla de Suicidios",
                "X" => "Tabla de 100%",
                _ => "Tabla de Fallecimiento"
            };
        }

        private sealed class
            FslTablaCoberturaOperacion
        {
            public string Sql { get; init; } =
                string.Empty;

            public object Parametros { get; init; } =
                new();

            public string Usuario { get; init; } =
                string.Empty;

            public string Movimiento { get; init; } =
                string.Empty;

            public string Tipo { get; init; } =
                string.Empty;

            public int Linea { get; init; } = 0;

            public string MensajeExito { get; init; } =
                string.Empty;

            public string MensajeSinCambios { get; init; } =
                string.Empty;
        }
    }
}