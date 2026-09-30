using System.Data;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public sealed partial class FrmFslRemesasPagoDB
    {
        /// <summary>
        /// Obtiene las remesas abiertas disponibles para carga.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Remesas abiertas.</returns>
        public ErrorDto<List<FslRemesaDto>>
            FSL_RemesasPago_Cargas_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    R.TESORERIA_REMESA AS tesoreria_remesa,
                    LTRIM(RTRIM(ISNULL(R.REGISTRO_USUARIO, '')))
                        AS registro_usuario,
                    R.REGISTRO_FECHA AS registro_fecha,
                    R.FECHA_INICIO AS fecha_inicio,
                    R.FECHA_CORTE AS fecha_corte,
                    LTRIM(RTRIM(ISNULL(R.NOTAS, ''))) AS notas,
                    LTRIM(RTRIM(ISNULL(R.ESTADO, ''))) AS estado,
                    'Remesa Abierta' AS estado_descripcion,
                    CONCAT(
                        RIGHT(
                            '0000' +
                            CONVERT(VARCHAR(20), R.TESORERIA_REMESA),
                            4
                        ),
                        '...',
                        LTRIM(RTRIM(ISNULL(R.REGISTRO_USUARIO, ''))),
                        '...',
                        CONVERT(VARCHAR(19), R.REGISTRO_FECHA, 120),
                        ' I:',
                        CONVERT(VARCHAR(10), R.FECHA_INICIO, 103),
                        ' C:',
                        CONVERT(VARCHAR(10), R.FECHA_CORTE, 103)
                    ) AS descripcion
                FROM FSL_REMESAS_TESORERIA R
                WHERE R.ESTADO = 'A'
                ORDER BY R.REGISTRO_FECHA DESC;
                """;

            return DbHelper.ExecuteListQuery<FslRemesaDto>(
                _portalDb,
                CodEmpresa,
                sql);
        }

        /// <summary>
        /// Obtiene los expedientes elegibles para una remesa abierta.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros de expedientes.</param>
        /// <returns>Expedientes elegibles.</returns>
        public ErrorDto<
            FslListaPaginadaDto<FslExpedienteRemesaDto>>
            FSL_RemesasPago_CargasLista_Obtener(
                int CodEmpresa,
                FslExpedientesFiltros filtros)
        {
            ArgumentNullException.ThrowIfNull(filtros);

            if (filtros.cod_remesa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeRemesaRequerida,
                    CodigoValidacion,
                    new FslListaPaginadaDto<
                        FslExpedienteRemesaDto>());
            }

            var filtro = filtros.filtro.Trim();
            var like = string.IsNullOrWhiteSpace(filtro)
                ? null
                : $"%{filtro}%";

            var offset = Math.Max(filtros.pagina, 0);
            var fetch = Math.Clamp(
                filtros.paginacion,
                1,
                500);

            var sortField =
                filtros.sort_field.Trim().ToLowerInvariant()
                switch
                {
                    "cod_expediente" => "cod_expediente",
                    "nombre" => "nombre",
                    "total_sobrante" => "total_sobrante",
                    _ => "cedula"
                };

            var sortOrder =
                filtros.sort_order == -1 ? -1 : 1;

            const string sql = """
                SELECT COUNT(*)
                FROM FSL_EXPEDIENTES E
                INNER JOIN SOCIOS S
                    ON E.CEDULA = S.CEDULA
                INNER JOIN FSL_REMESAS_TESORERIA R
                    ON R.TESORERIA_REMESA = @cod_remesa
                WHERE R.ESTADO = 'A'
                  AND E.RESOLUCION_FECHA >= R.FECHA_INICIO
                  AND E.RESOLUCION_FECHA <
                      DATEADD(DAY, 1, R.FECHA_CORTE)
                  AND E.TESORERIA_REMESA IS NULL
                  AND E.TIPO_DESEMBOLSO = 'T'
                  AND E.ESTADO = 'X'
                  AND E.TOTAL_SOBRANTE > 0
                  AND (
                      @like IS NULL
                      OR CONVERT(VARCHAR(30), E.COD_EXPEDIENTE) LIKE @like
                      OR E.CEDULA LIKE @like
                      OR S.NOMBRE LIKE @like
                      OR E.PRESENTA_CEDULA LIKE @like
                      OR E.PRESENTA_NOMBRE LIKE @like
                  );

                SELECT
                    CONVERT(VARCHAR(30), E.COD_EXPEDIENTE)
                        AS cod_expediente,
                    LTRIM(RTRIM(ISNULL(E.CEDULA, ''))) AS cedula,
                    LTRIM(RTRIM(ISNULL(S.NOMBRE, ''))) AS nombre,
                    CAST(ISNULL(E.TOTAL_SOBRANTE, 0) AS DECIMAL(18, 2))
                        AS total_sobrante,
                    LTRIM(RTRIM(ISNULL(E.PRESENTA_CEDULA, '')))
                        AS presenta_cedula,
                    LTRIM(RTRIM(ISNULL(E.PRESENTA_NOMBRE, '')))
                        AS presenta_nombre
                FROM FSL_EXPEDIENTES E
                INNER JOIN SOCIOS S
                    ON E.CEDULA = S.CEDULA
                INNER JOIN FSL_REMESAS_TESORERIA R
                    ON R.TESORERIA_REMESA = @cod_remesa
                WHERE R.ESTADO = 'A'
                  AND E.RESOLUCION_FECHA >= R.FECHA_INICIO
                  AND E.RESOLUCION_FECHA <
                      DATEADD(DAY, 1, R.FECHA_CORTE)
                  AND E.TESORERIA_REMESA IS NULL
                  AND E.TIPO_DESEMBOLSO = 'T'
                  AND E.ESTADO = 'X'
                  AND E.TOTAL_SOBRANTE > 0
                  AND (
                      @like IS NULL
                      OR CONVERT(VARCHAR(30), E.COD_EXPEDIENTE) LIKE @like
                      OR E.CEDULA LIKE @like
                      OR S.NOMBRE LIKE @like
                      OR E.PRESENTA_CEDULA LIKE @like
                      OR E.PRESENTA_NOMBRE LIKE @like
                  )
                ORDER BY
                    CASE
                        WHEN @sortField = 'cod_expediente'
                         AND @sortOrder = 1
                        THEN CONVERT(VARCHAR(30), E.COD_EXPEDIENTE)
                    END ASC,
                    CASE
                        WHEN @sortField = 'cod_expediente'
                         AND @sortOrder = -1
                        THEN CONVERT(VARCHAR(30), E.COD_EXPEDIENTE)
                    END DESC,
                    CASE
                        WHEN @sortField = 'cedula'
                         AND @sortOrder = 1
                        THEN E.CEDULA
                    END ASC,
                    CASE
                        WHEN @sortField = 'cedula'
                         AND @sortOrder = -1
                        THEN E.CEDULA
                    END DESC,
                    CASE
                        WHEN @sortField = 'nombre'
                         AND @sortOrder = 1
                        THEN S.NOMBRE
                    END ASC,
                    CASE
                        WHEN @sortField = 'nombre'
                         AND @sortOrder = -1
                        THEN S.NOMBRE
                    END DESC,
                    CASE
                        WHEN @sortField = 'total_sobrante'
                         AND @sortOrder = 1
                        THEN E.TOTAL_SOBRANTE
                    END ASC,
                    CASE
                        WHEN @sortField = 'total_sobrante'
                         AND @sortOrder = -1
                        THEN E.TOTAL_SOBRANTE
                    END DESC,
                    E.CEDULA ASC,
                    S.NOMBRE ASC
                OFFSET @offset ROWS
                FETCH NEXT @fetch ROWS ONLY;
                """;

            return DbHelper.WithConn(
                _portalDb,
                CodEmpresa,
                connection =>
                {
                    using var result =
                        connection.QueryMultiple(
                            sql,
                            new
                            {
                                filtros.cod_remesa,
                                like,
                                sortField,
                                sortOrder,
                                offset,
                                fetch
                            });

                    return new FslListaPaginadaDto<
                        FslExpedienteRemesaDto>
                    {
                        total =
                            result.ReadFirstOrDefault<int>(),
                        lista =
                            result
                                .Read<FslExpedienteRemesaDto>()
                                .ToList()
                    };
                });
        }

        /// <summary>
        /// Asocia los expedientes seleccionados con una remesa abierta.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Remesa, usuario y expedientes.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto FSL_RemesasPago_Cargas_Aplicar(
            int CodEmpresa,
            FslRemesaAplicarRequest request)
        {
            var validacion =
                FSL_RemesasPago_Aplicar_Request_Validar(
                    request);

            if (validacion != null)
            {
                return validacion;
            }

            var codigos = request.casos
                .Select(item =>
                    item.cod_expediente.Trim())
                .Where(codigo =>
                    !string.IsNullOrWhiteSpace(codigo))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (codigos.Count == 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe seleccionar al menos un expediente.",
                    CodigoValidacion);
            }

            using var connection =
                DbHelper.OpenConnection(
                    _portalDb,
                    CodEmpresa);

            try
            {
                connection.Open();

                using var transaction =
                    connection.BeginTransaction();

                const string sqlEstado = """
                    SELECT COUNT(*)
                    FROM FSL_REMESAS_TESORERIA
                    WHERE TESORERIA_REMESA = @cod_remesa
                      AND ESTADO = 'A';
                    """;

                var abierta =
                    connection.QuerySingle<int>(
                        sqlEstado,
                        new
                        {
                            request.cod_remesa
                        },
                        transaction);

                if (abierta == 0)
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        "La remesa indicada ya se encuentra cerrada.",
                        CodigoValidacion);
                }

                const string sqlActualizar = """
                    UPDATE E
                    SET E.TESORERIA_REMESA = @cod_remesa
                    FROM FSL_EXPEDIENTES E
                    INNER JOIN FSL_REMESAS_TESORERIA R
                        ON R.TESORERIA_REMESA = @cod_remesa
                    WHERE E.COD_EXPEDIENTE IN @codigos
                      AND R.ESTADO = 'A'
                      AND E.RESOLUCION_FECHA >= R.FECHA_INICIO
                      AND E.RESOLUCION_FECHA <
                          DATEADD(DAY, 1, R.FECHA_CORTE)
                      AND E.TESORERIA_REMESA IS NULL
                      AND E.TIPO_DESEMBOLSO = 'T'
                      AND E.ESTADO = 'X'
                      AND E.TOTAL_SOBRANTE > 0;
                    """;

                var afectados = connection.Execute(
                    sqlActualizar,
                    new
                    {
                        request.cod_remesa,
                        codigos
                    },
                    transaction);

                if (afectados != codigos.Count)
                {
                    transaction.Rollback();

                    return DbHelper.ErrorResponse(
                        "Uno o m&aacute;s expedientes ya no se encuentran disponibles para cargar.",
                        CodigoValidacion);
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(
                    ex.Message);
            }

            FSL_RemesasPago_Bitacora_Registrar(
                CodEmpresa,
                request.usuario,
                "Aplica",
                $"Carga Remesa Traslado a Tesoreria : {request.cod_remesa}");

            return DbHelper.OkResponse(
                "Proceso realizado satisfactoriamente.");
        }

        /// <summary>
        /// Valida una solicitud de aplicación de expedientes.
        /// </summary>
        /// <param name="request">Información recibida.</param>
        /// <returns>Error de validación o null.</returns>
        private static ErrorDto?
            FSL_RemesasPago_Aplicar_Request_Validar(
                FslRemesaAplicarRequest request)
        {
            if (request.cod_remesa <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeRemesaRequerida,
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    CodigoValidacion);
            }

            if (request.casos.Count == 0)
            {
                return DbHelper.ErrorResponse(
                    "Debe seleccionar al menos un expediente.",
                    CodigoValidacion);
            }

            return null;
        }
    }
}