using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Galileo.Models.TES
{
    public class TesTransaccionesData
    {
        public int total { get; set; }
        public int? minimo { get; set; }
        public int? maximo { get; set; }
        public long docInicial { get; set; }
        public bool docBloqueo { get; set; }
    }

    public class TesEmisionDocFiltros
    {
        public int cantidad { get; set; }
        public int banco { get; set; }
        public string plan { get; set; } = string.Empty;
        public int docInicial { get; set; }
        public string generarPor { get; set; } = string.Empty;
        public string tipoDoc { get; set; } = string.Empty;
        public int minimo { get; set; }
        public int maximo { get; set; }
        public int verificacion { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string? formatoTE { get; set; }
        public string usuario { get; set; } = string.Empty;
        public bool? docBloqueo { get; set; }
    }

    public class TesSolicitudesGenData
    {
        public int nsolicitud { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string beneficiario { get; set; } = string.Empty;
        public string? documento { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime? fecha { get; set; }
        public string? cta_ahorros { get; set; } = string.Empty;
        public DateTime? firmas_autoriza_fecha { get; set; }
        public string? firmas { get; set; } = string.Empty;
        public bool? pass { get; set; }
    }

    public class TesBancoDocsData
    {
        public int doc_auto { get; set; }
        public string comprobante { get; set; } = "";
    }

    public class TesBancoData
    {
        public decimal firmas_desde { get; set; }
        public decimal firmas_hasta { get; set; }
        public string formato_transferencia { get; set; } = string.Empty;
        public string lugar_Emision { get; set; } = string.Empty;
    }

    public class TesEmisionDocumentosResponse
    {
        public TesEmisionDocFiltros? filtro { get; set; }
        public SqlConnection? connection { get; set; }
        public TesBancoDocsData? bancoDocs { get; set; }
        public TesBancoData? bancoData { get; set; }
        public string? queryTransac { get; set; }
        public string? baseQuery { get; set; }
        public object? parametros { get; set; }

        public int vFirmas { get; set; }
    }

    public class ClasificarYGenerarBoletaSiAplicaResponse
    {
        public int? codEmpresa { get; set; }
        public TesEmisionDocFiltros? filtro { get; set; }
        public TesBancoDocsData? bancoDocs { get; set; }
        public TesBancoData? bancoData { get; set; }
        public int vFirmas { get; set; }
        public TesArchivosEspecialesData? chequesReport { get; set; }
        public TesTransaccionDto? item { get; set; }
        public FrmReporteGlobal? reporteData { get; set; }
        public string? reporteCkConFirmas { get; set; }
        public string? reporteCkSinFirmas { get; set; }
        public List<TesTransaccionDto>? listaConFirmas { get; set; }
        public List<TesTransaccionDto>? listaSinFirmas { get; set; }
        public List<TesTransaccionDto>? listaBoleta { get; set; }
        public List<byte[]>? pdfsBoleta { get; set; }
        public FileContentResult? fileResultBoleta { get; set; }
    }

}


