namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class RemesasTesoreriaObtenerDto
    {
        public int Remesa { get; set; }
        public string? RegistroUsuario { get; set; }
        public DateTime? RegistroFecha { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCorte { get; set; }
        public string? Notas { get; set; }
        public string? Estado { get; set; }
        public int Casos { get; set; }
        public decimal Monto { get; set; }
    }
    public class RemesaTesoreriaUpsertDto
    {
        public int? Remesa { get; set; } // null para insert, valor para update
        public string Usuario { get; set; } = string.Empty;
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaCorte { get; set; }
        public string Notas { get; set; } = string.Empty;
    }
    public class RemesaTesoreriaDesembolsoDisponibleDto : RemesaTesoreriaDesembolsoAsignadoDto
    {        
        public DateTime? TES_SUPERVISION_FECHA { get; set; }
        public int Duplicado { get; set; }
    }
    public class RemesaTesoreriaExisteDto
    {
        public int Existe { get; set; }
    }
    public class RemesaTesoreriaDesembolsoAsignadoDto
    {
        public int CodigoDesembolso { get; set; }
        public int NumeroOperacion { get; set; }
        public string Beneficiario { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public DateTime RegistroFecha { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
    }
}
