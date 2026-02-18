namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasCorreccionesModels
    {
        public class CuentaPorCobrarData
        {
            public string? Cedula { get; set; } = string.Empty;
            public string? Nombre { get; set; } = string.Empty;
            public string? Cod_Concepto { get; set; } = string.Empty;
            public string? ConceptoDesc { get; set; } = string.Empty;
            public string? Cod_Contrato { get; set; } = string.Empty;
            public string? ContratoDesc { get; set; } = string.Empty;
            public string? Cedula_Pagador { get; set; } = string.Empty;
            public string? PagadorNom { get; set; } = string.Empty;
            public string? Cedula_Autorizado { get; set; } = string.Empty;
            public string? AutorizadoNom { get; set; } = string.Empty;
            public string? BancoDesc { get; set; } = string.Empty;
            public int Emitir_Banco { get; set; }
            public string? Emitir_Tipo { get; set; } = string.Empty;
            public string? Emitir_Cuenta { get; set; } = string.Empty;
            public string? CuentaDesc { get; set; } = string.Empty;
            public string? Notas { get; set; } = string.Empty;
            public int? Operacion { get; set; }
            public int? Pagadores_Abierto { get; set; }
        }

        public class ResultadoCuentaPorCobrarData
        { 
            public string TipoDoc { get; set; } = string.Empty;
            public string NumDoc { get; set; } = string.Empty;
        }
        public class BancoAutorizados
        {
            public int Id_Banco { get; set; }
            public string? Descripcion { get; set; } = string.Empty;
            public string? Desc_Corta { get; set; } = string.Empty;
            public string? CTA { get; set; } = string.Empty;
            public string? Cod_Divisa { get; set; } = string.Empty;
            public int IdX { get; set; }
            public string? ItmX { get; set; } = string.Empty;
        }
        public class CuentasBancarias
        { 
            public string? Cuenta_Interna { get; set; } = string.Empty;
            public string? Cuenta_Desc { get; set; } = string.Empty;
            public int IdX { get; set; }
            public string? ItmX { get; set; } = string.Empty;
            public int Prioridad { get; set; }
        }
        public class GeneralData
        {
            public string? Item { get; set; } = string.Empty;
            public string? Detalle { get; set; } = string.Empty;
        }
        public class ContratoData
        {
            public string? Cod_Contrato { get; set; } = string.Empty;
            public string? Descripcion { get; set; } = string.Empty;
            public int? Pagadores_Abierto { get; set; }
        }
        public class  ConceptosData
        {
            public string? Descripcion { get; set; } = string.Empty;
            public int Requiere_Contrato { get; set; }
            public int Proceso_Descuento { get; set; }
        }
    }
}
