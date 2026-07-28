using System.Globalization;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrSeguimientoTramitesDb
    {
        private static List<CrSeguimientoTramitesOpcionItem> Cr_SeguimientoTramites_Opciones_Mapear(
            IEnumerable<CrSeguimientoTramitesOpcionRaw> opciones)
        {
            return opciones.Select(opcion => new CrSeguimientoTramitesOpcionItem
            {
                item = Convert.ToString(opcion.idx, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty,
                descripcion = opcion.itmx.Trim()
            }).ToList();
        }

        private static string Cr_SeguimientoTramites_Filtro_Normalizar(string? valor, int longitudMaxima)
        {
            string normalizado = (valor ?? string.Empty).Trim();
            return normalizado.Length <= longitudMaxima
                ? normalizado
                : normalizado[..longitudMaxima];
        }

        private static List<CrSeguimientoTramitesOpcionItem> Cr_SeguimientoTramites_TiposDocumento_Crear()
        {
            return
            [
                new() { item = "CK", descripcion = "Cheque" },
                new() { item = "TE", descripcion = "Transferencia" },
                new() { item = "TS", descripcion = "Transferencia SINPE" },
                new() { item = "ND", descripcion = "Nota Débito" },
                new() { item = "CD", descripcion = "Control de Desembolsos" },
                new() { item = "CP", descripcion = "Proveedor" },
                new() { item = "RC", descripcion = "Retiro en Caja" }
            ];
        }

        private static List<CrSeguimientoTramitesOpcionItem> Cr_SeguimientoTramites_Estados_Crear(
            string estadoSolicitud)
        {
            return estadoSolicitud.ToUpperInvariant() switch
            {
                "R" or "P" =>
                [
                    new() { item = "R", descripcion = "Recibida" },
                    new() { item = "P", descripcion = "Pendiente" },
                    new() { item = "D", descripcion = "Denegada" }
                ],
                "A" or "D" =>
                [
                    new() { item = "A", descripcion = "Aprobada" },
                    new() { item = "D", descripcion = "Denegada" }
                ],
                "N" or "F" =>
                [
                    new() { item = "N", descripcion = "Anulada" },
                    new() { item = "F", descripcion = "Formalizada" }
                ],
                _ => []
            };
        }

        private static string Cr_SeguimientoTramites_EstadoTooltip_Crear(
            CrSeguimientoTramitesOperacionData operacion)
        {
            return operacion.estadosol.ToUpperInvariant() switch
            {
                "R" => Cr_SeguimientoTramites_Tooltip_Formatear(
                    "Solicitado por",
                    operacion.userrec,
                    operacion.fechasol),
                "P" => Cr_SeguimientoTramites_Tooltip_Formatear(
                    "Pendiente",
                    operacion.userrec,
                    operacion.fechasol),
                "D" => Cr_SeguimientoTramites_Tooltip_Formatear(
                    "Denegado",
                    operacion.userrec,
                    operacion.fechasol),
                "F" => Cr_SeguimientoTramites_Tooltip_Formatear(
                    "Formalizado:",
                    operacion.userfor,
                    operacion.fecha_registro ?? operacion.fechaforp),
                "N" => Cr_SeguimientoTramites_Tooltip_Formatear(
                    "Anulado por",
                    operacion.anula_usuario,
                    operacion.anula_fecha),
                _ => string.Empty
            };
        }

        private static string Cr_SeguimientoTramites_Tooltip_Formatear(
            string accion,
            string usuario,
            DateTime? fecha)
        {
            string fechaTexto = fecha?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? string.Empty;
            return $"{accion} {usuario.Trim()} - {fechaTexto}".Trim();
        }

        private static string Cr_SeguimientoTramites_SeccionInicial_Obtener(
            string estadoSolicitud,
            string estadoCredito)
        {
            if (!string.Equals(estadoCredito, "N", StringComparison.OrdinalIgnoreCase))
            {
                return "0-1";
            }

            return estadoSolicitud.ToUpperInvariant() switch
            {
                "A" or "F" or "N" => "0-1",
                _ => "0-0"
            };
        }
    }
}
