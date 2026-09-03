using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Globalization;

namespace Galileo.DataBaseTier
{
    public class FrmInvKardexDb
    {
        private const string Todos = "Todos";
        private const int PaginacionMaxima = 1000;

        private const string ErrorEmpresaRequerida =
            "El c&oacute;digo de la empresa es requerido.";

        private const string ErrorFiltroRequerido =
            "Los filtros del kardex son requeridos.";

        private const string ErrorFechaInicioRequerida =
            "La fecha de inicio es requerida.";

        private const string ErrorFechaCorteRequerida =
            "La fecha de corte es requerida.";

        private const string ErrorFormatoFechas =
            "Las fechas deben utilizar el formato yyyy-MM-dd.";

        private const string ErrorRangoFechas =
            "La fecha de inicio no puede ser mayor que la fecha de corte.";

        private const string ErrorPaginaInvalida =
            "La posici&oacute;n inicial de la p&aacute;gina no es v&aacute;lida.";

        private const string ErrorPaginacionInvalida =
            "La cantidad de registros por p&aacute;gina no es v&aacute;lida.";

        private const string ErrorConsultarBodegas =
            "Ocurri&oacute; un error al consultar las bodegas.";

        private const string ErrorConsultarMovimientos =
            "Ocurri&oacute; un error al consultar los movimientos del kardex.";

        private const string QueryBodegas = """
            SELECT
                B.cod_bodega,
                B.descripcion
            FROM pv_bodegas B
            ORDER BY B.cod_bodega
            """;

        private const string QueryTotal = """
            SELECT COUNT(1)
            FROM pv_inventario_mov M
            INNER JOIN pv_productos P
                ON M.cod_producto = P.cod_producto
            INNER JOIN pv_bodegas B
                ON M.cod_bodega = B.cod_bodega
            WHERE M.fecha >= @FechaInicio
              AND M.fecha < @FechaCorteExclusiva
              AND (
                  @Tipo = @Todos
                  OR (
                      @Tipo IN ('E', 'S')
                      AND M.tipo = @Tipo
                  )
                  OR (
                      @Tipo NOT IN ('E', 'S')
                      AND M.origen = @Tipo
                  )
              )
              AND (
                  @CodBodega = @Todos
                  OR M.cod_bodega = @CodBodega
              )
              AND (
                  @CodProducto = @Todos
                  OR M.cod_producto = @CodProducto
              )
              AND (
                  @Filtro = ''
                  OR M.cod_producto LIKE @FiltroBusqueda
                  OR P.descripcion LIKE @FiltroBusqueda
                  OR M.codigo LIKE @FiltroBusqueda
                  OR M.origen LIKE @FiltroBusqueda
                  OR B.descripcion LIKE @FiltroBusqueda
                  OR CONVERT(
                      varchar(30),
                      M.fecha,
                      120
                  ) LIKE @FiltroBusqueda
              )
            """;

        private const string QueryMovimientos = """
            SELECT
                M.fecha,
                RTRIM(M.cod_producto) + ' - ' +
                    RTRIM(P.descripcion) AS producto,
                CASE M.tipo
                    WHEN 'E' THEN 'ENTRADA'
                    WHEN 'S' THEN 'SALIDA'
                    ELSE ''
                END AS tipox,
                M.origen,
                M.codigo,
                ISNULL(M.existencia, 0) AS existencia,
                M.cantidad,
                CASE
                    WHEN M.tipo = 'E'
                        THEN ISNULL(M.existencia, 0) + M.cantidad
                    WHEN M.tipo = 'S'
                        THEN ISNULL(M.existencia, 0) - M.cantidad
                    ELSE ISNULL(M.existencia, 0)
                END AS existenciax,
                M.precio,
                M.cantidad * M.precio AS totalsinimp,
                (M.cantidad * M.precio) *
                    (M.imp_ventas / 100.0) AS impventas,
                (M.cantidad * M.precio) *
                    (M.imp_consumo / 100.0) AS impconsumo,
                (M.cantidad * M.precio) +
                    (
                        (M.cantidad * M.precio) *
                        (M.imp_ventas / 100.0)
                    ) +
                    (
                        (M.cantidad * M.precio) *
                        (M.imp_consumo / 100.0)
                    ) AS totalconimp,
                RTRIM(M.cod_bodega) + ' - ' +
                    RTRIM(B.descripcion) AS bodega,
                ISNULL(
                    dbo.fxINVBodegaTraslado(
                        M.origen,
                        M.tipo,
                        M.linea
                    ),
                    ''
                ) AS bodegaenlace
            FROM pv_inventario_mov M
            INNER JOIN pv_productos P
                ON M.cod_producto = P.cod_producto
            INNER JOIN pv_bodegas B
                ON M.cod_bodega = B.cod_bodega
            WHERE M.fecha >= @FechaInicio
              AND M.fecha < @FechaCorteExclusiva
              AND (
                  @Tipo = @Todos
                  OR (
                      @Tipo IN ('E', 'S')
                      AND M.tipo = @Tipo
                  )
                  OR (
                      @Tipo NOT IN ('E', 'S')
                      AND M.origen = @Tipo
                  )
              )
              AND (
                  @CodBodega = @Todos
                  OR M.cod_bodega = @CodBodega
              )
              AND (
                  @CodProducto = @Todos
                  OR M.cod_producto = @CodProducto
              )
              AND (
                  @Filtro = ''
                  OR M.cod_producto LIKE @FiltroBusqueda
                  OR P.descripcion LIKE @FiltroBusqueda
                  OR M.codigo LIKE @FiltroBusqueda
                  OR M.origen LIKE @FiltroBusqueda
                  OR B.descripcion LIKE @FiltroBusqueda
                  OR CONVERT(
                      varchar(30),
                      M.fecha,
                      120
                  ) LIKE @FiltroBusqueda
              )
            ORDER BY
                M.fecha DESC,
                M.linea DESC
            OFFSET @Offset ROWS
            FETCH NEXT @Fetch ROWS ONLY
            """;

