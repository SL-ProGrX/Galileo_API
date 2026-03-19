namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCCuentasBusquedaOperacionLista
    {
        public int total { get; set; }
        public List<CxCCuentasBusquedaOperacionItem> lista { get; set; } = new();
    }

    public class CxCCuentasBusquedaOperacionRequest: CxCCuentasBusquedaOperacionItem
    {
        public int skip { get; set; }
        public int take { get; set; }
    }

    public class CxCCuentasBusquedaOperacionItem
    {
        public long? operacion { get; set; }
        public string? cedula { get; set; }
        public string? num_documento { get; set; }
        public string? cod_concepto { get; set; }
        public string? cod_oficina { get; set; }
    }

    public class BancoAutorizadoComboDto
    {
        public int IdX { get; set; }
        public string? ItmX { get; set; }
    }

    public class CxCCuentasConsultaData
    {
        public string? cedula { get; set; } = string.Empty;
        public long operacion { get; set; }
        public string? nombre { get; set; } = string.Empty;
        public string? cod_concepto { get; set; } = string.Empty;
        public string? conceptodesc { get; set; } = string.Empty;
        public string? cod_contrato { get; set; } = string.Empty;
        public string? contratodesc { get; set; } = string.Empty;
        public string? cedula_pagador { get; set; } = string.Empty;
        public string? pagadornom { get; set; } = string.Empty;
        public string? cedula_autorizado { get; set; } = string.Empty;
        public string? autorizadonom { get; set; } = string.Empty;
        public string? bancodesc { get; set; } = string.Empty;
        public int? emitir_banco { get; set; }
        public string? emitir_tipo { get; set; } = string.Empty;
        public string? emitir_cuenta { get; set; } = string.Empty;
        public string? cuentadesc { get; set; } = string.Empty;
        public string? notas { get; set; } = string.Empty;
        public string? cod_oficina { get; set; } = string.Empty;
        public string? oficinax { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public int plazo { get; set; }
        public decimal tasa_corriente { get; set; }
        public decimal tasa_mora { get; set; }
        public decimal cuota { get; set; }
        public string? num_documento { get; set; } = string.Empty;
        public string? estado { get; set; } = string.Empty;
        public string? autorizado { get; set; } = string.Empty;
        public string? autoriza_estado { get; set; } = string.Empty;
        public string? autoriza_usuario { get; set; } = string.Empty;
        public DateTime? autoriza_fecha { get; set; }
        public string? autoriza_notas { get; set; } = string.Empty;
        public string? registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string? activa_usuario { get; set; } = string.Empty;
        public DateTime? activa_fecha { get; set; }
        public string? tesoreria_usuario { get; set; } = string.Empty;
        public DateTime? tesoreria_fecha { get; set; }
        public decimal desembolso_monto { get; set; }
        public decimal desembolso_realizado { get; set; }
        public decimal desembolso_pendiente { get; set; }
        public int facturas { get; set; }
        public decimal adelanto_monto { get; set; }
        public decimal adelanto_porcentaje { get; set; }
        public decimal adelanto_comision { get; set; }
        public int adelanto_comision_dias { get; set; }
        public bool adelanto_comision_apl { get; set; }
        public bool cuotas_apl { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public int pagadores_abierto { get; set; }
        public DateTime? fechaserver { get; set; }
    }
}
