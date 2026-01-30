namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CbrUsuarioResult
    {
        public string? Usuario { get; set; }
    }

    public class CbrUsuarioGrupoListParams
    {
        public string Usuario { get; set; } = string.Empty;
        public string Filtro { get; set; } = string.Empty;
    }

    public class CbrUsuarioGrupoListResult
    {
        public int Id_Grupo { get; set; }
        public string? Descripcion { get; set; }
        public int Asignado { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
    }

    public class CbrControlDistribucionParams
    {
        public string Tipo { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public int? Inicializa { get; set; }
        public int? MantenerNuevos { get; set; }
        public int? CasosMorosos { get; set; }
        public int? CasosAlDia { get; set; }
    }

    public class CbrControlDistribucionResult
    {
        public int? Pass { get; set; }
        public string? Mensaje { get; set; }
    }
}
