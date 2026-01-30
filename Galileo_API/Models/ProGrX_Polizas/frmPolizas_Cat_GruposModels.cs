using System.ComponentModel.DataAnnotations;

namespace Galileo_API.Models.ProGrX_Polizas
{
    public class PolizaGrupoDto
    {
        public int Id_Poliza_Grupo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo_Aplicacion { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class PolizaGrupoExisteResult
    {
        public int Existe { get; set; }
    }

    public class PolizaGrupoSaveParams
    {
        [Required]
        public int Id_Poliza_Grupo { get; set; } 
        public string Descripcion { get; set; } = string.Empty;
        public string Tipo_Aplicacion { get; set; } = string.Empty;
        public bool? Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class PolizaGrupoDeleteParams
    {
        [Required] 
        public int Id_Poliza_Grupo { get; set; }  
        public string Usuario { get; set; } = string.Empty;
    }
}
