namespace PgxAPI.Models.ProGrX.Bancos
{

    public class DropDownListaBancosDocumentos
    {
        public string id_banco { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;

    }

    public class DropDownListaTiposDocumentos
    {
        public string itmy { get; set; } = string.Empty;
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class TES_EntregaDocumentosFiltros
    {
        public string id_banco { get; set; } = string.Empty;
        public string cod_banco { get; set; } = string.Empty;
        public string tipo_doc { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string fecha_desde { get; set; } = string.Empty;
        public string fecha_hasta { get; set; } = string.Empty;
        public bool todas_fechas { get; set; }


    }

    public class EntregaDocumentoPendientesDTO
        {
            public string nsolicitud { get; set; } = string.Empty;
            public string ndocumento { get; set; } = string.Empty;
            public decimal monto { get; set; }
            public string fecha_emision { get; set; } = string.Empty;
            public string beneficiario { get; set; } = string.Empty;
        }

    }




