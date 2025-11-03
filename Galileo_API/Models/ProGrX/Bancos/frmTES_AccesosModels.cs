namespace PgxAPI.Models.ProGrX.Bancos
{
    public class tesAccesosUsuariosData
    {
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public int id_banco { get; set; } = 0;
    }

    public class tesAccesosBancosData
    {
        public int id_banco { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string cta { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class tesAccesosUsuariosLista
    {
        public int total { get; set; } = 0;
        public List<DropDownListaGenericaModel> lista { get; set; } = new List<DropDownListaGenericaModel>();
    }

    public class tesAccesosDocumentosData
    {
        public string tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool solicita { get; set; } = false;
        public bool autoriza { get; set; } = false;
        public bool genera { get; set; } = false;
        public bool asientos { get; set; } = false;
        public bool anula { get; set; } = false;
    }

    public class tesAccesosConceptosData
    {
        public string cod_concepto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int id_banco { get; set; } = 0;
    }

    public class tesAccesosUnidadesData
    {
        public string cod_unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int id_banco { get; set; } = 0;
    }

    public class tesAccesosFirmasData
    {
        public int id_banco { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public bool utiliza_firmas_autoriza { get; set; } = false;
        public bool aplica_rango_autorizacion { get; set; } = false;
        public float firmas_autoriza_inicio { get; set; }
        public float firmas_autoriza_corte { get; set; } 

    }
}
