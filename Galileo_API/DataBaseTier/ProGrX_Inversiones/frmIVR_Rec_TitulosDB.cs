using Galileo.Models.ProGrX_Inversiones;

namespace Galileo.DataBaseTier.ProGrX_Inversiones
{
    public sealed partial class FrmIvrRecTitulosDb
    {
        private const int CodigoValidacion = -2;
        private readonly PortalDB _portalDb;

        public FrmIvrRecTitulosDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        private static bool IVR_Rec_Titulos_Id_Valido(long tituloId) =>
            tituloId > 0 && tituloId <= 9999999999;

        private static string IVR_Rec_Titulos_Texto_Normalizar(
            string? valor,
            int longitudMaxima)
        {
            var texto = (valor ?? string.Empty).Trim();
            return texto.Length <= longitudMaxima
                ? texto
                : texto[..longitudMaxima];
        }

        private static string IVR_Rec_Titulos_Tipo_Movimiento_Descripcion(
            string? tipoMovimiento) =>
            (tipoMovimiento ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "F" => "Fondo Cierre",
                "C" => "Cambio",
                "A" => "Aporte",
                "R" => "Retiro",
                _ => tipoMovimiento ?? string.Empty,
            };
    }
}