        private readonly IConfiguration _config;

        public FrmInvKardexDb(
            IConfiguration config)
        {
            _config = config ??
                throw new ArgumentNullException(
                    nameof(config));
        }

        /// <summary>
        /// Obtiene las bodegas disponibles para consultar el kardex.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de bodegas disponibles.</returns>
        public ErrorDto<List<InvKardexBodegaDto>>
            INV_Kardex_Bodegas_Obtener(
                int CodEmpresa)
        {
            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    ErrorEmpresaRequerida,
                    -2,
                    new List<InvKardexBodegaDto>());
            }

            var resultado =
                DbHelper.ExecuteListQuery<
                    InvKardexBodegaDto>(
                        CrearPortalDb(),
                        CodEmpresa,
                        QueryBodegas);

            return resultado.Code == 0
                ? resultado
                : DbHelper.CreateErrorResponse(
                    ErrorConsultarBodegas,
                    resultado.Code.GetValueOrDefault(-1),
                    new List<InvKardexBodegaDto>());
        }

        /// <summary>
        /// Obtiene los movimientos del kardex según los filtros indicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtros">Filtros de consulta y paginación.</param>
        /// <returns>Listado paginado de movimientos del kardex.</returns>
        public ErrorDto<InvKardexMovimientosListaDto>
            INV_Kardex_Movimientos_Obtener(
                int CodEmpresa,
                InvKardexMovimientosFiltro filtros)
        {
            string validacion =
                INV_Kardex_Filtros_Validar(
                    CodEmpresa,
                    filtros);

            if (!string.IsNullOrEmpty(validacion))
            {
                return DbHelper.CreateErrorResponse(
                    validacion,
                    -2,
                    CrearResultadoVacio());
            }

            DateTime fechaInicio =
                DateTime.ParseExact(
                    filtros.fecha_inicio,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);

            DateTime fechaCorte =
                DateTime.ParseExact(
                    filtros.fecha_corte,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture);

            NormalizarFiltros(filtros);

            DynamicParameters parametros =
                CrearParametros(
                    filtros,
                    fechaInicio,
                    fechaCorte);

            var resultado = DbHelper.WithConn(
                CrearPortalDb(),
                CodEmpresa,
                connection =>
                {
                    var respuesta =
                        CrearResultadoVacio();

                    respuesta.total =
                        connection.QueryFirstOrDefault<int>(
                            QueryTotal,
                            parametros);

                    respuesta.movimientos =
                        connection.Query<
                                InvKardexMovimientoDto>(
                                QueryMovimientos,
                                parametros)
                            .ToList();

                    return respuesta;
                });

            return resultado.Code == 0
                ? DbHelper.CreateOkResponse(
                    resultado.Result ??
                    CrearResultadoVacio())
                : DbHelper.CreateErrorResponse(
                    ErrorConsultarMovimientos,
                    resultado.Code.GetValueOrDefault(-1),
                    CrearResultadoVacio());
        }

        /// <summary>
        /// Valida la empresa y los filtros recibidos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtros">Filtros de consulta.</param>
        /// <returns>Mensaje de validación o una cadena vacía.</returns>
        private static string
            INV_Kardex_Filtros_Validar(
                int CodEmpresa,
                InvKardexMovimientosFiltro? filtros)
        {
            if (CodEmpresa <= 0)
            {
                return ErrorEmpresaRequerida;
            }

            if (filtros is null)
            {
                return ErrorFiltroRequerido;
            }

            if (string.IsNullOrWhiteSpace(
                filtros.fecha_inicio))
            {
                return ErrorFechaInicioRequerida;
            }

            if (string.IsNullOrWhiteSpace(
                filtros.fecha_corte))
            {
                return ErrorFechaCorteRequerida;
            }

            if (!TryParseFecha(
                    filtros.fecha_inicio,
                    out DateTime fechaInicio) ||
                !TryParseFecha(
                    filtros.fecha_corte,
                    out DateTime fechaCorte))
            {
                return ErrorFormatoFechas;
            }

            if (fechaInicio > fechaCorte)
            {
                return ErrorRangoFechas;
            }

            if (filtros.pagina < 0)
            {
                return ErrorPaginaInvalida;
            }

            if (
                filtros.paginacion <= 0 ||
                filtros.paginacion >
                    PaginacionMaxima)
            {
                return ErrorPaginacionInvalida;
            }

            return string.Empty;
        }

        /// <summary>
        /// Convierte una fecha con el formato requerido.
        /// </summary>
        /// <param name="valor">Fecha recibida.</param>
        /// <param name="fecha">Fecha convertida.</param>
        /// <returns>True cuando la fecha es válida.</returns>
        private static bool TryParseFecha(
            string valor,
            out DateTime fecha)
        {
            return DateTime.TryParseExact(
                valor,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out fecha);
        }

        /// <summary>
        /// Normaliza los filtros opcionales.
        /// </summary>
        /// <param name="filtros">Filtros que se normalizarán.</param>
        private static void NormalizarFiltros(
            InvKardexMovimientosFiltro filtros)
        {
            filtros.tipo =
                NormalizarSeleccion(
                    filtros.tipo);

            filtros.cod_bodega =
                NormalizarSeleccion(
                    filtros.cod_bodega);

            filtros.cod_producto =
                NormalizarSeleccion(
                    filtros.cod_producto);

            filtros.vfiltro =
                filtros.vfiltro?.Trim() ??
                string.Empty;
        }

        /// <summary>
        /// Normaliza un valor de selección.
        /// </summary>
        /// <param name="valor">Valor recibido.</param>
        /// <returns>Valor normalizado o Todos.</returns>
        private static string NormalizarSeleccion(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? Todos
                : valor.Trim();
        }

        /// <summary>
        /// Crea los parámetros utilizados por las consultas del kardex.
        /// </summary>
        /// <param name="filtros">Filtros normalizados.</param>
        /// <param name="fechaInicio">Fecha inicial.</param>
        /// <param name="fechaCorte">Fecha final.</param>
        /// <returns>Parámetros de consulta.</returns>
        private static DynamicParameters CrearParametros(
            InvKardexMovimientosFiltro filtros,
            DateTime fechaInicio,
            DateTime fechaCorte)
        {
            string filtro =
                filtros.vfiltro.Trim();

            var parametros =
                new DynamicParameters();

            parametros.Add(
                "FechaInicio",
                fechaInicio.Date);

            parametros.Add(
                "FechaCorteExclusiva",
                fechaCorte.Date.AddDays(1));

            parametros.Add(
                "Todos",
                Todos);

            parametros.Add(
                "Tipo",
                filtros.tipo);

            parametros.Add(
                "CodBodega",
                filtros.cod_bodega);

            parametros.Add(
                "CodProducto",
                filtros.cod_producto);

            parametros.Add(
                "Filtro",
                filtro);

            parametros.Add(
                "FiltroBusqueda",
                string.IsNullOrEmpty(filtro)
                    ? string.Empty
                    : $"%{filtro}%");

            parametros.Add(
                "Offset",
                filtros.pagina);

            parametros.Add(
                "Fetch",
                filtros.paginacion);

            return parametros;
        }

        /// <summary>
        /// Crea una respuesta vacía de movimientos.
        /// </summary>
        /// <returns>Respuesta vacía inicializada.</returns>
        private static InvKardexMovimientosListaDto
            CrearResultadoVacio()
        {
            return new InvKardexMovimientosListaDto
            {
                total = 0,
                movimientos = []
            };
        }

        /// <summary>
        /// Crea el acceso a configuración de base de datos.
        /// </summary>
        /// <returns>Instancia de PortalDB.</returns>
        private PortalDB CrearPortalDb()
        {
            return new PortalDB(_config);
        }
    }
}