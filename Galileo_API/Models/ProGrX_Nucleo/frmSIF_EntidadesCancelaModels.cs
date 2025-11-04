namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class SifEntidadesCancelaLista
    {
        public int total { get; set; }
        public List<SifEntidadesCancelaData> lista { get; set; } = new List<SifEntidadesCancelaData>();
    }
    public class SifEntidadesCancelaData
    {
        public string cod_entidad_pago { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; } = false;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public bool isNew { get; set; } = false;
    }
}
