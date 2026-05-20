namespace Galileo.Models.ProGrX.Cobros
{
    public class CoControlComPagoRemesaData
    {
        public int cod_remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string estado { get; set; } = string.Empty;
        public string estado_descripcion { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string notas { get; set; } = string.Empty;
        public int detalle_pago { get; set; }
    }

    public class CoControlComPagoRemesaGuardarRequest
    {
        public int cod_remesa { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string notas { get; set; } = string.Empty;
    }

    public class CoControlComPagoRemesaComboData
    {
        public int cod_remesa { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
    }

    public class CoControlComPagoBancoData
    {
        public int id_banco { get; set; }
        public string banco_desc { get; set; } = string.Empty;
    }

    public class CoControlComPagoBancoSpDto
    {
        public int id_Banco { get; set; }
        public string BancoDesc { get; set; } = string.Empty;
    }

    public class CoControlComPagoCargaData
    {
        public string usuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime recupera_inicio { get; set; }
        public DateTime recupera_corte { get; set; }
        public decimal recupera_monto { get; set; }
        public decimal comision { get; set; }
        public string banco_desc { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public string cuenta_ahorros { get; set; } = string.Empty;
    }

    public class CoControlComPagoCargaAplicarRequest
    {
        public int cod_remesa { get; set; }
        public List<string> usuarios { get; set; } = new();
    }

    public class CoControlComPagoTrasladoData
    {
        public string usuario { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string banco_desc { get; set; } = string.Empty;
        public decimal comision { get; set; }
        public int cod_banco { get; set; }
        public string tipo_emision { get; set; } = string.Empty;
        public string cuenta_ahorros { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string cta_conta { get; set; } = string.Empty;
    }

    public class CoControlComPagoTrasladoAplicarRequest
    {
        public int cod_remesa { get; set; }
        public List<string> usuarios { get; set; } = new();
    }

    public class CoControlComPagoProcesoResult
    {
        public int procesados { get; set; }
    }
}
