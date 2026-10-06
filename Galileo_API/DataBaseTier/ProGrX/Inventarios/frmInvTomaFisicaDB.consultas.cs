using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public sealed partial class FrmInvTomaFisicaDB
    {
        /// <summary>
        /// Obtiene las tomas físicas registradas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros, paginación y ordenamiento.</param>
        /// <returns>Listado de tomas físicas.</returns>
        public ErrorDto<List<TomaFisicaDto>>
            INV_TomaFisica_Lista_Obtener(
                int CodEmpresa,
                TomaFisicaListaFiltros? filtros)
        {
            int pagina = Math.Max(0, filtros?.pagina ?? 0);
            int paginacion =
                INV_TomaFisica_Paginacion_Obtener(
                    filtros?.paginacion ?? 0);

            string filtro =
                filtros?.filtro?.Trim() ?? string.Empty;

            if (filtro.Length > LongitudFiltroMaxima)
            {
                return DbHelper.CreateErrorResponse(
                    $"El filtro no puede superar los {LongitudFiltroMaxima} caracteres.",
                    CodigoValidacion,
                    new List<TomaFisicaDto>());
            }

            string? filtroLike = string.IsNullOrEmpty(filtro)
                ? null
                : $"%{INV_TomaFisica_Filtro_Escapar(filtro)}%";

            const string query = """
                SELECT
                    I.consecutivo,
                    RTRIM(ISNULL(I.cod_bodega, '')) AS cod_bodega,
                    RTRIM(ISNULL(B.descripcion, '')) AS bodega,
                    RTRIM(ISNULL(I.notas, '')) AS notas,
                    RTRIM(ISNULL(I.estado, '')) AS estado,
                    I.fecha_crea,
                    RTRIM(ISNULL(I.user_crea, '')) AS user_crea,
                    I.fecha_inicio,
                    I.fecha_corte,
                    I.fecha_aplica,
                    RTRIM(ISNULL(I.user_aplica, '')) AS user_aplica,
                    RTRIM(ISNULL(I.tipo_asiento, '')) AS tipo_asiento,
                    RTRIM(ISNULL(I.num_asiento, '')) AS num_asiento,
                    I.fecha_asiento,
                    RTRIM(ISNULL(I.causa_entrada, '')) AS causa_entrada,
                    RTRIM(ISNULL(I.causa_salida, '')) AS causa_salida,
                    ISNULL(I.cod_proveedor_entrada, 0)
                        AS cod_proveedor_entrada,
                    ISNULL(I.cod_entradag, 0) AS cod_entradag,
                    ISNULL(I.cod_salidag, 0) AS cod_salidag
                FROM pv_InvTomaFisica I
                INNER JOIN pv_Bodegas B
                    ON I.cod_bodega = B.cod_bodega
                WHERE
                    @Filtro IS NULL
                    OR CONVERT(varchar(20), I.consecutivo)
                        LIKE @Filtro ESCAPE '\'
                    OR I.cod_bodega LIKE @Filtro ESCAPE '\'
                    OR B.descripcion LIKE @Filtro ESCAPE '\'
                    OR I.user_crea LIKE @Filtro ESCAPE '\'
                    OR I.notas LIKE @Filtro ESCAPE '\'
                ORDER BY
                    CASE
                        WHEN @SortField = 'consecutivo'
                         AND @SortOrder = 1
                        THEN I.consecutivo
                    END ASC,
                    CASE
                        WHEN @SortField = 'consecutivo'
                         AND @SortOrder = -1
                        THEN I.consecutivo
                    END DESC,
                    CASE
                        WHEN @SortField = 'cod_bodega'
                         AND @SortOrder = 1
                        THEN I.cod_bodega
                    END ASC,
                    CASE
                        WHEN @SortField = 'cod_bodega'
                         AND @SortOrder = -1
                        THEN I.cod_bodega
                    END DESC,
                    CASE
                        WHEN @SortField = 'fecha_inicio'
                         AND @SortOrder = 1
                        THEN I.fecha_inicio
                    END ASC,
                    CASE
                        WHEN @SortField = 'fecha_inicio'
                         AND @SortOrder = -1
                        THEN I.fecha_inicio
                    END DESC,
                    CASE
                        WHEN @SortField = 'fecha_corte'
                         AND @SortOrder = 1
                        THEN I.fecha_corte
                    END ASC,
                    CASE
                        WHEN @SortField = 'fecha_corte'
                         AND @SortOrder = -1
                        THEN I.fecha_corte
                    END DESC,
                    I.consecutivo DESC
                OFFSET @Pagina ROWS
                FETCH NEXT @Paginacion ROWS ONLY;
                """;

            var result =
                DbHelper.ExecuteListQuery<TomaFisicaDto>(
                    _portalDb,
                    CodEmpresa,
                    query,
                    new
                    {
                        Filtro = filtroLike,
                        Pagina = pagina,
                        Paginacion = paginacion,
                        SortField =
                            INV_TomaFisica_Orden_Campo_Obtener(
                                filtros?.sortField),
                        SortOrder =
                            INV_TomaFisica_Orden_Direccion_Obtener(
                                filtros?.sortOrder ?? -1)
                    });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? [])
                : DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarLista,
                    result.Code.GetValueOrDefault(-1),
                    new List<TomaFisicaDto>());
        }

        /// <summary>
        /// Obtiene el detalle de una toma física.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="filtros">Filtros aplicados al detalle.</param>
        /// <returns>Detalle de la toma física.</returns>
        public ErrorDto<List<TomaFisicaDetalleDto>>
            INV_TomaFisica_Detalle_Obtener(
                int CodEmpresa,
                TomaFisicaDetalleFiltros? filtros)
        {
            int consecutivo = filtros?.consecutivo ?? 0;

            if (consecutivo <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeConsecutivoRequerido,
                    CodigoValidacion,
                    new List<TomaFisicaDetalleDto>());
            }

            string filtro = (filtros?.filtro ?? string.Empty).Trim();

            if (filtro.Length > LongitudFiltroMaxima)
            {
                return DbHelper.CreateErrorResponse(
                    $"El filtro no puede superar los {LongitudFiltroMaxima} caracteres.",
                    CodigoValidacion,
                    new List<TomaFisicaDetalleDto>());
            }

            string? filtroLike = string.IsNullOrEmpty(filtro)
                ? null
                : $"%{INV_TomaFisica_Filtro_Escapar(filtro)}%";

            const string query = """
                SELECT
                    D.consecutivo,
                    RTRIM(ISNULL(D.cod_bodega, '')) AS cod_bodega,
                    RTRIM(ISNULL(B.descripcion, '')) AS bodega,
                    RTRIM(ISNULL(D.cod_producto, '')) AS cod_producto,
                    RTRIM(ISNULL(P.descripcion, '')) AS descripcion,
                    RTRIM(ISNULL(P.tipo_producto, '')) AS tipo,
                    RTRIM(ISNULL(D.ubicacion, '')) AS ubicacion,
                    CAST(
                        ISNULL(D.existencia_logica, 0)
                        AS decimal(18, 4)
                    ) AS existencia_logica,
                    CAST(
                        ISNULL(D.existencia_fisica, 0)
                        AS decimal(18, 4)
                    ) AS existencia_fisica,
                    CAST(
                        ISNULL(D.existencia_logica, 0) -
                        ISNULL(D.existencia_fisica, 0)
                        AS decimal(18, 4)
                    ) AS diferencia
                FROM pv_invTF_Detalle D
                INNER JOIN pv_productos P
                    ON D.cod_producto = P.cod_producto
                INNER JOIN pv_bodegas B
                    ON D.cod_bodega = B.cod_bodega
                WHERE D.consecutivo = @Consecutivo
                  AND
                  (
                      @Filtro IS NULL
                      OR D.cod_producto LIKE @Filtro ESCAPE '\'
                      OR P.descripcion LIKE @Filtro ESCAPE '\'
                      OR D.ubicacion LIKE @Filtro ESCAPE '\'
                  )
                ORDER BY D.cod_producto, D.ubicacion;
                """;

            var result =
                DbHelper.ExecuteListQuery<
                    TomaFisicaDetalleDto>(
                        _portalDb,
                        CodEmpresa,
                        query,
                        new
                        {
                            Consecutivo = consecutivo,
                            Filtro = filtroLike
                        });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? [])
                : DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarDetalle,
                    result.Code.GetValueOrDefault(-1),
                    new List<TomaFisicaDetalleDto>());
        }

        /// <summary>
        /// Obtiene una toma física por consecutivo.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="consecutivo">Consecutivo de la toma.</param>
        /// <returns>Toma física encontrada.</returns>
        public ErrorDto<TomaFisicaDto>
            INV_TomaFisica_Consecutivo_Obtener(
                int CodEmpresa,
                int consecutivo)
        {
            if (consecutivo <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeConsecutivoRequerido,
                    CodigoValidacion,
                    new TomaFisicaDto());
            }

            const string query = """
                SELECT
                    I.consecutivo,
                    RTRIM(ISNULL(I.cod_bodega, '')) AS cod_bodega,
                    RTRIM(ISNULL(B.descripcion, '')) AS bodega,
                    RTRIM(ISNULL(I.notas, '')) AS notas,
                    RTRIM(ISNULL(I.estado, '')) AS estado,
                    I.fecha_crea,
                    RTRIM(ISNULL(I.user_crea, '')) AS user_crea,
                    I.fecha_inicio,
                    I.fecha_corte,
                    I.fecha_aplica,
                    RTRIM(ISNULL(I.user_aplica, '')) AS user_aplica,
                    RTRIM(ISNULL(I.tipo_asiento, '')) AS tipo_asiento,
                    RTRIM(ISNULL(I.num_asiento, '')) AS num_asiento,
                    I.fecha_asiento,
                    RTRIM(ISNULL(I.causa_entrada, '')) AS causa_entrada,
                    RTRIM(ISNULL(I.causa_salida, '')) AS causa_salida,
                    ISNULL(I.cod_proveedor_entrada, 0)
                        AS cod_proveedor_entrada,
                    ISNULL(I.cod_entradag, 0) AS cod_entradag,
                    ISNULL(I.cod_salidag, 0) AS cod_salidag
                FROM pv_InvTomaFisica I
                INNER JOIN pv_Bodegas B
                    ON I.cod_bodega = B.cod_bodega
                WHERE I.consecutivo = @Consecutivo;
                """;

            var result =
                DbHelper.ExecuteSingleQuery<TomaFisicaDto>(
                    _portalDb,
                    CodEmpresa,
                    query,
                    null,
                    new { Consecutivo = consecutivo });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarToma,
                    result.Code.GetValueOrDefault(-1),
                    new TomaFisicaDto());
            }

            return result.Result is null
                ? DbHelper.CreateErrorResponse(
                    MensajeTomaNoExiste,
                    CodigoValidacion,
                    new TomaFisicaDto())
                : DbHelper.CreateOkResponse(result.Result);
        }

        /// <summary>
        /// Obtiene el consecutivo anterior o siguiente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="consecutivo">Consecutivo actual.</param>
        /// <param name="tipo">Dirección de navegación.</param>
        /// <returns>Consecutivo encontrado.</returns>
        public ErrorDto<TomaFisicaDto>
            INV_TomaFisica_Consecutivo_Navegar(
                int CodEmpresa,
                int consecutivo,
                string? tipo)
        {
            string direccion =
                tipo?.Trim().ToLowerInvariant() ?? string.Empty;

            string query;

            if (direccion == DireccionDescendente &&
                consecutivo <= 0)
            {
                query = """
                    SELECT TOP 1 consecutivo
                    FROM pv_InvTomaFisica
                    ORDER BY consecutivo DESC;
                    """;
            }
            else if (direccion == DireccionDescendente)
            {
                query = """
                    SELECT TOP 1 consecutivo
                    FROM pv_InvTomaFisica
                    WHERE consecutivo < @Consecutivo
                    ORDER BY consecutivo DESC;
                    """;
            }
            else if (direccion == DireccionAscendente)
            {
                query = """
                    SELECT TOP 1 consecutivo
                    FROM pv_InvTomaFisica
                    WHERE consecutivo > @Consecutivo
                    ORDER BY consecutivo ASC;
                    """;
            }
            else
            {
                return DbHelper.CreateErrorResponse(
                    "La direcci&oacute;n de navegaci&oacute;n no es v&aacute;lida.",
                    CodigoValidacion,
                    new TomaFisicaDto());
            }

            var result =
                DbHelper.ExecuteSingleQuery<TomaFisicaDto>(
                    _portalDb,
                    CodEmpresa,
                    query,
                    null,
                    new { Consecutivo = consecutivo });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarToma,
                    result.Code.GetValueOrDefault(-1),
                    new TomaFisicaDto());
            }

            return result.Result is null
                ? DbHelper.CreateErrorResponse(
                    "No existen m&aacute;s tomas f&iacute;sicas.",
                    CodigoValidacion,
                    new TomaFisicaDto())
                : DbHelper.CreateOkResponse(result.Result);
        }

        /// <summary>
        /// Obtiene un producto por código o código de barras.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Criterios de búsqueda.</param>
        /// <returns>Producto encontrado.</returns>
        public ErrorDto<TomaFisicaDetalleDto>
            INV_TomaFisica_Producto_Obtener(
                int CodEmpresa,
                TomaFisicaProductoRequest? request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "La informaci&oacute;n de b&uacute;squeda es requerida.",
                    CodigoValidacion,
                    new TomaFisicaDetalleDto());
            }

            string validacion =
                INV_TomaFisica_Producto_Validar(request);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    CodigoValidacion,
                    new TomaFisicaDetalleDto());
            }

            string tipo =
                request.tipo.Trim().ToUpperInvariant();

            const string query = """
                SELECT TOP 1
                    RTRIM(ISNULL(P.cod_producto, ''))
                        AS cod_producto,
                    RTRIM(ISNULL(P.descripcion, ''))
                        AS descripcion,
                    RTRIM(ISNULL(P.tipo_producto, ''))
                        AS tipo,
                    CAST(0 AS decimal(18, 4))
                        AS existencia_logica,
                    CAST(0 AS decimal(18, 4))
                        AS existencia_fisica,
                    CAST(0 AS decimal(18, 4))
                        AS diferencia,
                    '' AS ubicacion
                FROM pv_productos P
                WHERE
                    (
                        @Tipo = 'CB'
                        AND P.cod_barras = @Codigo
                    )
                    OR
                    (
                        @Tipo = 'CP'
                        AND P.cod_producto = @Codigo
                    );
                """;

            var result =
                DbHelper.ExecuteSingleQuery<
                    TomaFisicaDetalleDto>(
                        _portalDb,
                        CodEmpresa,
                        query,
                        null,
                        new
                        {
                            Tipo = tipo,
                            Codigo = request.codigo.Trim()
                        });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? ErrorConsultarProducto,
                    result.Code.GetValueOrDefault(-1),
                    new TomaFisicaDetalleDto());
            }

            if (result.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontr&oacute; el producto indicado.",
                    CodigoValidacion,
                    new TomaFisicaDetalleDto());
            }

            result.Result.cod_bodega =
                request.cod_bodega.Trim();

            return DbHelper.CreateOkResponse(result.Result);
        }
    }
}
