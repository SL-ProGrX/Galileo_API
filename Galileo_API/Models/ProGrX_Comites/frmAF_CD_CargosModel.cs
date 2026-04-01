 
namespace Galileo_API.Models.ProGrX_Comites
{
    public class FrmAfCdCargos
    {


        public class CdCargosLista
        {
            public int? Total { get; set; }
            public List<CdCargosData> lista { get; set; } = new List<CdCargosData>();
        }
        public class CdCargosData
        {
            public string? Codigo { get; set; } = string.Empty;
            public string? Descripcion { get; set; } = string.Empty;
            public string? Cuenta { get; set; } = string.Empty;
            public string? Cod_cuenta_mask { get; set; } = string.Empty;
            public bool? Estado { get; set; }
            public bool? IsNew { get; set; }
        }

    }
}
