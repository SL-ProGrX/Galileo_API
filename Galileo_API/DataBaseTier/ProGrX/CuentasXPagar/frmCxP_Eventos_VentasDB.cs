using Galileo.Models.CxP;
using System.Data;
using Galileo.Models.ERROR;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX.CuentasXPagar
{
    public class FrmCxPEventosVentasDB
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmCxPEventosVentasDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmCxPEventosVentasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene el listado de eventos de cuentas por pagar.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de eventos disponibles.</returns>
        public ErrorDto<List<CxpEventosDto>> Eventos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<CxpEventosDto>(
                CreatePortalDb(),
                CodEmpresa,
                "Select Cod_Evento as IdX, Descripcion as ItmX from CxP_Eventos order by Fecha_Inicio desc");
        }

        /// <summary>
        /// Obtiene el detalle de ventas por eventos de cuentas por pagar según los filtros recibidos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="parametros">Filtros serializados en formato JSON.</param>
        /// <returns>Listado de ventas filtradas por evento.</returns>
        public ErrorDto<List<CxpEventosVentasDto>> Eventos_Ventas_Obtener(int CodEmpresa, string parametros)
        {
            var info = JsonConvert.DeserializeObject<CxpEventosVentasFiltros>(parametros) ?? new CxpEventosVentasFiltros();
            if (info.id_venta == 0)
            {
                info.id_venta = null;
            }

            return DbHelper.ExecuteListQuery<CxpEventosVentasDto>(
                CreatePortalDb(),
                CodEmpresa,
                "spCxP_Eventos_Ventas",
                new
                {
                    EventoId = info.id_venta,
                    inicio = info.inicio,
                    corte = info.corte,
                    proveedorId = info.proveedorId,
                    proveedorNombre = info.proveedorNombre,
                    cedula = info.cedula,
                    nombre = info.nombre,
                    usuario = info.usuario,
                    appcod = info.appcod
                });
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);
    }
}
