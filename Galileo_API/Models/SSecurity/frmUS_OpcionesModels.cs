namespace PgxAPI.Models
{
    public class OpcionDTO
    {
        public int Cod_Opcion { get; set; }
        public string Formulario { get; set; } = string.Empty;
        public int Modulo { get; set; }
        public string Opcion { get; set; } = string.Empty;
        public string Opcion_Descripcion { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;

    }



    public class FormularioDTO
    {
        public string Formulario { get; set; } = string.Empty;
        public int Modulo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class OpcionesRequest
    {
        public int modulo { get; set; }
        public string formulario { get; set; } = string.Empty;

    }

}
