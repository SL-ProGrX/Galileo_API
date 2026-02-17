namespace Galileo_API.Models.ProGrX_Polizas
{
    public class CrdCatalogoPolizasListDto
    {
        public string cod_poliza { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string @base { get; set; } = string.Empty;   // C/A/X/S/P
        public string tipo { get; set; } = string.Empty;    // P/M
        public decimal valor { get; set; } = 0;
        public decimal porc_formalizacion { get; set; } = 0;
        public string codigo_retencion { get; set; } = string.Empty;
        public string codigo_cargo { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
    }

    public class CrdCatalogoPolizasConsultaDto
    {
        public string cod_poliza { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;

        public string ctacodigo { get; set; } = string.Empty;
        public string ctadesc { get; set; } = string.Empty;

        public string @base { get; set; } = string.Empty;
        public string basedesc { get; set; } = string.Empty;

        public string tipo { get; set; } = string.Empty;
        public decimal valor { get; set; } = 0;

        public int integra_plan_pagos { get; set; } = 0;

        public string codigo_retencion { get; set; } = string.Empty;
        public string retenciondesc { get; set; } = string.Empty;

        public string codigo_cargo { get; set; } = string.Empty;
        public string cargodesc { get; set; } = string.Empty;

        public decimal porc_formalizacion { get; set; } = 0;
        public int plazo_meses { get; set; } = 0;

        public string? cod_aseguradora { get; set; }
        public string? aseguradoradesc { get; set; }

        public int? id_poliza_grupo { get; set; }
        public string? aplicaciondesc { get; set; }

        public decimal cobertura_inicio { get; set; } = 0;
        public decimal cobertura_corte { get; set; } = 0;

        public string? contrato_num { get; set; }

        public int vence_dia { get; set; } = 0;
        public string frecuenciadesc { get; set; } = string.Empty;
        public DateTime cobertura_vencimiento { get; set; }

        public int? poliza_general { get; set; }
        public string poliza_general_tipo { get; set; } = string.Empty;
        public decimal poliza_general_monto { get; set; }

        public int cobertura_region { get; set; } = 0;

        public int iva_aplica { get; set; } = 0;
        public int iva_incluido { get; set; } = 0;
        public decimal iva_porcentaje { get; set; } = 0;

        public string? ctagastocodigo { get; set; }
        public string? ctagastodesc { get; set; }

        public string? cod_unidad { get; set; }
        public string? unidaddesc { get; set; }

        public string? cod_centro_costo { get; set; }
        public string? centrocostodesc { get; set; }
    }

    public class CrdCatalogoPolizasAcreedorDto
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;

        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }

        // Campo calculado para Angular
        public bool asignado { get; set; } = false;   // true/false (VB6 usa checkbox)
    }

    public class CrdCatalogoPolizasAcreedorAsignarReq
    {
        public string cod_poliza { get; set; } = string.Empty;
        public string cod_acreedor { get; set; } = string.Empty;
        public bool asignar { get; set; } = false;
    }

    public class CrdCatalogoPolizasGarantiaDto
    {
        public string garantia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;

        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }

        public int asignado { get; set; } = 0;     // 1 o 0
    }

    public class CrdCatalogoPolizasGarantiaAsignaDto
    {
        public int pass { get; set; } = 0;
        public string? movimiento { get; set; }
        public string? mensaje { get; set; }
    }

    public class CrdCatalogoPolizasGarantiaAsignarReq
    {
        public string cod_poliza { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public bool asignar { get; set; } = false;
    }

    public class CrdPolizasAcreedoresGridDto
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal? cxp_enlace { get; set; }  // en VB6 lo tratan como número
        public bool activo { get; set; } = false;       // 1/0 (VB6 usa checkbox)
        public bool isNew { get; set; } = false;
    }

    public class CrdPolizasAcreedoresGridSaveDto
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal? cxp_enlace { get; set; }  // en VB6 lo tratan como número
        public int activo { get; set; } = 0;           // 1/0 (VB6 usa checkbox)
       
    }

    public class CrdPolizasAcreedorUsoDto
    {
        public int existe { get; set; } = 0;       // 1/0
        public int cantidad { get; set; } = 0;      // # de asignaciones
    }

    public class CrdTreeNodeDto
    {
        public string key { get; set; } = string.Empty;     // ej: "Lineas" o "0x0ABC123L"
        public string label { get; set; } = string.Empty;   // texto visible
        public bool leaf { get; set; }                      // true si no se expande
        public List<CrdTreeNodeDto>? children { get; set; } // opcional (si ya vienen cargados)
        public object? data { get; set; }                   // opcional: metadatos
    }

    public class CrdCatalogoPolizasAsignacionDto
    {
        public string cod_poliza { get; set; } = string.Empty;
        public string? descripcion { get; set; }

        // rs!Tipo en VB6 es "P" o "M"
        public string? tipo { get; set; }

        public decimal? valor { get; set; }

        // equivalente a: itmX.Checked = IIf(IsNull(rs!Existe), False, True)
        public bool asignado { get; set; } = false;
    }

    public class CrdCatalogoPolizasAsignacionUpdateDto
    {
        public string cod_poliza { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }

    public class CrdCatalogoPolizasGuardarDto
    {
        public string? cod_poliza { get; set; }
        public string? descripcion { get; set; }

        // Combos (se mandan ya como código)
        public string? @base { get; set; }                 // C/A/X/S/P
        public string? tipo { get; set; }                 // P/M  (Porcentaje/Monto)

        public decimal? valor { get; set; }
        public decimal? porc_formalizacion { get; set; }
        public int? plazo_meses { get; set; }

        public string? cod_cuenta { get; set; }           // ya formateada o normalizada por el front/back
        public string? codigo_retencion { get; set; }
        public string? codigo_cargo { get; set; }

        public decimal? cobertura_inicio { get; set; }
        public decimal? cobertura_corte { get; set; }

        public string? cod_aseguradora { get; set; }      // puede venir null
        public string? contrato_num { get; set; }

        public DateTime? cobertura_vencimiento { get; set; }
        public string? vence_frecuencia { get; set; }     // A/M (Anual/Mensual) o lo que uses en DB
        public int? vence_dia { get; set; }               // 1..30

        public int? poliza_general { get; set; }          // 0/1
        public int? cobertura_region { get; set; }        // 0/1
        public int? integra_plan_pagos { get; set; }      // 0/1

        public string? poliza_general_tipo { get; set; }  // C/A (Crédito/Asociados)
        public decimal? poliza_general_monto { get; set; }

        public int? iva_aplica { get; set; }              // 0/1
        public int? iva_incluido { get; set; }            // 0/1
        public decimal? iva_porcentaje { get; set; }

        public int? id_poliza_grupo { get; set; }         // cboAplicacion (puede venir null)

        public string? cod_cuenta_gasto { get; set; }
        public string? cod_unidad { get; set; }
        public string? cod_centro_costo { get; set; }
    }
}
