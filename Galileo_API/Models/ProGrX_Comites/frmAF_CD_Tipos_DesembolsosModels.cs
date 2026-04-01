 
namespace Galileo_API.Models.ProGrX_Comites
{
    public class FrmAfCdTiposDesembolsos
    {


        public class CdTiposDesembolsosLista
        {
            public int? Total { get; set; }
            public List<CdTiposDesembolsosData> lista { get; set; } = new List<CdTiposDesembolsosData>();
        }
        public class CdTiposDesembolsosData
        {
            public string? CodTipoCuenta { get; set; } = string.Empty;
            public string? NombreTipoCuenta { get; set; } = string.Empty; 
            public bool? Activo { get; set; }
            public bool? IsNew { get; set; }
        }

    }
}
