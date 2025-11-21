namespace Galileo.Models.FSL
{
    public class FslRequisitosData
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string registro_usuario { get; set; } = string.Empty;

    }

    public class FslRequisitosDataLista
    {
        public int Total { get; set; }
        public List<FslRequisitosData> requisitos { get; set; } = new List<FslRequisitosData>();
    }

    public class FslPanesCausasLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FslRequisitoCausa
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool opcional { get; set; }
        public bool asignado { get; set; }
        public int? cod_causa { get; set; }
        public int? cod_plan { get; set; }

    }

    public class FslPlanes
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FslRequisitoEditar
    {
        public string cod_plan { get; set; } = string.Empty;
        public string cod_causa { get; set; } = string.Empty;
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool opcional { get; set; }
        public bool asignado { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FslRequisitosFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }

        public string? comiteSeleccionado { get; set; }
    }
}