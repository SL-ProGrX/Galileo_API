using System.ComponentModel.DataAnnotations;

namespace Galileo.Models.ProGrX.Fondos
{
    public class FndReportesGeneralesCuboFiltros
    {
        [Required]
        public bool chk_todos { get; set; }
        [Required]
        public DateTime fecha_inicio { get; set; }
        [Required]
        public DateTime fecha_corte { get; set; }
    }

    public class FndMovAnalisisCuboData
    {
        public string plan_codigo { get; set; } = string.Empty;
        public string plan_descripcion { get; set; } = string.Empty;
        public string institucion { get; set; } = string.Empty;
        public string fecha_movimiento { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string caja_codigo { get; set; } = string.Empty;
        public string tipo_comprobante { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public decimal monto { get; set; } 
    }

}
