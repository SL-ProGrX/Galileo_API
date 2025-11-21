namespace Galileo.Models.AH
{
    public class ExcedentePeriodoDto
    {
        public int id_periodo { get; set; }
        public DateTime? Inicio { get; set; }
        public DateTime? Corte { get; set; }
        public string? Estado { get; set; }
        public int Reserva { get; set; }
        public int Capitaliza_Porc { get; set; }
        public bool Capitaliza_Renta_Aplica { get; set; }
        public int Nc_Saldos { get; set; }
        public int Nc_Mora { get; set; }
        public string Nc_Opcf { get; set; } = string.Empty;
        public bool Visible_Sys { get; set; }
        public bool Visible_Webapp { get; set; }
    }

    public class BitacoraExcedenteDto
    {
        public int linea { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public int Transaccion { get; set; }
        public string Detalle { get; set; } = string.Empty;
        public string Tipo_Documento { get; set; } = string.Empty;
        public string Cod_Transaccion { get; set; } = string.Empty;
    }
}
