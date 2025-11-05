namespace PgxAPI.Models.ProGrX.Clientes
{
    public class FrmAfLiquidacionWModels
    {
        public class AfLiquidacionBancos
        {
            public int Id_Banco { get; set; }
            public string Descripcion { get; set; } = string.Empty;
            public string Desc_Corta { get; set; } = string.Empty;
            public string Cta { get; set; } = string.Empty;
            public string Cod_Divisa { get; set; } = string.Empty;
            public int IdX { get; set; }
            public string ItmX { get; set; } = string.Empty;
        }

        public class AfLiquidacionBancosFiltro
        {
            public string Usuario { get; set; } = string.Empty;
            public string? Divisa { get; set; }
        }

        public class AfLiquidacionEmiteTDocFiltro
        {
            public int BancoId { get; set; }
            public int Mortalidad { get; set; }
            public string Cedula { get; set; } = "A";
            public string TipoRen { get; set; } = "A";
            public int IdCausa { get; set; } = 0;
        }

        public class AfLiquidacionEmiteTDoc
        {
            public string IdX { get; set; } = string.Empty;
            public string ItmX { get; set; } = string.Empty;
        }

        public class AfLiquidacionCausasDetalle
        {
            public byte Mortalidad { get; set; }
            public byte Liq_Alterna { get; set; }
            public string Tipo_Apl { get; set; } = string.Empty;
            public byte Ajuste_Tasas { get; set; }
        }

        public class AfLiquidacionCuentaBancaria
        {
            public string Cuenta_Bancaria { get; set; } = string.Empty;
            public string Cuenta_Desc { get; set; } = string.Empty;
            public string IdX { get; set; } = string.Empty;
            public string ItmX { get; set; } = string.Empty;
            public int Prioridad { get; set; }
        }

        public class AfLiquidacionCuentaBancariaFiltro
        {
            public string Identificacion { get; set; } = string.Empty;
            public int BancoId { get; set; }
            public short DivisaCheck { get; set; } = 0;
        }
    }   

    public class AfLiquidacionRenunciaSinLiquidar
    {
        public string Cedula { get; set; } = string.Empty;
        public string Id_Alterno { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class AfLiquidacionSocio
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class AfLiquidacionSocioDetalle
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public string EstadoActual { get; set; } = string.Empty;
        public int Boleta { get; set; }
        public string EstadoPersona { get; set; } = string.Empty;
    }
}