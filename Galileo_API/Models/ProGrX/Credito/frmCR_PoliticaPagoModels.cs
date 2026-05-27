namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrPoliticaPagoData
    {
        public int id_politica { get; set; }
        public int dia_inicio { get; set; } = 1;
        public int dia_corte { get; set; } = 1;
        public string politica { get; set; } = "ULT";
        public string politica_desc { get; set; } = "Ultimo dia del Mes";
        public int dia_base { get; set; } = 32;
    }

    public class CrPoliticaPagoTrasladoData
    {
        public int id_seq { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string tipo_desc { get; set; } = string.Empty;
        public int? dia_semana { get; set; }
        public string dia_semana_desc { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrPoliticaPagoTrasladoGuardarRequest
    {
        public string tipo { get; set; } = string.Empty;
        public int? dia_semana { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
    }
}
