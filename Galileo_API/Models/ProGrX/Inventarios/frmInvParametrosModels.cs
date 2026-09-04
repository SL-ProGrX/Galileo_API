namespace Galileo.Models.INV
{
    public class ParametrosGenDto
    {
        public int cod_par { get; set; } = 0;
        public int cod_empresa { get; set; } = 0;
        public string cta_comisiones { get; set; } = string.Empty;
        public string cta_imp_renta { get; set; } = string.Empty;
        public string cta_imp_consumo { get; set; } = string.Empty;
        public string cta_gastos { get; set; } = string.Empty;
        public string cta_costo_ventas { get; set; } = string.Empty;
        public string cta_recibos { get; set; } = string.Empty;
        public string cta_notas { get; set; } = string.Empty;
        public string cta_ventas_ing { get; set; } = string.Empty;
        public string ta_factura_man { get; set; } = string.Empty;
        public string ta_factura_auto { get; set; } = string.Empty;
        public string ta_entradas { get; set; } = string.Empty;
        public string ta_salidas { get; set; } = string.Empty;
        public string ta_traslados { get; set; } = string.Empty;
        public string ta_devoluciones { get; set; } = string.Empty;
        public string ta_nc { get; set; } = string.Empty;
        public string ta_recibos { get; set; } = string.Empty;
        public string ta_nd { get; set; } = string.Empty;
        public string ta_gen { get; set; } = string.Empty;
        public string enlace_conta { get; set; } = string.Empty;
        public string enlace_sif { get; set; } = string.Empty;
    }

    public class CntXContaDto
    {
        public int cod_contabilidad { get; set; } = 0;
        public string nombre { get; set; } = string.Empty;
    }
}