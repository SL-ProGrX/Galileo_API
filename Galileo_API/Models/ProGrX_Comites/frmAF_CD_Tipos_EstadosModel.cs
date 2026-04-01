 
namespace Galileo_API.Models.ProGrX_Comites
{
    public class FrmAfCdTiposEstados
    {


        public class CdTiposEstadosLista
        {
            public int? Total { get; set; }
            public List<CdTiposEstadosData> lista { get; set; } = new List<CdTiposEstadosData>();
        }
        public class CdTiposEstadosData
        {
            public string? CodEstado { get; set; } = string.Empty;
            public string? NombreEstado { get; set; } = string.Empty; 
            public bool? Activo { get; set; }
            public bool? IsNew { get; set; }
        }

    }
}
