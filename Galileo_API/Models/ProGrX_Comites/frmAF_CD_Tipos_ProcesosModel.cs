 
namespace Galileo_API.Models.ProGrX_Comites
{
    public class FrmAfCdTiposProcesos
    {


        public class CdTiposProcesosLista
        {
            public int? Total { get; set; }
            public List<CdTiposProcesosData> lista { get; set; } = new List<CdTiposProcesosData>();
        }
        public class CdTiposProcesosData
        {
            public string? CodTipoProceso { get; set; } = string.Empty;
            public string? NombreTipoProceso { get; set; } = string.Empty; 
            public bool? Activo { get; set; }
            public bool? IsNew { get; set; }
        }

    }
}
