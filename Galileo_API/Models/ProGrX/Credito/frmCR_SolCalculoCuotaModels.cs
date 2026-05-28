using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrSolCalculoCuotaPantallaData
    {
        public DateTime? fecha_servidor { get; set; }
        public string factor_default { get; set; } = "01";
        public List<DropDownListaGenericaModel> factores { get; set; } = new();
    }

    public class CrSolCalculoCuotaFactorRequest
    {
        public string dato { get; set; } = string.Empty;
    }

    public class CrSolCalculoCuotaFactorData
    {
        public string valor { get; set; } = string.Empty;
    }

    public class CrSolCalculoCuotaCalcularCuotaRequest
    {
        public decimal monto { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal interes { get; set; } = 0;
        public string frecuencia { get; set; } = "M";
    }

    public class CrSolCalculoCuotaCalcularCuotaData
    {
        public decimal cuota { get; set; } = 0;
    }

    public class CrSolCalculoCuotaNiveladaRequest
    {
        public decimal saldo { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal tasa { get; set; } = 0;
        public DateTime? fecha_inicio { get; set; }
    }

    public class CrSolCalculoCuotaNiveladaData
    {
        public decimal cuota { get; set; } = 0;
    }

    public class CrSolCalculoCuotaDiasMesRequest
    {
        public int mes { get; set; } = 0;
        public int anio { get; set; } = 0;
    }

    public class CrSolCalculoCuotaDiasMesData
    {
        public int dias { get; set; } = 0;
    }
}