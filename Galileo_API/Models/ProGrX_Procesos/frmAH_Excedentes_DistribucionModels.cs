namespace Galileo.Models.ProGrX_Procesos
{

    public class AHExcMontoDto
    {
        // ================= CAMPOS EDITABLES =================
        public string periodo { get; set; } = "";
        public string corte { get; set; } = "";
        public string tipo { get; set; } = "";
        public string baseCalculo { get; set; } = "";

        public decimal porcentaje { get; set; }
        public decimal monto { get; set; }
        public string justificacion { get; set; } = "";

        // ================= CAMPOS DE LA LISTA (SP) =================

    }


    public class AHExcMontoListadoDto
    {
        public int id_periodo { get; set; }
        public string corte { get; set; } = "";
        public int anio { get; set; }
        public string mes { get; set; } = "";
        public decimal monto_proyectado { get; set; }
        public decimal monto_cargado { get; set; }
        public decimal monto_real { get; set; }
        public decimal monto_prorrateado { get; set; }
        public decimal porc_distribuido { get; set; }
        public string base_calculo { get; set; } = "";
        public decimal diferencia { get; set; }
        public string base_calculo_desc { get; set; } = "";
    }
}