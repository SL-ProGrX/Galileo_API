using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;
using System.Globalization;

namespace Galileo.DataBaseTier
{
    public class FrmInvKardexDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvKardexDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvKardexDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta vacía para el listado de movimientos.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static MovimientosDtoList CrearListaVacia() => new()
        {
            Total = 0,
            Movimientos = new List<MovimientosDto>()
        };

        /// <summary>
        /// Obtiene el filtro tipado desde la cadena JSON.
        /// </summary>
        /// <param name="filtroString">Cadena JSON con los filtros.</param>
        /// <returns>Objeto de filtros inicializado.</returns>
        private static MovimientosInventariosFiltros ObtenerFiltros(string filtroString)
        {
            return JsonConvert.DeserializeObject<MovimientosInventariosFiltros>(filtroString) ?? new MovimientosInventariosFiltros();
        }

        /// <summary>
        /// Valida y normaliza una fecha del filtro.
        /// </summary>
        /// <param name="valor">Valor de fecha recibido.</param>
        /// <param name="nombreCampo">Nombre del campo para mensajes de error.</param>
        /// <returns>Fecha formateada en yyyy-MM-dd.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando la fecha es nula o vacía.</exception>
        /// <exception cref="FormatException">Se lanza cuando la fecha no tiene formato válido.</exception>
        private static string NormalizarFecha(string? valor, string nombreCampo)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                throw new ArgumentNullException(nombreCampo, $"{nombreCampo} is required");
            }

            if (!DateTimeOffset.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset fecha))
            {
                throw new FormatException($"El valor de '{nombreCampo}' no tiene un formato válido.");
            }

            return fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Agrega los filtros del kardex a la consulta y parámetros.
        /// </summary>
        /// <param name="filtros">Filtros de búsqueda.</param>
        /// <param name="whereBuilder">Cláusula WHERE a construir.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltrosKardex(MovimientosInventariosFiltros filtros, System.Text.StringBuilder whereBuilder, DynamicParameters parametros)
        {
            string fechaInicio = NormalizarFecha(filtros.fecha_inicio, nameof(filtros.fecha_inicio));
            string fechaCorte = NormalizarFecha(filtros.fecha_corte, nameof(filtros.fecha_corte));

            whereBuilder.Append(" WHERE M.fecha BETWEEN @FechaInicio AND @FechaCorte ");
            parametros.Add("FechaInicio", fechaInicio + " 00:00:00");
            parametros.Add("FechaCorte", fechaCorte + " 23:59:59");

            if (!string.Equals(filtros.Tipo, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(filtros.Tipo, "E", StringComparison.OrdinalIgnoreCase) || string.Equals(filtros.Tipo, "S", StringComparison.OrdinalIgnoreCase))
                {
                    whereBuilder.Append(" AND M.Tipo = @Tipo ");
                    parametros.Add("Tipo", filtros.Tipo);
                }
                else
                {
                    whereBuilder.Append(" AND M.Origen = @Origen ");
                    parametros.Add("Origen", filtros.Tipo);
                }
            }

            if (!string.Equals(filtros.cod_Bodega, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                whereBuilder.Append(" AND M.COD_BODEGA = @CodBodega ");
                parametros.Add("CodBodega", filtros.cod_Bodega);
            }

            if (!string.Equals(filtros.cod_Producto, "Todos", StringComparison.OrdinalIgnoreCase))
            {
                whereBuilder.Append(" AND M.cod_producto = @CodProducto ");
                parametros.Add("CodProducto", filtros.cod_Producto);
            }

            if (!string.IsNullOrWhiteSpace(filtros.vfiltro))
            {
                whereBuilder.Append(@" AND (
                                        M.cod_producto LIKE @Filtro
                                        OR P.descripcion LIKE @Filtro
                                        OR M.codigo LIKE @Filtro
                                        OR CONVERT(varchar(30), M.Fecha, 120) LIKE @Filtro
                                      )");
                parametros.Add("Filtro", $"%{filtros.vfiltro.Trim()}%");
            }
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH a la consulta.
        /// </summary>
        /// <param name="pagina">Fila inicial.</param>
        /// <param name="paginacion">Cantidad de filas.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarPaginacion(int? pagina, int? paginacion, System.Text.StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (!pagina.HasValue || !paginacion.HasValue)
            {
                return;
            }

            queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY ");
            parametros.Add("Offset", pagina.Value);
            parametros.Add("Fetch", paginacion.Value);
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene las bodegas disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de bodegas.</returns>
        public ErrorDto<List<ConsultaMovimientoBodegaCDdto>> Obtener_Bodegas(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<ConsultaMovimientoBodegaCDdto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT COD_BODEGA, DESCRIPCION FROM PV_BODEGAS");
        }

        /// <summary>
        /// Obtiene los movimientos del kardex filtrados y paginados.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="filtroString">Cadena JSON con los filtros.</param>
        /// <returns>Listado de movimientos del kardex.</returns>
        public ErrorDto<MovimientosDtoList> consultarMovimientos_Obtener(int CodCliente, string filtroString)
        {
            try
            {
                var filtros = ObtenerFiltros(filtroString);
                var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
                {
                    var respuesta = CrearListaVacia();
                    var parametros = new DynamicParameters();
                    var whereBuilder = new System.Text.StringBuilder();

                    AgregarFiltrosKardex(filtros, whereBuilder, parametros);

                    string whereClause = whereBuilder.ToString();

                    string totalQuery = @"SELECT COUNT(M.cod_producto)
                                          FROM pv_inventario_mov M
                                          INNER JOIN pv_productos P ON M.cod_producto = P.cod_producto
                                          INNER JOIN pv_Bodegas B ON M.cod_bodega = B.cod_bodega"
                                          + whereClause;

                    respuesta.Total = connection.QueryFirstOrDefault<int>(totalQuery, parametros);

                    var detalleQuery = new System.Text.StringBuilder(@"SELECT
                                    M.Fecha,
                                    (RTRIM(M.cod_producto) + ' - ' + RTRIM(P.descripcion)) AS Producto,
                                    CASE M.tipo
                                        WHEN 'E' THEN 'ENTRADA'
                                        WHEN 'S' THEN 'SALIDA'
                                    END AS TipoX,
                                    M.origen,
                                    M.codigo,
                                    ISNULL(M.existencia, 0) AS Existencia,
                                    M.cantidad,
                                    CASE
                                        WHEN M.tipo = 'E' THEN ISNULL(M.existencia, 0) + M.Cantidad
                                        WHEN M.tipo = 'S' THEN ISNULL(M.existencia, 0) - M.Cantidad
                                    END AS ExistenciaX,
                                    M.precio,
                                    (M.cantidad * M.precio) AS TotalSinImp,
                                    (M.cantidad * M.precio) * (M.imp_ventas / 100) AS ImpVentas,
                                    (M.cantidad * M.precio) * (M.imp_consumo / 100) AS ImpConsumo,
                                    (M.cantidad * M.precio) + ((M.cantidad * M.precio) * (M.imp_ventas / 100)) + ((M.cantidad * M.precio) * (M.imp_consumo / 100)) AS TotalConImp,
                                    (RTRIM(M.cod_bodega) + ' - ' + RTRIM(B.descripcion)) AS Bodega,
                                    dbo.fxINVBodegaTraslado(M.Origen, M.Tipo, M.Linea) AS BodegaEnlace
                                FROM pv_inventario_mov M
                                INNER JOIN pv_productos P ON M.cod_producto = P.cod_producto
                                INNER JOIN pv_Bodegas B ON M.cod_bodega = B.cod_bodega");

                    detalleQuery.Append(whereClause);
                    detalleQuery.Append(" ORDER BY M.Fecha desc ");
                    AgregarPaginacion(filtros.pagina, filtros.paginacion, detalleQuery, parametros);

                    respuesta.Movimientos = connection.Query<MovimientosDto>(detalleQuery.ToString(), parametros).ToList();
                    return respuesta;
                });

                return result.Code == 0
                    ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                    : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener los movimientos del kardex.", result.Code.GetValueOrDefault(-1), CrearListaVacia());
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, CrearListaVacia());
            }
        }

        #endregion
    }
}