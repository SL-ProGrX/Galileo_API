namespace Galileo.Models.ProGrX.Cobros
{
    public class COAplExcPrioridadData
    {
        public string codigo { get; set; } = "";
        public string descripcion { get; set; } = "";
        public int orden { get; set; } = 0;

        public bool contrato_apl { get; set; } = false;
        public bool activo { get; set; } = true;

        public string registro_fecha { get; set; } = "";
        public string registro_usuario { get; set; } = "";
        public string modifica_fecha { get; set; } = "";
        public string modifica_usuario { get; set; } = "";

        public bool isNew { get; set; } = false;
    }

    public class COAplExcPrioridadesListaResult
    {
        public int total { get; set; } = 0;
        public List<COAplExcPrioridadData> lista { get; set; } = new();
    }
}
