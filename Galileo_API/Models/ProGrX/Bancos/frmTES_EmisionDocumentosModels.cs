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


    internal sealed class EmisionClasificacionRequest
    {
        public int CodEmpresa { get; init; }
        public TesEmisionDocFiltros Filtro { get; init; } = default!;
        public TesBancoDocsData BancoDocs { get; init; } = default!;
        public TesBancoData BancoData { get; init; } = default!;
        public int UsaFirmas { get; init; } // vFirmas
        public TesArchivosEspecialesData ChequesReport { get; init; } = default!;
        public FrmReporteGlobal ReporteData { get; init; } = default!;
    }

    internal sealed class EmisionClasificacionState
    {
        public string ReporteCkConFirmas { get; set; } = string.Empty;
        public string ReporteCkSinFirmas { get; set; } = string.Empty;

        public List<TesTransaccionDto> ListaConFirmas { get; } = new();
        public List<TesTransaccionDto> ListaSinFirmas { get; } = new();

        public List<TesTransaccionDto> ListaBoleta { get; } = new();
        public List<byte[]> PdfsBoleta { get; } = new();

        // Si tu flujo necesita conservar “el último file result”
        public FileContentResult? FileResultBoleta { get; set; }
    }

}


