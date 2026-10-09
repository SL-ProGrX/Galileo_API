using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public sealed class CrConsultaPlanillaAbonoDistInicialData
    {
        public string cedula { get; set; } = string.Empty;

        public string nombre { get; set; } = string.Empty;

        public int cod_institucion { get; set; } = 0;

        public DateTime fecha { get; set; }

        public int proceso { get; set; } = 0;

        public List<DropDownListaGenericaModel> deductoras { get; set; } = [];
    }

    public sealed class CrConsultaPlanillaAbonoDistUltimoData
    {
        public decimal monto { get; set; } = 0;

        public int proceso { get; set; } = 0;
    }

    public sealed class CrConsultaPlanillaAbonoDistConsultaRequest
    {
        public string cedula { get; set; } = string.Empty;

        public int? cod_institucion { get; set; }

        public int? proceso { get; set; }

        public decimal? monto { get; set; }

        public DateTime? corte { get; set; }
    }

    public sealed class CrConsultaPlanillaAbonoDistDetalleData
    {
        public long operacion { get; set; } = 0;

        public string linea { get; set; } = string.Empty;

        public string tipo { get; set; } = string.Empty;

        public int proceso { get; set; } = 0;

        public decimal ab_int_cor { get; set; } = 0;

        public decimal ab_int_mor { get; set; } = 0;

        public decimal ab_cargo { get; set; } = 0;

        public decimal ab_amortiza { get; set; } = 0;

        public decimal int_cor { get; set; } = 0;

        public decimal int_mor { get; set; } = 0;

        public decimal cargo { get; set; } = 0;

        public decimal amortiza { get; set; } = 0;

        public int orden { get; set; } = 0;

        public decimal poliza_prevista => 0M;

        public decimal total =>
            ab_int_cor +
            ab_int_mor +
            ab_cargo +
            ab_amortiza;

        public decimal compromiso_pendiente =>
            string.Equals(
                tipo.Trim(),
                "E",
                StringComparison.OrdinalIgnoreCase)
                ? ab_amortiza * -1M
                : int_cor +
                  int_mor +
                  cargo +
                  amortiza -
                  total;
    }
}