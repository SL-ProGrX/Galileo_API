using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrRemesasCreditoListaRequest
    {
        public FiltrosLazyLoadData filtros { get; set; } = new();
        public CrRemesasCreditoFiltroRequest filtro { get; set; } = new();
    }

    public class CrRemesasCreditoFiltroRequest
    {
        public int fuente { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string estado { get; set; } = string.Empty;
        public string cod_grupo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string cod_destino { get; set; } = string.Empty;
        public string cod_oficina { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public bool creditos_no_revisados { get; set; }
    }

    public class CrRemesasCreditoLista
    {
        public int total { get; set; }
        public List<CrRemesasCreditoData> lista { get; set; } = new();
    }

    public class CrRemesasCreditoData
    {
        public bool seleccionado { get; set; }
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public decimal monto { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public long referencia { get; set; }
    }

    public class CrRemesasCreditoCrearRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string tag_codigo { get; set; } = string.Empty;
        public List<CrRemesasCreditoCrearDetalle> operaciones { get; set; } = new();
    }

    public class CrRemesasCreditoCrearDetalle
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public long referencia { get; set; }
    }

    public class CrRemesasCreditoCrearResult
    {
        public int remesa { get; set; }
        public int cantidad { get; set; }
    }

    public class CrRemesasCreditoTagLista
    {
        public int total { get; set; }
        public List<CrRemesasCreditoTagData> lista { get; set; } = new();
    }

    public class CrRemesasCreditoTagData
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public short activo { get; set; }
        public int consecutivo { get; set; }
        public bool isNew { get; set; }
    }

    public class CrRemesasCreditoTagGuardarRequest
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public short? activo { get; set; }
        public int? consecutivo { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrRemesasCreditoInformeListaRequest
    {
        public FiltrosLazyLoadData filtros { get; set; } = new();
        public CrRemesasCreditoInformeFiltroRequest filtro { get; set; } = new();
    }

    public class CrRemesasCreditoInformeFiltroRequest
    {
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public bool todas_fechas { get; set; }
        public string tag_codigo { get; set; } = string.Empty;
        public int tag_consecutivo { get; set; }
        public int top { get; set; } = 15;
    }

    public class CrRemesasCreditoInformeLista
    {
        public int total { get; set; }
        public List<CrRemesasCreditoInformeData> lista { get; set; } = new();
    }

    public class CrRemesasCreditoInformeData
    {
        public int remesa { get; set; }
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? microfilm_fecha { get; set; }
        public string microfilm_usuario { get; set; } = string.Empty;
        public string tag_codigo { get; set; } = string.Empty;
        public int tag_consecutivo { get; set; }
    }

    public class CrRemesasCreditoArchivoDigitalRequest
    {
        public int? remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrRemesasCreditoArchivoDigitalDto
    {
        public int remesa { get; set; }
        public string microfilm_usuario { get; set; } = string.Empty;
        public DateTime? microfilm_fecha { get; set; }
    }

    public class CrRemesasCreditoConsultaDto
    {
        public int remesa { get; set; }
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string texto { get; set; } = string.Empty;
    }

    public class CrRemesasCreditoListadoCargaRequest
    {
        public List<long> operaciones { get; set; } = new();
    }

    public class CrRemesasCreditoListadoCargaResult
    {
        public int total { get; set; }
        public List<CrRemesasCreditoListadoCargaData> lista { get; set; } = new();
    }

    public class CrRemesasCreditoListadoCargaData
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime? fecha { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int? remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? microfilm_fecha { get; set; }
        public string microfilm_usuario { get; set; } = string.Empty;
        public string tag_descripcion { get; set; } = string.Empty;
        public int? tag_consecutivo { get; set; }
        public string estado { get; set; } = string.Empty;
    }

    public class CrRemesasCreditoReporteRequest
    {
        public int? remesa { get; set; }
        public string tipo_reporte { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrRemesasCreditoReporteDto
    {
        public string nombre_reporte { get; set; } = string.Empty;
        public string titulo { get; set; } = string.Empty;
        public string subtitulo { get; set; } = string.Empty;
        public string filtro { get; set; } = string.Empty;
        public int remesa { get; set; }
    }
}