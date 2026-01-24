namespace Galileo_API.Models.ProGrX_Nucleo
{
    public class SugefTipoCambioResult
    {
        public decimal? TC { get; set; }
    }

    public class SugefRomMonitorConsultaResult
    {
        public DateTime Corte { get; set; }
        public string? NumerdoIdentificacion { get; set; }
        public string? PrimerApellidoCliente { get; set; }
        public string? SegundoApellidoCliente { get; set; }
        public string? NombreCliente { get; set; }
        public decimal? MontoMovimiento { get; set; }
        public decimal? SalarioPromedio { get; set; }
        public decimal? Monto_Col { get; set; }
        public decimal? Salario_Col { get; set; }
        public decimal? Monto_Dol { get; set; }
        public decimal? Salario_Dol { get; set; }
        public string? NombreClienteCompleto { get; set; }
        public string? Cedula { get; set; }
        public string? Tipo_Id_Desc { get; set; }
    }
}
