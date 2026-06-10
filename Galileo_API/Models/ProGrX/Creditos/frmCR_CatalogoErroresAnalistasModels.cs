namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrCatalogoErroresAnalistasModel
    {
        public int Id_Error { get; set; } = 0;
        public string Descripcion { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Activo { get; set; } = string.Empty;
    }

    public class CrCatalogoErroresAnalistasGuardarRequest
    {
        public int Id_Error { get; set; } = 0;
        public string Descripcion { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Activo { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CrCatalogoErroresAnalistasEliminarRequest
    {
        public int Id_Error { get; set; } = 0;
        public string Usuario { get; set; } = string.Empty;
    }
}
