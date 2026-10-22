namespace Galileo.Models.ProGrX_Inversiones
{
    public sealed class IvrCatCuentasContablesInicializarRequest
    {
        public string tipo { get; set; } = string.Empty;

        public string codigo_01 { get; set; } = string.Empty;

        public string codigo_02 { get; set; } = string.Empty;

        public int cod_contabilidad { get; set; } = 0;
    }

    public sealed class IvrCatCuentasContablesInicialData
    {
        public IvrCatCuentasContablesData cuentas { get; set; } =
            new();

        public List<DropDownListaGenericaModel<string>> unidades
        {
            get;
            set;
        } = [];

        public List<DropDownListaGenericaModel<string>> centros_costos
        {
            get;
            set;
        } = [];
    }

    public sealed class IvrCatCuentasContablesData
    {
        public string tipo_base { get; set; } = string.Empty;

        public string codigo_01 { get; set; } = string.Empty;

        public string codigo_02 { get; set; } = string.Empty;

        public string cod_unidad { get; set; } = string.Empty;

        public string cod_centro_costo { get; set; } = string.Empty;

        public string unidad_desc { get; set; } = string.Empty;

        public string centro_desc { get; set; } = string.Empty;

        public string cta_inversion_mask { get; set; } =
            string.Empty;

        public string cta_inversion_desc { get; set; } =
            string.Empty;

        public string cta_intereses_acum_cobrar_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_intereses_acum_cobrar_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_ingresos_intereses_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_ingresos_intereses_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_ingresos_fluctuaciones_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_ingresos_fluctuaciones_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_gasto_comisiones_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_gasto_comisiones_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_prima_mask { get; set; } =
            string.Empty;

        public string cta_prima_desc { get; set; } =
            string.Empty;

        public string cta_prima_gasto_amort_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_prima_gasto_amort_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_descuentos_mask { get; set; } =
            string.Empty;

        public string cta_descuentos_desc { get; set; } =
            string.Empty;

        public string cta_descuento_ingreso_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_descuento_ingreso_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_val_activo_mask { get; set; } =
            string.Empty;

        public string cta_val_activo_desc { get; set; } =
            string.Empty;

        public string cta_val_patrimonio_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_val_patrimonio_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_val_resultados_gastos_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_val_resultados_gastos_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_val_resultados_ingresos_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_val_resultados_ingresos_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_recompra_principal_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_recompra_principal_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_recompra_intereses_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_recompra_intereses_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_intereses_pagar_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_intereses_pagar_desc
        {
            get;
            set;
        } = string.Empty;

        public string cta_gasto_intereses_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_gasto_intereses_desc
        {
            get;
            set;
        } = string.Empty;

        public string registro_usuario { get; set; } =
            string.Empty;

        public DateTime? registro_fecha { get; set; }

        public string actualiza_usuario { get; set; } =
            string.Empty;

        public DateTime? actualiza_fecha { get; set; }
    }

    public sealed class IvrCatCuentasContablesGuardarRequest
    {
        public string tipo { get; set; } = string.Empty;

        public string codigo_01 { get; set; } = string.Empty;

        public string codigo_02 { get; set; } = string.Empty;

        public string usuario { get; set; } = string.Empty;

        public string cod_unidad { get; set; } = string.Empty;

        public string cod_centro_costo { get; set; } =
            string.Empty;

        public string cta_inversion_mask { get; set; } =
            string.Empty;

        public string cta_intereses_acum_cobrar_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_ingresos_intereses_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_ingresos_fluctuaciones_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_gasto_comisiones_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_prima_mask { get; set; } =
            string.Empty;

        public string cta_prima_gasto_amort_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_descuentos_mask { get; set; } =
            string.Empty;

        public string cta_descuento_ingreso_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_val_activo_mask { get; set; } =
            string.Empty;

        public string cta_val_patrimonio_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_val_resultados_gastos_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_val_resultados_ingresos_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_recompra_principal_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_recompra_intereses_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_intereses_pagar_mask
        {
            get;
            set;
        } = string.Empty;

        public string cta_gasto_intereses_mask
        {
            get;
            set;
        } = string.Empty;
    }
}