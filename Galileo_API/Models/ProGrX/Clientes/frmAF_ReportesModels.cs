namespace Galileo.Models.ProGrX.Clientes
{
    public class AfReportesCombosDto
    {
        public List<DropDownListaGenericaModel> Provincias { get; set; } = new();
        public List<DropDownListaGenericaModel> GruposUsuarios { get; set; } = new();
        public List<DropDownListaGenericaModel> Instituciones { get; set; } = new();
        public List<DropDownListaGenericaModel> Promotores { get; set; } = new();
        public List<DropDownListaGenericaModel> EstadosPersona { get; set; } = new();
        public List<DropDownListaGenericaModel> EstadoCivil { get; set; } = new();
        public List<DropDownListaGenericaModel> Profesiones { get; set; } = new();
        public List<DropDownListaGenericaModel> Sectores { get; set; } = new();
        public List<DropDownListaGenericaModel> Zonas { get; set; } = new();
        public List<DropDownListaGenericaModel> TiposIdentificacion { get; set; } = new();
        public List<DropDownListaGenericaModel> EstadoLaboral { get; set; } = new();
    }

    public class AfGrupoMiembroDto
    {
        public required string nombre { get; set; }
        public required string descripcion { get; set; }
        public string? usuario { get; set; }
    }

    public class AfReporteDto
    {
        public required int id_rep { get; set; }
        public required string tipo { get; set; }
        public required string reporte { get; set; }
        public required string prefijo { get; set; }
        public required bool seguridad { get; set; }
    }

    public class AfSeguridadGrupoDto
    {
        public required int codgrupo { get; set; }
        public string? descripcion { get; set; }
        public required bool activo { get; set; }
    }

    public class AfSeguridadMiembroDto
    {
        public required string nombre { get; set; }
        public string? descripcion { get; set; }
        public string? usuario { get; set; } // null si no pertenece
    }

    public class AfSeguridadReporteDto
    {
        public required string tipo { get; set; }
        public int idrep { get; set; }
        public required string reporte { get; set; }
        public int? codgrupo { get; set; } // null si el reporte no está autorizado
    }

    public class AfGrupoConfiguracionDto
    {
        public required string cod_grupo { get; set; }
        public required string descripcion { get; set; }

    }
}