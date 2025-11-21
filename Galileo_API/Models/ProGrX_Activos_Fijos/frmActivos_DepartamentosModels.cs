namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosDepartamentosLista
    {
        public int total { get; set; } = 0;
        public List<ActivosDepartamentosData> lista { get; set; } = new();
    }

    public class ActivosDepartamentosData
    {
        public string cod_departamento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string unidad_desc { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;

        public bool isNew { get; set; } = false;
    }
    
    public class ActivosSeccionesLista
    {
        public int total { get; set; } = 0;
        public List<ActivosSeccionesData> lista { get; set; } = new();
    }

    public class ActivosSeccionesData
    {
        public string cod_departamento { get; set; } = string.Empty;

        public string departamento_desc { get; set; } = string.Empty;
        public string cod_seccion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string centro_costo_desc { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool isNew { get; set; } = false;
    }
}