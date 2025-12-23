namespace Galileo.DataBaseTier
{
    public partial class MTesoreria
    {
        private static class Mappers
        {
            public const string PERM_SOLICITA = "SOLICITA";
            public const string PERM_AUTORIZA = "AUTORIZA";
            public const string PERM_GENERA = "GENERA";
            public const string PERM_ASIENTOS = "ASIENTOS";
            public const string PERM_ANULA = "ANULA";

            public static string GestionFromCodigo(string vGestion) => (vGestion ?? "S").Trim().ToUpperInvariant() switch
            {
                "S" => PERM_SOLICITA,
                "A" => PERM_AUTORIZA,
                "G" => PERM_GENERA,
                "X" => PERM_ASIENTOS,
                "N" => PERM_ANULA,
                _ => PERM_SOLICITA
            };

            public static string NormalizePermiso(string permiso) => (permiso ?? "").Trim().ToUpperInvariant() switch
            {
                PERM_SOLICITA => PERM_SOLICITA,
                PERM_AUTORIZA => PERM_AUTORIZA,
                PERM_GENERA => PERM_GENERA,
                PERM_ASIENTOS => PERM_ASIENTOS,
                PERM_ANULA => PERM_ANULA,
                _ => throw new ArgumentException("Permiso inválido", nameof(permiso))
            };

            public static string NormalizeBancoDocsCampo(string campo) => (campo ?? "").Trim() switch
            {
                "Comprobante" => "Comprobante",
                "Consecutivo" => "Consecutivo",
                "CONSECUTIVO_DET" => "CONSECUTIVO_DET",
                "DOC_AUTO" => "DOC_AUTO",
                "Movimiento" => "Movimiento",
                _ => throw new ArgumentException("Campo inválido", nameof(campo))
            };

            public static string NormalizePlan(string tipo, string plan)
            {
                if (!string.Equals(tipo, "TE", StringComparison.OrdinalIgnoreCase) && plan != "-sp-")
                    return "-sp-";
                return string.IsNullOrWhiteSpace(plan) ? "-sp-" : plan;
            }
        }
    }
}