namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrGeneraGarantiaOperacionRequest
    {
        public required long operacion { get; set; }
        public bool reemplazar_informacion { get; set; }
        public bool usar_cedula_real { get; set; }
        public string lugar_firma { get; set; } = string.Empty;
        public bool imprimir_contrato { get; set; }
        public bool imprimir_nombres_cedula { get; set; }
    }

    public class CrGeneraGarantiaRangoRequest
    {
        public required long inicio { get; set; }
        public required long corte { get; set; }
    }

    public class CrGeneraGarantiaPagareDto
    {
        public string cedula { get; set; } = string.Empty;
        public string secciones { get; set; } = string.Empty;
    }

    public class CrGeneraGarantiaContratoDto
    {
        public string cedula { get; set; } = string.Empty;
    }

    public class CrGeneraGarantiaEmailDto
    {
        public string email { get; set; } = string.Empty;
    }

    public class CrGeneraGarantiaLetraDto
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public decimal montoapr { get; set; }
        public decimal montosol { get; set; }
        public string estadosol { get; set; } = string.Empty;
        public string lugar_fecha { get; set; } = string.Empty;
        public string monto_letras { get; set; } = string.Empty;
        public string monto { get; set; } = string.Empty;
    }

    public class CrGeneraGarantiaPreImpresoDto
    {
        public string formula01 { get; set; } = string.Empty;
        public string formula02 { get; set; } = string.Empty;
        public string monto_letras { get; set; } = string.Empty;
        public string prometo { get; set; } = string.Empty;
        public decimal mora { get; set; }
        public string fiadores { get; set; } = "N";
    }

    internal class CrGeneraGarantiaOperacionData
    {
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string estadosol { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public decimal montoapr { get; set; }
        public int plazo { get; set; }
        public decimal int_corriente { get; set; }
    }

    internal class CrGeneraGarantiaSocioData
    {
        public string nombre { get; set; } = string.Empty;
        public string estado_civil { get; set; } = string.Empty;
        public string sexo { get; set; } = string.Empty;
        public string cedular { get; set; } = string.Empty;
        public string provincia_desc { get; set; } = string.Empty;
        public string canton_desc { get; set; } = string.Empty;
        public string distrito_desc { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
    }
}
