namespace PgxAPI.Models.AF
{
    public class AfBeneficiosBancosDataLista
    {
        public int Total { get; set; }
        public List<AfBeneficiosBancosData> bancosX { get; set; } = new List<AfBeneficiosBancosData>();
    }

    public class AfBeneficiosBancosData
    {
        public string id_banco { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool cheque { get; set; }
        public bool transferencia { get; set; }
    }

    public class AfBeneficioBancosfiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }
}