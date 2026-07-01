using Dapper;
using Newtonsoft.Json;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Globalization;

namespace Galileo.DataBaseTier
{
    public class FrmInvOrdenesAutorizacionDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvOrdenesAutorizacionDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvOrdenesAutorizacionDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Obtiene el filtro tipado desde la cadena JSON.
        /// </summary>
        /// <param name="filtroString">Cadena JSON con los filtros.</param>
        /// <returns>Objeto de filtros inicializado.</returns>
        private static ResolucionTransaccionFiltros ObtenerFiltros(string filtroString)
        {
            return JsonConvert.DeserializeObject<ResolucionTransaccionFiltros>(filtroString) ?? new ResolucionTransaccionFiltros();
        }

        /// <summary>
        /// Valida y normaliza una fecha del filtro.
        /// </summary>
        /// <param name="valor">Valor de fecha recibido.</param>
        /// <param name="nombreCampo">Nombre del campo para mensajes de error.</param>
        /// <returns>Fecha formateada en yyyy-MM-dd.</returns>
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
        /// Agrega el filtro de fechas a la consulta de resolución de transacciones.
        /// </summary>
        /// <param name="filtros">Filtros de búsqueda.</param>
        /// <param name="whereBuilder">Builder del WHERE.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroFecha(ResolucionTransaccionFiltros filtros, System.Text.StringBuilder whereBuilder, DynamicParameters parametros)
        {
            if (filtros.fecha == "0")
            {
                string fechaInicio = NormalizarFecha(filtros.fecha_inicio, nameof(filtros.fecha_inicio));
                string fechaCorte = NormalizarFecha(filtros.fecha_corte, nameof(filtros.fecha_corte));

                whereBuilder.Append(" WHERE R.Genera_Fecha BETWEEN @FechaInicio AND @FechaCorte ");
                parametros.Add("FechaInicio", fechaInicio + " 00:00:00");
                parametros.Add("FechaCorte", fechaCorte + " 23:59:59");
                return;
            }

            whereBuilder.Append(" WHERE R.Genera_Fecha BETWEEN @FechaInicio AND @FechaCorte ");
            parametros.Add("FechaInicio", "1900-01-01 23:59:59");
            parametros.Add("FechaCorte", "2999-01-01 23:59:59");
        }

        /// <summary>
        /// Actualiza el estado de una orden seleccionada.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="tipo">Tipo de orden.</param>
        /// <param name="usuario">Usuario que autoriza o rechaza.</param>
        /// <param name="item">Orden a procesar.</param>
        /// <param name="estado">Estado a aplicar.</param>
        private static void ActualizarEstadoOrden(System.Data.IDbConnection connection, string tipo, string usuario, ResolucionTransaccionDto item, string estado)
        {
            if (tipo == "R")
            {
                connection.Execute(
                    @"update pv_requisiciones
                      set autoriza_fecha = GetDate(),
                          autoriza_user = @Usuario,
                          estado = @Estado
                      where cod_requisicion = @CodOrden",
                    new
                    {
                        Usuario = usuario,
                        Estado = estado,
                        CodOrden = item.Cod_Orden
                    });

                return;
            }

            connection.Execute(
                @"update pv_InvTranSac
                  set autoriza_fecha = GetDate(),
                      autoriza_user = @Usuario,
                      estado = @Estado
                  where boleta = @CodOrden
                    and tipo = @Tipo",
                new
                {
                    Usuario = usuario,
                    Estado = estado,
                    CodOrden = item.Cod_Orden,
                    Tipo = tipo
                });
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene las transacciones pendientes de resolución según los filtros indicados.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="filtroString">Cadena JSON con los filtros.</param>
        /// <returns>Listado de transacciones pendientes.</returns>
        public ErrorDto<List<ResolucionTransaccionDto>> resolucionTransaccion_Obtener(int CodCliente, string filtroString)
        {
            try
            {
                var filtros = ObtenerFiltros(filtroString);
                var parametros = new DynamicParameters();
                var whereBuilder = new System.Text.StringBuilder();
                AgregarFiltroFecha(filtros, whereBuilder, parametros);

                if (filtros.tipo == "R")
                {
                    string query = @"SELECT
                                        R.cod_requisicion AS Cod_Orden,
                                        'Requisiones' AS Tipo_Orden,
                                        0 AS Total,
                                        R.Genera_User AS User_Solicita,
                                        R.Genera_Fecha AS Fecha,
                                        C.descripcion AS Causa,
                                        R.notas AS Nota,
                                        'Proceso' AS proceso
                                     FROM pv_requisiciones R
                                     INNER JOIN pv_entrada_salida C ON R.cod_entsal = C.cod_entsal"
                                     + whereBuilder
                                     + " AND R.ESTADO = 'P'";

                    return DbHelper.ExecuteListQuery<ResolucionTransaccionDto>(
                        CreatePortalDb(),
                        CodCliente,
                        query,
                        parametros);
                }

                parametros.Add("Tipo", filtros.tipo);
                string inventarioQuery = @"SELECT 
                                            R.boleta AS Cod_Orden,
                                            CASE 
                                                WHEN R.tipo = 'E' THEN 'Entrada'
                                                WHEN R.tipo = 'S' THEN 'Salida'
                                                ELSE R.tipo
                                            END AS Tipo_Orden,
                                            R.total,
                                            R.GENERA_USER,
                                            R.Genera_Fecha AS Fecha,
                                            C.descripcion AS Causa,
                                            R.notas AS Nota,
                                            'Proceso' AS proceso
                                          FROM pv_InvTranSac R
                                          INNER JOIN pv_entrada_salida C ON R.cod_entsal = C.cod_entsal"
                                          + whereBuilder
                                          + " AND R.TIPO = @Tipo AND R.ESTADO = 'P'";

                return DbHelper.ExecuteListQuery<ResolucionTransaccionDto>(
                    CreatePortalDb(),
                    CodCliente,
                    inventarioQuery,
                    parametros);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<ResolucionTransaccionDto>());
            }
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Autoriza las órdenes seleccionadas.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="tipo">Tipo de orden.</param>
        /// <param name="usuario">Usuario que autoriza.</param>
        /// <param name="lista">Listado de órdenes.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ResolucionTransaccion_Autorizar(int CodCliente, string tipo, string usuario, List<ResolucionTransaccionDto> lista)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodCliente, connection =>
            {
                foreach (var item in lista.Where(x => x.seleccionado == true))
                {
                    ActualizarEstadoOrden(connection, tipo, usuario, item, "A");
                }

                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al autorizar las transacciones.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Rechaza las órdenes seleccionadas.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="tipo">Tipo de orden.</param>
        /// <param name="usuario">Usuario que rechaza.</param>
        /// <param name="lista">Listado de órdenes.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ResolucionTransaccion_Rechazo(int CodCliente, string tipo, string usuario, List<ResolucionTransaccionDto> lista)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodCliente, connection =>
            {
                foreach (var item in lista.Where(x => x.seleccionado == true))
                {
                    ActualizarEstadoOrden(connection, tipo, usuario, item, "R");
                }

                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al rechazar las transacciones.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}