using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrCatalogoGrupoData
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal presu_mensual { get; set; } = 0;
        public decimal presu_diario { get; set; } = 0;
        public bool estado { get; set; } = false;
    }

    public class CrCatalogoGrupoConsultaData
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal presupuesto { get; set; } = 0;
        public decimal real { get; set; } = 0;
        public decimal diferencia { get; set; } = 0;
        public bool negativo { get; set; } = false;
    }

    public class CrCatalogoGrupoAsignacionCatalogoData : DropDownListaGenericaModel
    {
        public string tipo { get; set; } = string.Empty;
        public bool existe { get; set; } = false;
    }

    public class CrCatalogoGrupoDiarioData
    {
        public DateTime? fecha { get; set; }
        public decimal presupuesto { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fechai { get; set; }
    }

    public class CrCatalogoGrupoConsultaRequest
    {
        public string referencia { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public bool activos { get; set; } = false;
        public List<string> grupos { get; set; } = new();
    }

    public class CrCatalogoGrupoAsignacionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public bool isChecked { get; set; } = false;
    }

    public class CrCatalogoGrupoDiarioGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public decimal presupuesto { get; set; } = 0;
        public bool reemplazar { get; set; } = false;
    }
}