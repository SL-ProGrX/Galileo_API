namespace Galileo.Models.ProGrX.Clientes
{
    public class FndPlanesItem
    {
        public string? Cod_Plan { get; set; }
        public string? Descripcion { get; set; }
    }

    public class FndPlanesObtenerRequest
    {
        public string OrdenarPor { get; set; } = "cod_plan"; // "cod_plan" o "descripcion"
    }

    public class FndVencimientosConsultaRequest
    {
        public string Plan { get; set; } = string.Empty;
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? TipoFondo { get; set; }
        public short TipoCDP { get; set; }
    }

    public class FndVencimientosConsultaResult
    {
        public required string Cedula { get; set; }
        public int Cod_Operadora { get; set; }
        public required string Cod_Plan { get; set; }
        public int Cod_Contrato { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido1 { get; set; }
        public string? Apellido2 { get; set; }
        public string? Email { get; set; }
        public string? Movil { get; set; }
        public string? Tel_Hab { get; set; }
        public string? Provincia { get; set; }
        public string? Canton { get; set; }
        public string? Plan_Desc { get; set; }
        public string? Institucion { get; set; }
        public decimal Monto { get; set; }
        public decimal Total { get; set; }
        public int Plazo { get; set; }
        public DateTime Ultimo_Mov { get; set; }
        public DateTime Fecha_Inicio { get; set; }
        public DateTime Fecha_Corte { get; set; }
        public string? Departamento { get; set; }
        public string? Oficina { get; set; }
        public decimal? Tasa_Referencia { get; set; }
        public decimal? Tasa_Original { get; set; }
        public string? Destino { get; set; }
        public string? Objetivo { get; set; }
    }
}