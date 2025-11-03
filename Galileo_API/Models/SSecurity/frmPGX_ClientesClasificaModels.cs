namespace PgxAPI.Models
{
    public class Cliente_Clasifica
    {
        public string Cod_Clasificacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
    }

    public class Cliente_Selecciona
    {
        public int Cod_Empresa { get; set; }
        public string Nombre_Largo { get; set; } = string.Empty;
        public string Nombre_Corto { get; set; } = string.Empty;
    }

    public class ErrorCliente_ClasificaDTO
    {
        public int Code { get; set; }
        public string Description { get; set; } = string.Empty;
    }

}
