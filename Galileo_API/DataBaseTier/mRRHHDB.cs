namespace Galileo.DataBaseTier
{
    public static class MrrhhDb
    {
        public static string fxTipoDocumento(string? tipo)
        {
            return (tipo ?? string.Empty).Trim() switch
            {
                "CK" => "Cheque",
                "TE" => "Transferencia",
                "EF" or "RE" => "Efectivo",
                "ND" => "Nota Debito",
                "NC" => "Nota Credito",
                "OT" => "Otro...",
                "CD" => "Ctrl Desembolsos",

                "Cheque" => "CK",
                "Transferencia" => "TE",
                "Efectivo" => "EF",
                "Nota Debito" => "ND",
                "Nota Credito" => "NC",
                "Otro..." => "OT",
                "Ctrl Desembolsos" => "CD",

                _ => string.Empty
            };
        }
    }
}