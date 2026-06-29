namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrCatalogoCopiaLineaDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrCatalogoCopiaDescripcionDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrCatalogoCopiaScrollDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrCatalogoCopiaFlagsDto
    {
        public bool general { get; set; } = false;
        public bool cuentas { get; set; } = false;
        public bool rangos { get; set; } = false;
        public bool destinos { get; set; } = false;
        public bool cargos { get; set; } = false;
        public bool recursos { get; set; } = false;
        public bool requisitos { get; set; } = false;
        public bool cobro { get; set; } = false;
        public bool resolucion { get; set; } = false;
        public bool refundibles { get; set; } = false;
        public bool adjuntos { get; set; } = false;
    }

    public class CrCatalogoCopiaNuevaLineaDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrCatalogoCopiaRequest
    {
        public string linea_origen { get; set; } = string.Empty;
        public List<string> lineas_destino { get; set; } = new();
        public CrCatalogoCopiaNuevaLineaDto? nueva_linea { get; set; }
        public CrCatalogoCopiaFlagsDto flags { get; set; } = new();
        public string usuario { get; set; } = string.Empty;
    }

    public class CrCatalogoCopiaResultadoItemDto
    {
        public string linea_destino { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool es_nueva { get; set; } = false;
        public bool procesada { get; set; } = false;
        public string mensaje { get; set; } = string.Empty;
    }

    public class CrCatalogoCopiaResultadoDto
    {
        public int total_procesadas { get; set; } = 0;
        public List<CrCatalogoCopiaResultadoItemDto> detalle { get; set; } = new();
    }
}