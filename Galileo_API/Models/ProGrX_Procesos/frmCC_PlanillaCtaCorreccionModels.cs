using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Procesos
{
    public class CcPlanillaCtaCorreccionListaRequest
    {
        public int tipo_consulta { get; set; } = 1;
        public int cod_institucion { get; set; }
        public decimal proceso { get; set; }
        public string? operacion { get; set; }
        public string? linea { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? usuario { get; set; }
        public FiltrosLazyLoadData filtros { get; set; } = new();
    }

    public class CcPlanillaCtaCorreccionListaResult
    {
        public int total { get; set; }
        public List<CcPlanillaCtaCorreccionListaItemDto> lista { get; set; } = new();
    }

    public class CcPlanillaCtaCorreccionListaItemDto
    {
        public int tipo_consulta { get; set; }
        public string id_registro { get; set; } = string.Empty;
        public string referencia { get; set; } = string.Empty;
        public string cod_deduccion { get; set; } = string.Empty;
        public string proceso_bitacora { get; set; } = string.Empty;
        public long id_solicitud { get; set; }
        public string linea { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public short indicador_mora { get; set; }
        public decimal cuota { get; set; }
        public decimal cuota_anterior { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class CcPlanillaCtaCorreccionActualizarCuotaRequest
    {
        public int cod_institucion { get; set; }
        public int proceso { get; set; }
        public long operacion { get; set; }
        public string linea { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public decimal cuota { get; set; }
        public decimal cuota_anterior { get; set; }
        public string referencia { get; set; } = string.Empty;
        public short mora_ind { get; set; }
        public string cod_deduccion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CcPlanillaCtaCorreccionProcesoScrollDto
    {
        public decimal proceso { get; set; }
        public string proceso_format { get; set; } = string.Empty;
    }

    public class CcPlanillaCtaCorreccionPersonaF4Dto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }
}