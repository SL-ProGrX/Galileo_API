namespace Galileo_API.Models.ProGrX.Cajas
{

    public class CajasAperturaReporteDto
    {
        public int cod_apertura { get; set; }

        public DateTime apertura_fecha { get; set; }
        public string apertura_usuario { get; set; }

        public string estado { get; set; }

        public DateTime? cierre_fecha { get; set; }
        public string? cierre_usuario { get; set; }

        public DateTime? recibe_fecha { get; set; }
        public string? recibe_usuario { get; set; }

        public DateTime? revisa_fecha { get; set; }
        public string? revisa_usuario { get; set; }
    }

    public class CajasAccesoDto
    {
        public DateTime fecha { get; set; }

        public string caja { get; set; }
        public int apertura { get; set; }

        public string usuario { get; set; }
        public string version { get; set; }
    }

    public class CajasDepositoDto
    {
        public string dp_numero { get; set; }

        public decimal monto { get; set; }

        public string estado { get; set; }

        public string dp_cuenta { get; set; }
        public string bancodesc { get; set; }
    }


}

