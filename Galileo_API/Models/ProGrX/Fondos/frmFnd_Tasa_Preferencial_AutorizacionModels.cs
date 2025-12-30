using System.ComponentModel.DataAnnotations;

namespace Galileo.Models.ProGrX.Fondos
{
    public class FndTasaPrefFiltros
    {
        public string usuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; } = null;
        public DateTime? fecha_corte { get; set; } = null;
        public string logUsuario { get; set; } = string.Empty;

    }

    public class FndTPListDto
    {
        public int id_tp { get; set; } = 0;
        public int cod_operadora { get; set; } = 0;
        public string cod_plan { get; set; } = string.Empty;
        public string cod_contrato { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public decimal? monto_inversion { get; set; }
        public int? plazo_dias { get; set; }
        public decimal? tasa_calculada { get; set; }
        public decimal? tasa_solicitada { get; set; }
        public decimal? margen_solicitado { get; set; }
        public decimal? margen_maximo { get; set; }
        public string observacion { get; set; } = string.Empty;
        public string cupon_frecuencia { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string resuelve_usuario { get; set; } = string.Empty;
        public DateTime? resuelve_fecha { get; set; }
        public string aplica_usuario { get; set; } = string.Empty;
        public DateTime? aplica_fecha { get; set; }
        public string plan_desc { get; set; } = string.Empty;
    }

}
