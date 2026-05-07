namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class ParametrosCodigoData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool @checked { get; set; } = false;
    }

    public class ParametrosMembresiaData
    {
        public int? desde { get; set; } = 0;
        public int? hasta { get; set; } = 0;
        public decimal? monto { get; set; } = 0;
    }

    public class ParametrosOtrosData
    {
        public int meses_transcurridos { get; set; } = 0;
        public int porc_fiduciarios { get; set; } = 0;
        public int porc_cancelado { get; set; } = 0;
        public bool activar_sgt { get; set; } = false;
    }

    public class ParametrosCodigoActualizarRequest
    {
        public string garantia { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public bool @checked { get; set; } = false;
    }

    public class ParametrosMembresiasGuardarRequest
    {
        public string garantia { get; set; } = string.Empty;
        public List<ParametrosMembresiaData> membresias { get; set; } = new();
    }

    public class ParametrosOtrosGuardarRequest
    {
        public int meses_transcurridos { get; set; } = 0;
        public int porc_fiduciarios { get; set; } = 0;
        public int porc_cancelado { get; set; } = 0;
        public bool activar_sgt { get; set; } = false;
    }
}