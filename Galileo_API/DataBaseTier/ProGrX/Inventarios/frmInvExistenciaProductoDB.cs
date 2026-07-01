using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Newtonsoft.Json;
using System.Globalization;

namespace Galileo.DataBaseTier
{
    public class FrmInvExistenciaProductoDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvExistenciaProductoDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvExistenciaProductoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Convierte el filtro JSON en un objeto tipado.
        /// </summary>
        /// <param name="filtroString">Filtro serializado en JSON.</param>
        /// <returns>Objeto de filtros inicializado.</returns>
        private static ExistenciaProductoFiltros ObtenerFiltros(string filtroString)
        {
            return JsonConvert.DeserializeObject<ExistenciaProductoFiltros>(filtroString) ?? new ExistenciaProductoFiltros();
        }

        /// <summary>
        /// Valida y normaliza una fecha del filtro.
        /// </summary>
        /// <param name="valor">Valor de fecha recibido.</param>
        /// <param name="nombreCampo">Nombre del campo para el mensaje de error.</param>
        /// <returns>Fecha normalizada en formato yyyy-MM-dd.</returns>
        /// <exception cref="FormatException">Se lanza cuando la fecha no tiene un formato válido.</exception>
        private static string NormalizarFecha(string? valor, string nombreCampo)
        {
            if (!DateTimeOffset.TryParse(valor, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset fecha))
            {
                throw new FormatException($"El valor de '{nombreCampo}' no tiene un formato válido.");
            }

            return fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene la existencia del producto por bodega.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="filtroString">Filtro serializado en JSON.</param>
        /// <returns>Listado de existencias por bodega.</returns>
        public ErrorDto<List<ExistenciaProductoDto>> existenciaProducto_Obtener(int CodCliente, string filtroString)
        {
            try
            {
                var filtros = ObtenerFiltros(filtroString);

                _ = NormalizarFecha(filtros.fecha_inicio, "fecha_inicio");
                _ = NormalizarFecha(filtros.fecha_corte, "fecha_corte");

                return DbHelper.ExecuteListQuery<ExistenciaProductoDto>(
                    CreatePortalDb(),
                    CodCliente,
                    @"SELECT b.Cod_Bodega AS Bodega,
                             b.Descripcion AS Descripcion,
                             SUM(ip.existencia_inicial + ip.entradas - ip.salidas) AS Existencia
                      FROM pv_bodegas b
                      JOIN pv_inventario_proceso ip ON b.Cod_Bodega = ip.cod_bodega
                      WHERE ip.cod_producto = @CodProducto
                      GROUP BY b.COD_BODEGA, b.DESCRIPCION",
                    new
                    {
                        CodProducto = filtros.cod_Producto
                    });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new List<ExistenciaProductoDto>());
            }
        }

        #endregion
    }
}