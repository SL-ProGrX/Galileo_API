namespace Galileo.Models.ProGrX.Fondos
{
    public class FndSegNivelesGrupoDto
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal? monto_inicio { get; set; }
        public decimal? monto_corte { get; set; }
        public bool? activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FndSegNivelesPlanesData
    {
        public string cod_operadora { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FndSegNivelesUsuariosData
    {
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FndSegNivelesPlanesDto
    {
        public string cod_operadora { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public bool asignar { get; set; } = false;
    }

    public class FndSegNivelesUsuariosDto
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public bool asignar { get; set; } = false;
    }
}
