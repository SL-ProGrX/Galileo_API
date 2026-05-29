namespace Galileo.Models.ProGrX.Clientes
{
    public class UsMovimiento
    {
        public string? Movimiento { get; set; }
        public string? Descripcion { get; set; }
    }

    public class FndBitacoraCambiosRequest
    {
        public string? Cedula { get; set; }
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
        public List<string>? Movimientos { get; set; }
        public string? CodPlan { get; set; }
        public int? CodOperadora { get; set; }
        public int? CodContrato { get; set; }
        public string? SoloNoRevisados { get; set; }
    }

    public class FndBitacoraCambiosResult
    {
        public int Id_Bitacora { get; set; }
        public int Cod_Operadora { get; set; }
        public string? Cod_Plan { get; set; }
        public int Cod_Contrato { get; set; }
        public string? Usuario { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Movimiento { get; set; }
        public string? Detalle { get; set; }
        public string? Revisado_Usuario { get; set; }
        public DateTime? Revisado_Fecha { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? MovimientoDesc { get; set; }
        public int Revisado { get; set; }
    }

    public class FndBitacoraCambioRevisarRequest
    {
        public required int Id_Bitacora { get; set; }
        public string Revisado_Usuario { get; set; } = string.Empty;
    }

    public class SifRegistraTagsRequest
    {
        public string Codigo { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Observacion { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string Modulo { get; set; } = string.Empty;
        public string Llave_01 { get; set; } = string.Empty;
        public string Llave_02 { get; set; } = string.Empty;
        public string Llave_03 { get; set; } = string.Empty;
    }
}