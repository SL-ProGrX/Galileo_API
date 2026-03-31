 
namespace Galileo_API.Models.ProGrX_Comites
{
    public class FrmAfCdTiposAprobaciones
    {


        public class CdTiposAprobacionesLista
        {
            public int? Total { get; set; }
            public List<CdTiposAprobacionesData> lista { get; set; } = new List<CdTiposAprobacionesData>();
        }
        public class CdTiposAprobacionesData
        {
            public string? CodTipoAprobacion { get; set; } = string.Empty;
            public string? NombreTipoAprobacion { get; set; } = string.Empty; 
            public bool? Activo { get; set; }
            public bool? IsNew { get; set; }
        }

    }
}
