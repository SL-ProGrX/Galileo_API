
namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class frmSIF_DocsTrasladoModels
    {
        public class SifDocsTrasladoDocumentosData
        {
            public string Tipo_Documento { get; set; }
            public string descripcion { get; set; }
            public int pendientes { get; set; }
            public int bloqueados { get; set; }
            public int? asientoTransaccion { get; set; }
            public int codContabilidad { get; set; }
        }

        public class SifDocsTrasladoDocumentosLista
        {
            public int total { get; set; }
            public List<SifDocsTrasladoDocumentosData>? lista { get; set; }
        }
        public class SifDocsTrasladoDesbalanceadoData
        {
            public string Tipo_Documento { get; set; } = "";
            public string cod_transaccion { get; set; } = "";
            public DateTime Registro_Fecha { get; set; }
            public string Registro_Usuario { get; set; } = "";
            public decimal Monto { get; set; }
            public string Referencia { get; set; } = "";
            public string Notas { get; set; } = "";
        }

        public class SifDocsTrasladoDesbalanceadosLista
        {
            public int total { get; set; }
            public List<SifDocsTrasladoDesbalanceadoData>? lista { get; set; }
        }
        public class SifDocsTrasladoDocumentoConfig
        {
            public string tipoDocumento { get; set; }
            public string tipoAsiento { get; set; }
            public string asientoMascara { get; set; }
            public int asientoTransaccion { get; set; }
            public string asientoModulo { get; set; }
            public string descripcion { get; set; }
        }
        public class SifDocsTrasladoBuscarRequest
        {
            public DateTime fechaInicio { get; set; }
            public DateTime fechaFin { get; set; }
            public bool soloBalanceados { get; set; }
            public int? codContabilidad { get; set; }
            public bool reactivarAutomaticamente { get; set; }
        }

        public class SifDocsTrasladoEjecutarRequest
        {
            public string tipoDocumento { get; set; }
            public DateTime fechaInicio { get; set; }
            public DateTime fechaFin { get; set; }
            public bool soloBalanceados { get; set; }
            public string modo { get; set; }
            public string usuario { get; set; }
            public int? codContabilidad { get; set; }
        }

        public class SifDocsTrasladoReactivarRequest
        {
            public DateTime fechaInicio { get; set; }
            public DateTime fechaFin { get; set; }
        }
    }
}
