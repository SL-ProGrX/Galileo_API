
namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrNivelesGrupoLista
    {
        public int total { get; set; }
        public List<CrNivelesGrupoDto> lista { get; set; } = new();
    }

    public class CrNivelesMiembroLista
    {
        public int total { get; set; }
        public List<CrNivelesMiembroDto> lista { get; set; } = new();
    }

    public class CrNivelesLineaLista
    {
        public int total { get; set; }
        public List<CrNivelesLineaDto> lista { get; set; } = new();
    }

    public class CrNivelesGrupoDto
    {
        public int nv_cod_grupo { get; set; }
        public string nv_descripcion { get; set; } = string.Empty;
        public string nv_tipo { get; set; } = string.Empty;
        public decimal nv_desde { get; set; }
        public decimal nv_hasta { get; set; }
        public bool isNew { get; set; }
    }

    public class CrNivelesGrupoDetalleDto
    {
        public CrNivelesGrupoDto grupo { get; set; } = new();
        public CrNivelesMiembroLista miembros { get; set; } = new();
        public CrNivelesLineaLista lineas { get; set; } = new();
    }

    public class CrNivelesMiembroDto
    {
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CrNivelesLineaDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool asignado { get; set; }
    }

    public class CrNivelesGrupoGuardarRequest
    {
        public int? nv_cod_grupo { get; set; }
        public string nv_descripcion { get; set; } = string.Empty;
        public string nv_tipo { get; set; } = string.Empty;
        public decimal? nv_desde { get; set; }
        public decimal? nv_hasta { get; set; }
    }

    public class CrNivelesAsignacionMiembroRequest
    {
        public int? nv_cod_grupo { get; set; }
        public string nombre { get; set; } = string.Empty;
        public bool? asignado { get; set; }
    }

    public class CrNivelesAsignacionLineaRequest
    {
        public int? nv_cod_grupo { get; set; }
        public string codigo { get; set; } = string.Empty;
        public bool? asignado { get; set; }
    }

    public static class CrNivelesConstantes
    {
        public const string TipoProcesoInvalido = "Debe indicar un proceso válido.";
        public const string GrupoRequerido = "Debe seleccionar un grupo.";
        public const string GrupoDescripcionRequerida = "Falta El Nombre Del Grupo.";
        public const string RangoInvalido = "Debe indicar rangos válidos.";
        public const string ScrollValido = "El tipo de navegación debe ser 0: siguiente o 1: anterior.";
        public const string NoHayMasGrupos = "No hay más grupos para navegar.";
        public const string MiembroRequerido = "Debe indicar el miembro.";
        public const string LineaRequerida = "Debe indicar la línea.";
    }
}