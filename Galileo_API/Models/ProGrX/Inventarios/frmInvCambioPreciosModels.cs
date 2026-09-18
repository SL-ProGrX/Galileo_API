namespace Galileo.Models.INV
{
    public class CambioPrecioArchivoDetalleDto
    {
        public string codigo { get; set; } = string.Empty;
        public decimal precio { get; set; } = 0m;
    }

    public class CambioPrecioArchivoCargaRequest
    {
        public string tipo_precio { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public List<CambioPrecioArchivoDetalleDto> registros { get; set; } = [];
    }

    public class CambioPrecioArchivoResultadoDto
    {
        public string llave_01 { get; set; } = string.Empty;
        public string ref_01 { get; set; } = string.Empty;
        public decimal monto_01 { get; set; } = 0m;
        public decimal monto_02 { get; set; } = 0m;
        public string detalle { get; set; } = string.Empty;
    }

    public class CambioPrecioArchivoCargaResponse
    {
        public List<CambioPrecioArchivoResultadoDto> registros { get; set; } = [];
        public int casos { get; set; } = 0;
        public int no_existen { get; set; } = 0;
        public decimal monto_total { get; set; } = 0m;
    }

    public class CambioPrecioArchivoProcesarRequest
    {
        public string tipo_precio { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public List<CambioPrecioArchivoDetalleDto> registros { get; set; } = [];
    }

    public class FacturaPrecioDetalleDto
    {
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal porc_utilidad { get; set; } = 0m;
        public decimal costo_regular { get; set; } = 0m;
        public decimal precio_regular_impuesto { get; set; } = 0m;
        public decimal precio_factura { get; set; } = 0m;
        public decimal nuevo_precio { get; set; } = 0m;
    }

    public class FacturaPrecioActualizarDto
    {
        public string cod_producto { get; set; } = string.Empty;
        public decimal nuevo_precio { get; set; } = 0m;
    }

    public class CambioPrecioFacturaActualizarRequest
    {
        public List<FacturaPrecioActualizarDto> registros { get; set; } = [];
    }
}