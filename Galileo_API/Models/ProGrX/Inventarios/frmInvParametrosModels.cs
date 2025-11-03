namespace PgxAPI.Models.INV
{

    public class ParametrosGenDTO
    {
        public int Cod_Par { get; set; }
        public string Cta_Comisiones { get; set; } = string.Empty;
        public string Cta_Imp_Renta { get; set; } = string.Empty;
        public string Cta_Imp_Consumo { get; set; } = string.Empty;
        public string Cta_Gastos { get; set; } = string.Empty;
        public string Cta_Costo_Ventas { get; set; } = string.Empty;
        public string Cta_Recibos { get; set; } = string.Empty;
        public string Cta_Notas { get; set; } = string.Empty;
        public string Cta_Ventas_Ing { get; set; } = string.Empty;
        public string Ta_Factura_Man { get; set; } = string.Empty;
        public string Ta_Factura_Auto { get; set; } = string.Empty;
        public string Ta_Entradas { get; set; } = string.Empty;
        public string Ta_Salidas { get; set; } = string.Empty;
        public string Ta_Traslados { get; set; } = string.Empty;
        public string Ta_Devoluciones { get; set; } = string.Empty;
        public string Ta_Nc { get; set; } = string.Empty;
        public string Ta_Recibos { get; set; } = string.Empty;
        public string Ta_Nd { get; set; } = string.Empty;
        public string Ta_Gen { get; set; } = string.Empty;
        public string Enlace_Conta { get; set; } = string.Empty;
        public string Enlace_Sif { get; set; } = string.Empty;
    }


    public class CntX_ContaDTO
    {
        public int Cod_Contabilidad { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }


    public class DescripcionCuentasDTO
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
    }

    public class DescripcionTipoAsientoDTO
    {
        public string Tipo_Asiento { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

}
