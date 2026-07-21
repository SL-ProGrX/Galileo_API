namespace Galileo.Models.ProGrX.Bancos
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

    public class TesEntregaDocumentosFiltros
    {
        public string id_banco { get; set; } = string.Empty;
        public string cod_banco { get; set; } = string.Empty;
        public string tipo_doc { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string fecha_desde { get; set; } = string.Empty;
        public string fecha_hasta { get; set; } = string.Empty;
        public bool todas_fechas { get; set; }
    }

    public class EntregaDocumentoPendientesDto
    {
        public string nsolicitud { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string fecha_emision { get; set; } = string.Empty;
        public string beneficiario { get; set; } = string.Empty;
    }

    public sealed class TesEmisionGenerarLoteRequest
    {
        public required int CodEmpresa { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Filtros { get; set; } = string.Empty;   // JSON de filtros base
        public int Minimo { get; set; }
        public int Maximo { get; set; }
        public List<int> NSolicitudes { get; set; } = new();
    }

    public sealed class TesEmisionProcesoError
    {
        public int NSolicitud { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public sealed class TesEmisionGenerarLoteResult
    {
        public int Procesados { get; set; }
        public int ConErrores { get; set; }
        public List<TesEmisionProcesoError> Errores { get; set; } = new();
    }
}