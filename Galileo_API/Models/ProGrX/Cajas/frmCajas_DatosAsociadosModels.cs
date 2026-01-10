namespace Galileo_API.Models.ProGrX.Cajas
{

    public class CajasCreditoDto
    {
        public int? id_solicitud { get; set; }
        public string? codigo { get; set; }
        public string? garantia { get; set; }
        public decimal? saldo { get; set; }
        public decimal? mora { get; set; }
        public decimal? cuota { get; set; }
        public string? linea_desc { get; set; }
    }

    public class CajasFondosDto
    {
        public string? contrato { get; set; }
        public string? plan { get; set; }
        public decimal? aportes { get; set; }
        public decimal? rendimiento { get; set; }
        public decimal? acumulado { get; set; }
        public decimal? monto { get; set; }
        public string? descripcion { get; set; }
    }

    public class CajasCxcDto
    {
        public string? operacion { get; set; }
        public string? documento { get; set; }
        public DateTime? fecha { get; set; }
        public decimal? monto { get; set; }
        public decimal? saldo { get; set; }
        public decimal? cuota { get; set; }
        public string? estado { get; set; }
    }

    public class CajasServiciosDto
    {
        public string? servicio { get; set; }
        public decimal? monto { get; set; }
        public DateTime? fecha { get; set; }
        public string? referencia { get; set; }
        public string? caja { get; set; }
        public string? usuario { get; set; }
    }

    public class CajasSaldoFavorDto
    {
        public int? linea { get; set; }
        public string? documento { get; set; }
        public DateTime? fecha { get; set; }
        public decimal? monto { get; set; }
        public decimal? saldo { get; set; }
        public string? referencia { get; set; }
    }

    public class CajasReciboMultipleDto
    {
        public int? recibo { get; set; }
        public decimal? monto { get; set; }
        public DateTime? fecha { get; set; }
        public string? caja { get; set; }
        public int? apertura { get; set; }
        public string? usuario { get; set; }
    }


}