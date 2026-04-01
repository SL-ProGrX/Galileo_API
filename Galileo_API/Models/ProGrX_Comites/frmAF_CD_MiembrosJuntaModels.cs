namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdDirectorDto
    {
        public int Cod_Director { get; set; }
        public string? Nombre { get; set; }
        public string? Puesto { get; set; }
        public bool Activo { get; set; }
    }

    public class AfCdComiteDirectorDto
    {
        public int Cod_Comite { get; set; }
        public string? Descripcion { get; set; }
        public int Cod_Director { get; set; }
    }

    public class AfCdDirectorSaveDto : AfCdDirectorDto
    {      
        public string Usuario { get; set; } = string.Empty;
        public string? Cedula { get; set; } 
    }
}
