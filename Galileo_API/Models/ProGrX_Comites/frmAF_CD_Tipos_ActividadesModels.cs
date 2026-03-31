namespace Galileo_API.Models.ProGrX_Comites
{
    public class FrmAfCdTiposActividades
    {


        public class CDTiposActividadesLista
        {
            public int? total { get; set; }
            public List<CDTiposActividadesData> lista { get; set; } = new List<CDTiposActividadesData>();
        }
        public class CDTiposActividadesData
        {
            public string? CodTipoActividad { get; set; } = string.Empty;
            public string? NombreTipoActividad { get; set; } = string.Empty; 
            public bool? Activo { get; set; }
            public bool? IsNew { get; set; }
        }

    }
}
