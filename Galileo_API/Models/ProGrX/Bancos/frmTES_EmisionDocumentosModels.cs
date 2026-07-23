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
        public int cantidad { get; set; } = 0;
        public int banco { get; set; } = 0;
        public string plan { get; set; } = string.Empty;
        public int docInicial { get; set; } = 0;
        public string generarPor { get; set; } = string.Empty;
        public string tipoDoc { get; set; } = string.Empty;
        public int minimo { get; set; } = 0;
        public int maximo { get; set; } = 0;
        public int verificacion { get; set; } = 0;
        public DateTime? fecha_inicio { get; set; } = null;
        public DateTime? fecha_corte { get; set; } = null;
        public string? formatoTE { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool? docBloqueo { get; set; } = false;
        public bool especial { get; set; } = false;

        public string? bancoDescripcion { get; set; } = string.Empty;
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
        public string? estadoSinpe { get; set; }
        public int? id_banco { get; set; }
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
        public TesEmisionDocFiltros Filtro { get; init; } = default;
        public TesBancoDocsData BancoDocs { get; init; } = default;
        public TesBancoData BancoData { get; init; } = default;
        public int UsaFirmas { get; init; } // vFirmas
        public TesArchivosEspecialesData ChequesReport { get; init; } = default;
        public FrmReporteGlobal ReporteData { get; init; } = default;
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

    public class TesSolicitudesFormatoRequest
    {
        public int CodEmpresa { get; init; } = 0;
        public TesEmisionDocFiltros Filtro { get; init; } = default;
        public List<TesSolicitudesGenData> Solicitudes { get; init; } = new();
        public long ConsecutivoInterno { get; init; } = 0;
    }

    public sealed class TesEmisionGenerarLoteRequest
    {
        public required int CodEmpresa { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Filtros { get; set; } = string.Empty;   // JSON de filtros base
        public int Minimo { get; set; } = 0;
        public int Maximo { get; set; } = 0;
        public List<int> NSolicitudes { get; set; } = new();
        // Documento único de la emisión (modelo v6: un documento por emisión).
        // Se avanza una sola vez al inicio y se marca en todas las solicitudes del lote.
        public long BancoConsec { get; set; } = 0;
    }

    public sealed class TesEmisionProcesoError
    {
        public int NSolicitud { get; set; }
        public int Codigo { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public sealed class TesEmisionLoteQuery
    {
        public string QueryTransac { get; set; } = string.Empty;
        public string BaseQuery { get; set; } = string.Empty;
    }

    public sealed class TesEmisionGenerarLoteResult
    {
        public int Procesados { get; set; }
        public int ConErrores { get; set; }
        public List<TesEmisionProcesoError> Errores { get; set; } = new();
        // Datos para el paso final (abrir "Procesar Transferencias" que cambia el estado)
        public string BancoConsec { get; set; } = string.Empty;
        public TesEmisionLoteQuery? StrQuery { get; set; }
    }

}


