namespace Galileo.Models.PRES
{
    public class PresUsuariosData
    {
        public string Usuario { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class PresContabilidadesData
    {
        public int Cod_Contabilidad { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class PresUnidadesData
    {
        public int Cod_Contabilidad { get; set; }
        public string Cod_Unidad { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class PresUsuariosInsert
    {
        public string Usuario { get; set; } = string.Empty;
        public string UserReg { get; set; } = string.Empty;
        public bool? Activo { get; set; }
    }

    public class PresUnidadesInsert
    {
        public string Usuario { get; set; } = string.Empty;
        public int? Cod_Contabilidad { get; set; }
        public string Cod_Unidad { get; set; } = string.Empty;
        public string UserReg { get; set; } = string.Empty;
        public bool? Activo { get; set; }
    }
}