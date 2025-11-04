namespace PgxAPI.Models.ProGrX_Procesos
{
    public class PatrimonioPrincipalDto
    {
        public string consec { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string monto { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public int fechaproc { get; set; }
        public string estado { get; set; } = string.Empty;
        public string numcom { get; set; } = string.Empty;
        public string tcon { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string fecajusteahorro { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public string tipodoc { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;

    }

    public class LiquidacionPatrimonioDto
    {
        public string consec { get; set; } = string.Empty;
        public string fecliq { get; set; } = string.Empty;
        public string aporte_liq { get; set; } = string.Empty;
        public string ahorro_liq { get; set; } = string.Empty;
        public string extra_liq { get; set; } = string.Empty;
        public string capitalizado_liq { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;

    }

    public class ResumenPatrimonioDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estadoactual { get; set; } = string.Empty;
        public string obrero { get; set; } = string.Empty;
        public string patronal { get; set; } = string.Empty;
        public string custodia { get; set; } = string.Empty;
        public string capitaliza { get; set; } = string.Empty;
        public string fecahorro { get; set; } = string.Empty;
        public string fecaporte { get; set; } = string.Empty;
        public string feccapitaliza { get; set; } = string.Empty;
        public string feccustodia { get; set; } = string.Empty;
        public string fecliq { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;

    }

    public class ExcedentePatrimonioDto
    {
        public string inicio { get; set; } = string.Empty;
        public string corte { get; set; } = string.Empty;
        public string id_periodo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string excedente_bruto { get; set; } = string.Empty;
        public string capitalizado { get; set; } = string.Empty;
        public string renta_total { get; set; } = string.Empty;
        public string renta { get; set; } = string.Empty;
        public string renta_retenida { get; set; } = string.Empty;
        public string excedente_neto { get; set; } = string.Empty;
        public string donacion { get; set; } = string.Empty;
        public string excedente_neto2 { get; set; } = string.Empty;
        public string ajuste_cargado { get; set; } = string.Empty;
        public string ajuste_aplicado { get; set; } = string.Empty;
        public string excedente_posajuste { get; set; } = string.Empty;
        public string mora_cargada { get; set; } = string.Empty;
        public string mora_aplicada { get; set; } = string.Empty;
        public string exc_posmora { get; set; } = string.Empty;
        public string moraopcf_cargada { get; set; } = string.Empty;
        public string moraopcf_aplicada { get; set; } = string.Empty;
        public string exc_posmoraopcf { get; set; } = string.Empty;
        public string capitalizado_individual { get; set; } = string.Empty;
        public string saldos_ase_cargado { get; set; } = string.Empty;
        public string saldos_ase_aplicados { get; set; } = string.Empty;
        public string exc_possaldos_ase { get; set; } = string.Empty;
        public string excedente_final { get; set; } = string.Empty;
        public string ind_act_mora { get; set; } = string.Empty;
        public string ind_act_capind { get; set; } = string.Empty;
        public string ind_act_capgen { get; set; } = string.Empty;
        public string ind_act_ajustes { get; set; } = string.Empty;
        public string ind_act_moraopcf { get; set; } = string.Empty;
        public string ind_act_creditosase { get; set; } = string.Empty;
        public string estadoactual { get; set; } = string.Empty;
        public string salida_codigo { get; set; } = string.Empty;
        public DateTime salida_fecha { get; set; }
        public string salida_usuario { get; set; } = string.Empty;
        public string cuenta_bancaria { get; set; } = string.Empty;
        public string reserva { get; set; } = string.Empty;
        public string salidafnd { get; set; } = string.Empty;
        public string extraordinario_apl { get; set; } = string.Empty;
        public string ind_insolvente { get; set; } = string.Empty;

    }

    public class HistoricoPatrimonioDto
    {
        public string anio { get; set; } = string.Empty;
        public string mes { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string aporte { get; set; } = string.Empty;
        public string extra { get; set; } = string.Empty;
        public string ahorro { get; set; } = string.Empty;
        public string capitaliza { get; set; } = string.Empty;
        public string estadoactual { get; set; } = string.Empty;
        public string custodia { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string rend_custodia_base { get; set; } = string.Empty;
        public string rend_custodia { get; set; } = string.Empty;
        public string rend_custodia_apl { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
    }
}
