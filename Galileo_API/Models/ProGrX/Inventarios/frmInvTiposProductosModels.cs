namespace PgxAPI.Models.INV
{
    public class TipoProductoDto
    {
        public int Cod_Prodclas { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Alter { get; set; } = string.Empty;
        public string Costeo { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Valuacion { get; set; } = string.Empty;
        public string Cta_Desc { get; set; } = string.Empty;
        public int Cantidad_Sub { get; set; }
    }

    public class TipoProductoDataLista
    {
        public int Total { get; set; }
        public List<TipoProductoDto> Lista { get; set; } = new List<TipoProductoDto>();
    }

    public class TipoProductoSubGradaData
    {
        public string key { get; set; } = string.Empty;
        public string icon { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public TipoProductoSubDto? data { get; set; }
        public List<TipoProductoSubGradaData>? children { get; set; }
    }

    public class TipoProductoSubDto
    {
        public int Cod_Prodclas { get; set; }
        public string Cod_Linea_Sub { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public string Cabys { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public int Nivel { get; set; } = 0;
        public string Cod_Linea_Sub_Madre { get; set; } = string.Empty;
        public string Niveles { get; set; } = string.Empty; 
    }

    public class TipoProductoSubDataLista
    {
        public int Total { get; set; }
        public List<TipoProductoSubDto> Lista { get; set; } = new List<TipoProductoSubDto>();
    }

    public class InvCabys
    {
        public string Cod_ByS { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}