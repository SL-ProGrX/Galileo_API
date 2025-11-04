namespace PgxAPI.Models.GEN
{
    public class CCPeriodoList
    {
        public int idx { get; set; }
        public string itmx { get; set; } = string.Empty;
    }

    public class CCExcPeriodoData
    {
        public string? nc_mora { get; set; }
        public string? nc_opcf { get; set; }
        public string? nc_saldos { get; set; }
    }

    public class CCConsultaExcedenteData
    {
        public int id_periodo { get; set; }
        public string cedula { get; set; } = string.Empty;
        public decimal excedente_bruto { get; set; }
        public decimal capitalizado { get; set; }
        public decimal renta_total { get; set; }
        public decimal renta { get; set; }
        public decimal renta_retenida { get; set; }
        public decimal excedente_neto { get; set; }
        public decimal donacion { get; set; }
        public decimal excedente_neto2 { get; set; }
        public decimal ajuste_cargado { get; set; }
        public decimal ajuste_aplicado { get; set; }
        public decimal excedente_posajuste { get; set; }
        public decimal mora_cargada { get; set; }
        public decimal mora_aplicada { get; set; }
        public decimal exc_posmora { get; set; }
        public decimal moraopcf_cargada { get; set; }
        public decimal moraopcf_aplicada { get; set; }
        public decimal exc_posmoraopcf { get; set; }
        public decimal capitalizado_individual { get; set; }
        public decimal saldos_ase_cargado { get; set; }
        public decimal saldos_ase_aplicados { get; set; }
        public decimal exc_possaldos_ase { get; set; }
        public decimal excedente_final { get; set; }
        public int ind_act_mora { get; set; }
        public int ind_act_capind { get; set; }
        public int ind_act_capgen { get; set; }
        public int ind_act_ajustes { get; set; }
        public int ind_act_moraopcf { get; set; }
        public int ind_act_creditosase { get; set; }
        public string estadoactual { get; set; } = string.Empty;
        public string salida_codigo { get; set; } = string.Empty;
        public DateTime? salida_fecha { get; set; }
        public string salida_usuario { get; set; } = string.Empty;
        public string cuenta_bancaria { get; set; } = string.Empty;
        public decimal? reserva { get; set; }
        public decimal? salidafnd { get; set; }
        public decimal? extraordinario_apl { get; set; }
        public int? ind_insolvente { get; set; }
        public string salidadesc { get; set; } = string.Empty;
    }

    public class VSifAuxCreditosMovDetalle
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string lineax { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public int id_solicitud { get; set; }
        public decimal intcor { get; set; }
        public decimal intmor { get; set; }
        public decimal cargo { get; set; }
        public decimal poliza { get; set; }
        public decimal principal { get; set; }
        public int ncon { get; set; }
        public string tcon { get; set; } = string.Empty;
        public float proceso { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string fuente { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public float prideduc { get; set; }
        public float fecult { get; set; }
        public decimal monto_credito { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string garantiadesc { get; set; } = string.Empty;
        public string fecha_emision { get; set; } = string.Empty;
        public decimal id_seq { get; set; }
        public decimal total_mov { get; set; }
        public string antiguedad { get; set; } = string.Empty;
        public decimal tasa_mov { get; set; }
        public decimal tasa_actual { get; set; }
        public decimal debito { get; set; }
        public decimal credito { get; set; }
        public decimal int_debito { get; set; }
        public decimal int_credito { get; set; }
        public int anio { get; set; }
        public int mes { get; set; }
        public decimal cuotacancelada { get; set; }
    }
}