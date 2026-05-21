namespace Galileo.Models.ProGrX.Fondos
{
    public class CuentaBancariaVendedorDto
    {
        public string banco { get; set; } = string.Empty;
        public string tipodesc { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public string cuenta_interna { get; set; } = string.Empty;
        public bool cuenta_interbanca { get; set; }
        public bool activa { get; set; }
        public string destino { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FndVendedorDto
    {
        public required int cod_vendedor { get; set; }
        public string? nombre { get; set; } = string.Empty;
        public int? cod_banco { get; set; }
        public string? tipo_pago { get; set; } = string.Empty;
        public string? cuenta_ahorros { get; set; } = string.Empty;
        public required bool aplica_comision { get; set; }
        public int? minimo { get; set; }
        public decimal? porc_comision { get; set; }
        public string? cedula { get; set; } = string.Empty;
        public int? tipo_id { get; set; }
        public string? estado { get; set; } = string.Empty;
        public string? banco_desc { get; set; } = string.Empty;
    }

    public class FndVendedorListaDto
    {
        public string cod_vendedor { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class FndVendedoresBancoSpDto
    {
        public int IDX { get; set; }
        public string ITMX { get; set; } = string.Empty;
    }

}
