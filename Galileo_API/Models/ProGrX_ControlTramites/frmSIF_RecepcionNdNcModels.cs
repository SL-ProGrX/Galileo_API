using Galileo.Models;

namespace Galileo_API.Models.ProGrX_ControlTramites
{
    public sealed class SifRecepcionNdNcInicializaData
    {
        public string tag_recepcion { get; set; } = string.Empty;
        public string tag_devolucion { get; set; } = string.Empty;
        public string tag_recepcion_devolucion { get; set; } = string.Empty;
        public DateTime? fecha_servidor { get; set; }
        public List<DropDownListaGenericaModel> tipos_documento { get; set; } = [];
        public List<DropDownListaGenericaModel> usuarios { get; set; } = [];
    }

    public sealed class SifRecepcionNdNcDocumentosRequest
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string movimiento { get; set; } = "RECEPCION";
    }

    public sealed class SifRecepcionNdNcDocumentoData
    {
        public string cod_transaccion { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public string cliente_identificacion { get; set; } = string.Empty;
        public string cliente_nombre { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public sealed class SifRecepcionNdNcPendientesRequest
    {
        public string tipo_documento { get; set; } = string.Empty;
    }

    public sealed class SifRecepcionNdNcAplicarRequest
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string movimiento { get; set; } = "RECEPCION";
        public List<string> documentos { get; set; } = [];
        public string usuario { get; set; } = string.Empty;
    }

    public sealed class SifRecepcionNdNcConsultaRequest
    {
        public string tipo_documento { get; set; } = string.Empty;
        public string cod_transaccion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_fin { get; set; }
    }

    public sealed class SifRecepcionNdNcConsultaData
    {
        public string descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }
}