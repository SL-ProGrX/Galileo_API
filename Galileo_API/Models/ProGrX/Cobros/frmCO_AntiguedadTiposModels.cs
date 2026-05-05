
namespace Galileo.Models.ProGrX.Cobros
{
    public class FrmCOAntiguedadTipoData
    {
        public string cod_antiguedad { get; set; } = "";
        public string descripcion { get; set; } = "";
        public int dias_desde { get; set; } = 0;
        public int dias_hasta { get; set; } = 0;
        public decimal estimacion_nocubierta { get; set; } = 0;
        public decimal estimacion_cubierta { get; set; } = 0;
        public bool isNew { get; set; } = false;
    }

    public class FrmCOAntiguedadTiposListaResult
    {
        public List<FrmCOAntiguedadTipoData> lista { get; set; } = new();
        public int total { get; set; } = 0;
    }

    public class FrmCOAntiguedadGarantiaMitigadorData
    {
        public string codigo { get; set; } = "";
        public string garantia { get; set; } = "";
        public decimal mitigador { get; set; } = 0;
        public bool isNew { get; set; } = false;
    }

    public class FrmCOAntiguedadDetalleGuardarDto
    {
        public string cod_antiguedad { get; set; } = "";
        public string codigo { get; set; } = "";
        public string garantia { get; set; } = "";
        public decimal mitigador { get; set; } = 0;
        public bool isNew { get; set; } = false;
    }
}
