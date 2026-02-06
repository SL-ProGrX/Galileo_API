namespace Galileo_API.Models.ProGrX_Polizas
{
    public class SiniestroTipoDto
    {
        public int Id_Siniestro { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class SiniestroTipoExisteResult
    {
        public int Existe { get; set; }
    }

    public class SiniestroTipoSaveParams
    {
        public int Id_Siniestro { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool? Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class SiniestroTipoDeleteParams
    {
        public int Id_Siniestro { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
