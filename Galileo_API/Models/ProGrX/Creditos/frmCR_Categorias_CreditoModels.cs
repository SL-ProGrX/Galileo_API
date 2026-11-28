namespace Galileo_API.Models.ProGrX.Creditos
{
    public abstract class
        CrCategoriasCreditoData
    {
        public string usuario_registra { get; set; } = string.Empty;

        public DateTime? fec_registra { get; set; }

        public string usuario_modifica { get; set; } = string.Empty;

        public DateTime? fec_modifica { get; set; }
    }

    public class
        CrCategoriasCreditoProbabilidadDefaultData
        : CrCategoriasCreditoData
    {
        public int id_probabilidad_def { get; set; } = 0;

        public string descripcion { get; set; } = string.Empty;

        public string categoria { get; set; } = string.Empty;

        public decimal valor_inicial { get; set; } = 0;

        public decimal valor_final { get; set; } = 0;
    }

    public class
        CrCategoriasCreditoProbabilidadMoraData
        : CrCategoriasCreditoData
    {
        public int id_probabilidad_mora { get; set; } = 0;

        public string descripcion { get; set; } = string.Empty;

        public string tipo_mora { get; set; } = string.Empty;

        public decimal porc_probabilidad { get; set; } = 0;
    }

    public class
        CrCategoriasCreditoSegmentoData
        : CrCategoriasCreditoData
    {
        public int id_segmento { get; set; } = 0;

        public string cod_segmento { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;

        public decimal porc_segmento { get; set; } = 0;
    }

    public class
        CrCategoriasCreditoCargaProbabilidadData
    {
        public string segmento { get; set; } = string.Empty;

        public int mes { get; set; }

        public decimal probabilidad { get; set; }

        public DateTime? fec_registra { get; set; }

        public string usuario_registra { get; set; } = string.Empty;
    }

    public class
        CrCategoriasCreditoCargaProbabilidadRequest
    {
        public string usuario { get; set; } = string.Empty;

        public string nombre_archivo { get; set; } = string.Empty;

        public List<CrCategoriasCreditoCargaProbabilidadData> registros { get; set; } = [];
    }

    public class CrCategoriasCreditoCrudRequest
    {
        public int id { get; set; }

        public bool es_nuevo { get; set; }

        public string descripcion { get; set; } = string.Empty;

        public string categoria { get; set; } = string.Empty;

        public decimal valor_inicial { get; set; }

        public decimal valor_final { get; set; }

        public decimal porc_probabilidad { get; set; }

        public string cod_segmento { get; set; } = string.Empty;

        public decimal porc_segmento { get; set; }
    }
}
