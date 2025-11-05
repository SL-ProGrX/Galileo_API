namespace PgxAPI.Models.ProGrX.Bancos
{
    public class DropDownListaBancos
    {
        public string id_banco { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class DropDownListaTipos
    {
        public string itmy { get; set; } = string.Empty;
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class TesDocumentosDuplicadosFiltros
    {
        public string id_banco { get; set; } = string.Empty;
        public string cod_banco { get; set; } = string.Empty;
        public string tipo_doc { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string fecha_desde { get; set; } = string.Empty;
        public string fecha_hasta { get; set; } = string.Empty;
    }

    public class DocumentoDuplicadosLista
    {
        public int nsolicitud { get; set; }
        public string id_banco { get; set; } = string.Empty;
        public int ndocumento { get; set; } 
        public decimal monto { get; set; } 
        public DateTime fecha_emision { get; set; }
        public string beneficiario { get; set; } = string.Empty;
        public string estado_asiento { get; set; } = string.Empty;
    }
}