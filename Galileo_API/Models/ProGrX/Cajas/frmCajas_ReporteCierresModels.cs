namespace Galileo_API.Models.ProGrX.Cajas
{

    public class CajasAperturaReporteDto
    {
        public int CodApertura { get; set; }

        public DateTime Apertura_fecha { get; set; }
        public string Apertura_Usuario { get; set; }

        public string Estado { get; set; }

        public DateTime? Cierre_Fecha { get; set; }
        public string? Cierre_Usuario { get; set; }

        public DateTime? Recibe_Fecha { get; set; }
        public string? Recibe_Usuario { get; set; }

        public DateTime? Revisa_Fecha { get; set; }
        public string? Revisa_Usuario { get; set; }
    }

    public class CajasAccesoDto
    {
        public DateTime Fecha { get; set; }

        public string Caja { get; set; }
        public int Apertura { get; set; }

        public string Usuario { get; set; }
        public string Version { get; set; }
    }

    public class CajasDepositoDto
    {
        public string Dp_Numero { get; set; }

        public decimal Monto { get; set; }

        public string Estado { get; set; }

        public string Dp_Cuenta { get; set; }
        public string BancoDesc { get; set; }
    }


}

