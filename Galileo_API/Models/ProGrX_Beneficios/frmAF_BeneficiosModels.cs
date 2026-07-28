namespace Galileo.Models.AF
{
    public class AfiBeneficiosDto
    {
        public string cod_beneficio { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string? notas { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public DateTime registra_fecha { get; set; }
        public string registra_user { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public int? consecutivo { get; set; }
        public float? monto { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int maximo_otorga { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int modifica_monto { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public float modifica_diferencia { get; set; }
        public string cod_cuenta { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public int aplica_beneficiarios { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int aplica_parcial { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int tipo_producto { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int tipo_monetario { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int vigencia_meses { get; set; }
        public string cod_grupo { get; set; } = string.Empty;
        public string cod_categoria { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public int pagos_multiples { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int i_condicion_especial { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int i_morosidad { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int i_suspendidos { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int i_insolventes { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int i_cobro_judicial { get; set; }

    }

    public class AfiBeneficioMontoData
    {
        [System.Text.Json.Serialization.JsonRequired]
        public int id_bene { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int inicio { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int corte { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public float monto { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string registra_user { get; set; } = string.Empty;
    }

    public class AfiBeneficioGruposData
    {
        public string Grupo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool cod_grupo { get; set; }
    }

    public class AfiBeneListas
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
    
    public class AfiBeneFechaPagoData
    {
        [System.Text.Json.Serialization.JsonRequired]
        public int id_fecha_pago { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
        public string cod_categoria { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonRequired]
        public DateTime fecha_corte { get; set; }
        public int? mes { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public int periodo { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public bool activo { get; set; }
        public string? registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public float monto { get; set; }
    }
}
