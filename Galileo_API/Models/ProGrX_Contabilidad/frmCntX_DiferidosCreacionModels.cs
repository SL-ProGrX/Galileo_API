namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXDiferidosCreacionRequest
    {
        public int cod_contabilidad { get; set; }
        public bool edita { get; set; }
        public string? usuario { get; set; }
        public CntXDiferidoCreacionData? data { get; set; }
    }

    public class CntXDiferidoCreacionData
    {
        public int cod_DifPlantilla { get; set; } = 0;
        public int cod_diferido { get; set; } = 0;
        public int cod_contabilidad { get; set; } = 0;
        public string tipo_asiento { get; set; } = string.Empty;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string? descripcion { get; set; }
        public string? detalle { get; set; }
        public string? documento { get; set; }
        public decimal monto_diferir { get; set; } = 0;
        public decimal acumulado { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public DateTime? fecha_crea { get; set; }
        public string? user_crea { get; set; }
        public DateTime? fecha_procesa { get; set; }
        public string? user_procesa { get; set; }
        public int consecutivo { get; set; } = 0;
        public string? estado { get; set; }
        public string? descPlantilla { get; set; }
        public string? tipo { get; set; }
    }

    public class CntXDiferidoHistoricoData
    {
        public string? num_asiento { get; set; }
        public string? tipo_asiento { get; set; }
        public DateTime? fecha { get; set; }
        public int anio { get; set; }
        public int mes { get; set; }
    }

    public class CntXDiferidosPlantillaData
    {
        public int cod_difplantilla { get; set; } = 0;
        public int cod_diferido { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
    }
}
