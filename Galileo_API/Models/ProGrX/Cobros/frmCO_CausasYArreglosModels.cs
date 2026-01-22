namespace Galileo.Models.ProGrX.Cobros
{
    public class COCausaMorosidadData
    {
        public string cod_causa { get; set; } = "";
        public string descripcion { get; set; } = "";
        public bool activa { get; set; } = true;
        public bool isNew { get; set; } = false;
    }

    public class COArregloPagoTipoData
    {
        public string cod_arreglo { get; set; } = "";
        public string descripcion { get; set; } = "";
        public bool activo { get; set; } = true;
        public bool isNew { get; set; } = false;
    }

    public class COCausaMorosidadListaResult
    {
        public int total { get; set; } = 0;
        public List<COCausaMorosidadData> lista { get; set; } = new();
    }

    public class COArregloPagoTipoListaResult
    {
        public int total { get; set; } = 0;
        public List<COArregloPagoTipoData> lista { get; set; } = new();
    }
}
