namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdActividadDto
    {
        public int Cod_Actividad { get; set; }
        public string? Descripcion { get; set; }
        public string? Cod_Cuenta { get; set; }
        public DateTime? FechaPeriocidad { get; set; }
        public DateTime? FechaLiq { get; set; }
        public bool Activa { get; set; }
        public string? CuentaX { get; set; }
        public string? Tipo { get; set; }
    }

    public class AfCdActividadComiteDto
    {
        public int Cod_Actividad { get; set; }
        public int Cod_Comite { get; set; }
        public string? Descripcion { get; set; }
    }

    public class AfCdActividadSimpleDto
    {
        public int Cod_Actividad { get; set; }
        public string? Descripcion { get; set; }
    }

    public class AfCdActividadRangoDto
    {
        public int Cod_Monto { get; set; }
        public decimal Monto { get; set; }
        public decimal Minimo { get; set; }
        public decimal Maximo { get; set; }
    }
}
