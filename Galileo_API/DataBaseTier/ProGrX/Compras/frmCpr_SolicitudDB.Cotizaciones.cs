using Dapper;
using Galileo.Models.CPR;
using System.Data;

namespace Galileo.DataBaseTier
{
    public partial class FrmCprSolicitudDB
    {
        private static void CprSolicitudCotizacionBs_UnidadesAsignar(
            IDbConnection conn,
            List<CprSolicitudCotizacionPrvBs> cotizaciones)
        {
            var productos = CprSolicitudCotizacionBs_ProductosObtener(cotizaciones);
            if (productos.Count == 0)
            {
                return;
            }

            const string qUnidades = "SELECT COD_PRODUCTO, COD_UNIDAD FROM PV_PRODUCTOS WHERE COD_PRODUCTO IN @Productos;";
            var map = conn.Query<(string COD_PRODUCTO, string COD_UNIDAD)>(qUnidades, new { Productos = productos })
                .ToDictionary(x => x.COD_PRODUCTO, x => x.COD_UNIDAD);

            foreach (var item in cotizaciones)
            {
                CprSolicitudCotizacionBs_UnidadAsignar(item, map);
            }
        }

        private static List<string> CprSolicitudCotizacionBs_ProductosObtener(
            IEnumerable<CprSolicitudCotizacionPrvBs> cotizaciones)
        {
            return cotizaciones
                .Select(item => item.cod_producto)
                .OfType<string>()
                .Where(codProducto => !string.IsNullOrWhiteSpace(codProducto))
                .Distinct()
                .ToList();
        }

        private static void CprSolicitudCotizacionBs_UnidadAsignar(
            CprSolicitudCotizacionPrvBs item,
            IReadOnlyDictionary<string, string> unidades)
        {
            var codProducto = item.cod_producto;
            if (string.IsNullOrWhiteSpace(codProducto))
            {
                return;
            }

            if (unidades.TryGetValue(codProducto, out string? unidad))
            {
                item.unidad = unidad;
            }
        }
    }
}
