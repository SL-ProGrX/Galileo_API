namespace Galileo.Models.AF
{
    public class AfBeneficiosBancosDataLista
    {
        public int total { get; set; }
        public List<AfBeneficiosBancosData> bancosX { get; set; } = new List<AfBeneficiosBancosData>();
    }

    public class AfBeneficiosBancosData
    {
        public string id_banco { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public bool cheque { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool transferencia { get; set; }
    }
}
