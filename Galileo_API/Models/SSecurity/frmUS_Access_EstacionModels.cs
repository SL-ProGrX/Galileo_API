namespace PgxAPI.Models.US
{
    public class EstacionDto
    {
        //public int CodEmpresa { get; set; }
        public string Estacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string MAC1 { get; set; } = string.Empty;
        public string MAC2 { get; set; } = string.Empty;
        public List<string> lstMAC1 { get; set; } = new List<string>();
        public List<string> lstMAC2 { get; set; } = new List<string>();
    }

    public class EstacionGuardarDto
    {
        public int CodEmpresa { get; set; }
        public string Estacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public int Modulo { get; set; } = 0;
        public string MAC1 { get; set; } = string.Empty;
        public string MAC2 { get; set; } = string.Empty;
        public string AppEquipo { get; set; } = string.Empty;
        public string AppVersion { get; set; } = string.Empty;
        public string AppIP { get; set; } = string.Empty;
    }

    public class EstacionEliminarDto
    {
        public int CodEmpresa { get; set; }
        public string Estacion { get; set; } = string.Empty;
    }

    public class EstacionMACDto
    {
        public string MAC_01 { get; set; } = string.Empty;
        public string MAC_02 { get; set; } = string.Empty;
    }

    public class EstacionSinVincularDto
    {
        public string Estacion { get; set; } = string.Empty;
        public string MAX { get; set; } = string.Empty;
    }

    public class EstacionVinculaDto
    {
        public int Cliente { get; set; }
        public string Estacion { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int Vincula { get; set; }
    }
}
