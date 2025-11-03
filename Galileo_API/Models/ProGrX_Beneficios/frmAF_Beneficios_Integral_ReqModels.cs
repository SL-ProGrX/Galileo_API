namespace PgxAPI.Models.AF
{
    public class BENE_REG_REQUISITO
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public bool asigna { get; set; }

    }

    public class BeneRequisitosGuardar
    {
        public int codCliente { get; set; }
        public int consec { get; set; }
        public string cod_requisito { get; set; } = string.Empty;
        public string cod_beneficio { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class BENE_REG_REQUISITOLista
    {
        public int total { get; set; }
        public List<BENE_REG_REQUISITO> lista { get; set; } = new List<BENE_REG_REQUISITO>();
    }
}
