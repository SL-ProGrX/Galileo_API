namespace Galileo.Models.GG_PE
{
    public class PeProyectosDto
    {
        public int proyecto_id { get; set; }
        public int programa_id { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string responsable { get; set; } = string.Empty;
        public float presupuesto { get; set; }
        public Nullable<DateTime> fecha_inicio { get; set; }
        public Nullable<DateTime> fecha_finaliza { get; set; }
        public bool activo { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }

    public class PeProyectosLista
    {
        public int total { get; set; }
        public List<PeProyectosDto>? proyectos { get; set; }
    }

    public class PeProyectosFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class PeProyectoObjetivosLista
    {
        public int objetivo_id { get; set; }
        public string objetivo { get; set; } = string.Empty;
        public string perspectiva { get; set; } = string.Empty;
        public string plan { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class PeProyectoObjetivosExportar
    {
        public int proyecto_id { get; set; }
        public string proyecto { get; set; } = string.Empty;
        public int programa_id { get; set; }
        public string programa { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public float presupuesto { get; set; }
        public Nullable<DateTime> fecha_inicio { get; set; }
        public Nullable<DateTime> fecha_finaliza { get; set; }
        public string? objetivo { get; set; }
        public string? descripcion_objetivo { get; set; }
        public bool activo { get; set; }
    }
}