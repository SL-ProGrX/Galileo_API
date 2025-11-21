namespace Galileo.Models.Security
{
    public class ClienteClasifica
    {
        public string Cod_Clasificacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
    }

    public class ClienteSelecciona
    {
        public int Cod_Empresa { get; set; }
        public string Nombre_Largo { get; set; } = string.Empty;
        public string Nombre_Corto { get; set; } = string.Empty;
    }
}
