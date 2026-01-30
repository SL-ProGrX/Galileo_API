namespace Galileo_API.Models.ProGrX_Polizas
{

    public class EnfermedadVidaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

    public class EnfermedadVidaExisteResult
    {
        public int Existe { get; set; }
    }

    public class EnfermedadVidaSaveParams
    {
        public int? Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool? Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class EnfermedadVidaDeleteParams
    {
        public int? Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
