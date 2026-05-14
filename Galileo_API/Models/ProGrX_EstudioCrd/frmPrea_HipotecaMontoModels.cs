namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaHipotecaMontoListaResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public List<FrmPreaHipotecaMontoItem> lista { get; set; } = new();
    }

    public class FrmPreaHipotecaMontoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public int id_param { get; set; } = 0;
    }

    public class FrmPreaHipotecaMontoGuardarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public int id_param { get; set; } = 0;
        public string mensaje { get; set; } = string.Empty;
    }

    public class FrmPreaHipotecaMontoItem
    {
        public int id_param { get; set; } = 0;
        public int asigna { get; set; } = 0;
        public decimal monto_min { get; set; } = 0;
        public decimal monto_max { get; set; } = 0;
        public decimal gastos { get; set; } = 0;
        public decimal honorarios { get; set; } = 0;
        public decimal imp_traspaso { get; set; } = 0;
    }

    internal class FrmPreaHipotecaMontoItemData
    {
        public int id_param { get; set; } = 0;
        public int asigna { get; set; } = 0;
        public decimal monto_min { get; set; } = 0;
        public decimal monto_max { get; set; } = 0;
        public decimal gastos { get; set; } = 0;
        public decimal honorarios { get; set; } = 0;
        public decimal imp_traspaso { get; set; } = 0;
    }
}
