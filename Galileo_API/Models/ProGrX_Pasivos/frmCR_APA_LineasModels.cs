namespace Galileo_API.Models.ProGrX_Pasivos
{
    public class FrmCrApaLineaCatalogoDto
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class FrmCrApaLineaCatalogosDto
    {
        public List<FrmCrApaLineaCatalogoDto> acreedores { get; set; } = [];
        public List<FrmCrApaLineaCatalogoDto> divisas { get; set; } = [];
        public List<FrmCrApaLineaCatalogoDto> recursos { get; set; } = [];
        public List<FrmCrApaLineaCatalogoDto> unidades { get; set; } = [];
        public DateTime fecha_servidor { get; set; }
    }

    public class FrmCrApaLineaConsultaRequest
    {
        public string? cod_acreedor { get; set; }
        public string? estado { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_vence { get; set; }
    }

    public class FrmCrApaLineaGridDto
    {
        public int cod_linea { get; set; }
        public string cod_acreedor { get; set; } = string.Empty;
        public string acreedor_desc { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string revolutiva_desc { get; set; } = string.Empty;
        public string tipo_desc { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_vence { get; set; }
        public decimal monto_aprobado { get; set; }
        public decimal tasa { get; set; }
        public int plazo { get; set; }
        public decimal cuota_inicial { get; set; }
        public decimal comision { get; set; }
        public string unidad_desc { get; set; } = string.Empty;
        public string centro_costo_desc { get; set; } = string.Empty;
        public string recurso_desc { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class FrmCrApaLineaDatosDto
    {
        public int cod_linea { get; set; }
        public string cod_acreedor { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_recurso { get; set; } = string.Empty;
        public string recurso_desc { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string unidad_desc { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string centro_costo_desc { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string divisa_desc { get; set; } = string.Empty;
        public decimal tipo_cambio { get; set; }
        public string prorrateo { get; set; } = string.Empty;
        public string prorrateo_desc { get; set; } = string.Empty;
        public bool activa { get; set; }
        public bool linea_revolutiva { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_vence { get; set; }
        public decimal monto_aprobado { get; set; }
        public decimal tasa { get; set; }
        public decimal comision { get; set; }
        public decimal cuota_inicial { get; set; }
        public int plazo { get; set; }
        public string notas { get; set; } = string.Empty;
    }

    public class FrmCrApaLineaGuardarRequest : FrmCrApaLineaDatosDto
    {
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmCrApaLineaGuardarResultadoDto
    {
        public int pass { get; set; }
        public int linea_id { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }
}
