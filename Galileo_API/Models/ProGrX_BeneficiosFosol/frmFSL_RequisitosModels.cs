namespace PgxAPI.Models.FSL
{
    public class fslRequisitosData
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string registro_usuario { get; set; } = string.Empty;

    }

    public class fslRequisitosDataLista
    {
        public int Total { get; set; }
        public List<fslRequisitosData> requisitos { get; set; } = new List<fslRequisitosData>();
    }

    public class fslPanesCausasLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class fslRequisitoCausa
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool opcional { get; set; }
        public bool asignado { get; set; }
        public int? cod_causa { get; set; }
        public int? cod_plan { get; set; }

    }

    public class fslPlanes
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class fslRequisitoEditar
    {
        public string cod_plan { get; set; } = string.Empty;
        public string cod_causa { get; set; } = string.Empty;
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool opcional { get; set; }
        public bool asignado { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }


    public class fslRequisitosFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }

        public string? comiteSeleccionado { get; set; }
    }
}
