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


}
