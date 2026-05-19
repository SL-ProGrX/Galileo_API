using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrCatalogoGrupoData
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal presu_mensual { get; set; }
        public decimal presu_diario { get; set; }
        public bool estado { get; set; }
    }

    public class CrCatalogoGrupoConsultaData
    {
        public string cod_grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal presupuesto { get; set; }
        public decimal real { get; set; }
        public decimal diferencia { get; set; }
        public bool negativo { get; set; }
    }

    public class CrCatalogoGrupoAsignacionCatalogoData : DropDownListaGenericaModel
    {
        public string tipo { get; set; } = string.Empty;
        public bool existe { get; set; }
    }

    public class CrCatalogoGrupoDiarioData
    {
        public DateTime fecha { get; set; }
        public decimal presupuesto { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? fechai { get; set; }
    }

    public class CrCatalogoGrupoConsultaRequest
    {
        public string referencia { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public bool activos { get; set; } = true;
        public List<string> grupos { get; set; } = new();
    }

    public class CrCatalogoGrupoAsignacionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public bool isChecked { get; set; }
    }

    public class CrCatalogoGrupoDiarioGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public decimal presupuesto { get; set; }
        public bool reemplazar { get; set; }
    }
}