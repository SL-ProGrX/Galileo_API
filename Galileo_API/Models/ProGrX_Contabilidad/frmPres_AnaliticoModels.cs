namespace PgxAPI.Models.PRES
{
    public class PresAnaliticoDescData
    {
        public string modelo_desc { get; set; } = string.Empty;
        public string conta_desc { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
        public string unidad_desc { get; set; } = string.Empty;
        public string centro_desc { get; set; } = string.Empty;
        public string cuenta_mask { get; set; } = string.Empty;
    }

    public class PresAnaliticoBuscar
    {
        public string Modelo { get; set; } = string.Empty;
        public int Contabilidad { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public string CentroCosto { get; set; } = string.Empty;
        public string Periodo { get; set; } = string.Empty;
    }

    public class PresAnaliticoData
    {
        public string tipo_asiento { get; set; } = string.Empty;
        public string num_asiento { get; set; } = string.Empty;
        public DateTime fecha_asiento { get; set; }
        public string user_crea { get; set; } = string.Empty;
        public string user_aplica { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string tipo_cambio { get; set; } = string.Empty;
        public float importe { get; set; }
        public float monto_debito { get; set; }
        public float monto_credito { get; set; }
        public string documento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string referencia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }
}