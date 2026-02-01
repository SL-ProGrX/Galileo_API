namespace Galileo_API.Models.ProGrX_Polizas
{
    public class PolizaBalancePagoResumenDto
    {
        public DateTime Corte { get; set; }
        public string Cod_Poliza { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Operaciones { get; set; }
        public decimal Monto_Asegurado { get; set; }
        public decimal Prima { get; set; }
        public decimal Monto_Pagado { get; set; }
        public decimal Monto_Recaudado { get; set; }
        public decimal Balanza { get; set; }
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Cta_Desc { get; set; } = string.Empty;
    }

    public class PolizaBalancePagoDetalleDto
    {
        public DateTime Corte { get; set; }
        public int TipoId { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Apellido_1 { get; set; } = string.Empty;
        public string Apellido_2 { get; set; } = string.Empty;
        public string Nombre_1 { get; set; } = string.Empty;
        public string Nombre_2 { get; set; } = string.Empty;
        public string Direccion_Persona { get; set; } = string.Empty;
        public DateTime? Fecha_Nacimiento { get; set; }
        public string Genero { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public decimal Saldo { get; set; }
        public int Id_Solicitud { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public DateTime Fecha_Emision { get; set; }
        public int Id_Poliza { get; set; }
        public string Cod_Poliza { get; set; } = string.Empty;
        public decimal Monto_Asegurado { get; set; }
        public decimal Prima { get; set; }
        public decimal Monto_Pagado { get; set; }
        public decimal Monto_Recaudado { get; set; }
        public decimal Balanza { get; set; }
    }

    public class PolizaBalancePagoParams
    {
        public string Poliza { get; set; } = string.Empty;
        public DateTime Corte { get; set; }
        public string TipoInforme { get; set; } = "R";
        public string Balanza { get; set; } = "T";
        public string? Cedula { get; set; }
        public string? AseguradoraId { get; set; } = "GEN";
    }
}
