namespace Galileo_API.Models.ProGrX_Polizas
{
    public class IncendioCausaDto
    {
        public int ID { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class IncendioCausaSaveParams
    {
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class IncendioCausaUpdateParams
    {
        public int ID { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class IncendioCausaDeleteParams
    {
        public int ID { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
