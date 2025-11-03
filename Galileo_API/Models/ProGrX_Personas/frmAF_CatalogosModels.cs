namespace PgxAPI.Models.ProGrX_Personas
{
    public class CatalogoData
    {
        public int Linea_Id { get; set; }
        public string Catalogo_Id { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = false;
        public int Tipo_Id { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Modifica_Fecha { get; set; }
        public string? Modifica_Usuario { get; set; }
    }

    public class CatalogoLista
    {
        public int Total { get; set; }
        public List<CatalogoData> Lista { get; set; } = new List<CatalogoData>();
    }

    public class CatalogoValidate
    {
        public int Existe { get; set; }
        public int Linea_Id { get; set; }
    }

    public class CatalogoTipoData
    {
        public int Tipo_Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }
}
