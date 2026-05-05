namespace Galileo.Models.ProGrX.Cobros
{
    public class COCarteraClasificacionData
    {
        public string cod_clasificacion { get; set; } = "";
        public string descripcion { get; set; } = "";
        public bool estado { get; set; } = true;
        public bool isNew { get; set; } = false;
    }
    public class COCarteraCatalogoData
    {
        public string codigo { get; set; } = "";
        public string descripcion { get; set; } = "";
    }
    public class COCarteraAsignacionCatItemData
    {
        public string cod_clasificacion { get; set; } = "";
        public string descripcion { get; set; } = "";
        public bool asignado { get; set; } = false;
    }
    public class COCarteraAsignacionCodigoItemData
    {
        public string codigo { get; set; } = "";
        public string descripcion { get; set; } = "";
        public bool asignado { get; set; } = false;
    }
    public class COCarteraAsignacionGuardarDto
    {
        public string cod_clasificacion { get; set; } = "";
        public string codigo { get; set; } = "";
        public bool asignar { get; set; } = false;
    }
    public class COCarteraAsignacionBulkDto
    {
        public string cod_clasificacion { get; set; } = "";
        public bool asignar_todos { get; set; } = false;
    }
    public class COCarteraListaResult
    {
        public int total { get; set; } = 0;
        public List<COCarteraClasificacionData> lista { get; set; } = new();
    }
}