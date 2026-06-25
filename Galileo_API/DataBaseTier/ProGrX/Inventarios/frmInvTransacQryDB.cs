using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvTransacQryDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTransacQryDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTransacQryDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene el nombre de la columna de fecha según el tipo indicado.
        /// </summary>
        /// <param name="tipoFecha">Tipo de fecha.</param>
        /// <returns>Nombre de columna o cadena vacía.</returns>
        private static string ObtenerCampoFecha(string? tipoFecha)
        {
            return tipoFecha switch
            {
                "S" => "genera_fecha",
                "A" => "autoriza_fecha",
                "P" => "procesa_fecha",
                "I" => "fecha",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Obtiene el nombre de la columna de usuario según el tipo indicado.
        /// </summary>
        /// <param name="tipoUsuario">Tipo de usuario.</param>
        /// <returns>Nombre de columna o cadena vacía.</returns>
        private static string ObtenerCampoUsuario(string? tipoUsuario)
        {
            return tipoUsuario switch
            {
                "S" => "genera_user",
                "A" => "autoriza_user",
                "P" => "procesa_user",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Agrega filtros dinámicos seguros a la consulta de transacciones.
        /// </summary>
        /// <param name="parametrosEntrada">Parámetros de búsqueda.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltros(TransacQryParametros parametrosEntrada, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            queryBuilder.Append(" WHERE 1 = 1 ");

            if (!string.IsNullOrWhiteSpace(parametrosEntrada.Tipo))
            {
                queryBuilder.Append(" AND tipo LIKE @Tipo ");
                parametros.Add("Tipo", $"%{parametrosEntrada.Tipo.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(parametrosEntrada.Estado))
            {
                queryBuilder.Append(" AND estado = @Estado ");
                parametros.Add("Estado", parametrosEntrada.Estado.Trim());
            }

            string campoFecha = ObtenerCampoFecha(parametrosEntrada.TipoFecha);
            if (!string.IsNullOrWhiteSpace(campoFecha))
            {
                queryBuilder.Append($" AND {campoFecha} BETWEEN @FechaInicio AND @FechaCorte ");
                parametros.Add("FechaInicio", $"{parametrosEntrada.FechaInicio} 00:00:00");
                parametros.Add("FechaCorte", $"{parametrosEntrada.FechaCorte} 23:59:59");
            }

            string campoUsuario = ObtenerCampoUsuario(parametrosEntrada.TipoUsuario);
            if (!string.IsNullOrWhiteSpace(campoUsuario) && !string.IsNullOrWhiteSpace(parametrosEntrada.Usuario))
            {
                queryBuilder.Append($" AND {campoUsuario} LIKE @Usuario ");
                parametros.Add("Usuario", $"%{parametrosEntrada.Usuario.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(parametrosEntrada.vfiltro))
            {
                queryBuilder.Append(@" AND (
                                        boleta LIKE @FiltroGeneral
                                        OR notas LIKE @FiltroGeneral
                                        OR documento LIKE @FiltroGeneral
                                      ) ");
                parametros.Add("FiltroGeneral", $"%{parametrosEntrada.vfiltro.Trim()}%");
            }
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH a la consulta.
        /// </summary>
        /// <param name="pagina">Fila inicial.</param>
        /// <param name="paginacion">Cantidad de filas.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarPaginacion(int? pagina, int? paginacion, StringBuilder queryBuilder, DynamicParameters parametros)
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
        /// Obtiene las transacciones de inventario según los parámetros especificados utilizando filtros seguros.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="parametros">Parámetros de búsqueda.</param>
        /// <returns>Listado de transacciones y total.</returns>
        public ErrorDto<TransacQryDataList> TransacInv_Obtener(int CodEmpresa, TransacQryParametros parametros)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = new TransacQryDataList
                {
                    Total = 0,
                    Transacciones = new List<TransacQryData>()
                };

                var countParams = new DynamicParameters();
                var countQuery = new StringBuilder("SELECT COUNT(*) FROM pv_invTransac");
                AgregarFiltros(parametros, countQuery, countParams);
                respuesta.Total = connection.QueryFirstOrDefault<int>(countQuery.ToString(), countParams);

                var dataParams = new DynamicParameters();
                var dataQuery = new StringBuilder("SELECT * FROM pv_invTransac");
                AgregarFiltros(parametros, dataQuery, dataParams);
                dataQuery.Append(" ORDER BY fecha DESC ");
                AgregarPaginacion(parametros.pagina, parametros.paginacion, dataQuery, dataParams);
                respuesta.Transacciones = connection.Query<TransacQryData>(dataQuery.ToString(), dataParams).ToList();

                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new TransacQryDataList { Total = 0, Transacciones = new List<TransacQryData>() })
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener las transacciones de inventario.",
                    result.Code.GetValueOrDefault(-1),
                    new TransacQryDataList
                    {
                        Total = 0,
                        Transacciones = new List<TransacQryData>()
                    });
        }

        #endregion
    }
}