namespace PgxAPI.Models.AF
{

    public class ExcelRoot
    {
        public bool Closed { get; set; }
        public List<object> CurrentObservers { get; set; } = new List<object>();
        public List<object> Observers { get; set; } = new List<object>();
        public bool IsStopped { get; set; }
        public bool HasError { get; set; }
        public object ThrownError { get; set; } = new object();
        public List<BeneficioExcelData> _Value { get; set; } = new List<BeneficioExcelData>();
    }

    public class BeneficioExcelData
    {
        public int? linea_id { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public float monto { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string? beneficiario_id { get; set; }
        public string? beneficiario_nombre { get; set; }
        public string? beneficiario_iban { get; set; }
        public int inicializa { get; set; }
        public int? procesado { get; set; }
        public string? revision { get; set; }
    }

    public class BeneficioLoteRevisaData
    {
        public string cod_beneficio { get; set; } = string.Empty;
        public string @Usuario { get; set; } = string.Empty;
    }

    public class AfiBeneCargaLoteData
    {
        public int linea_id { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string beneficiario_id { get; set; } = string.Empty;
        public string beneficiario_nombre { get; set; } = string.Empty;
        public string beneficiario_iban { get; set; } = string.Empty;
        public int procesado { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string revision { get; set; } = string.Empty;
    }
}
