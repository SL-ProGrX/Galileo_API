namespace Galileo_API.Models.ProGrX.Creditos
{
    public class FrecuenciaReductora
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Frecuencia_Id { get; set; } = string.Empty;
    }

    public class CrCreditosCargaLoteCargadoInsertarRequest
    {
        public int Linea { get; set; } = 0;
        public string Codigo { get; set; } = string.Empty;
        public string Cod_Referencia { get; set; } = string.Empty;
        public long Proceso { get; set; } = 0;
        public string Cedula { get; set; } = string.Empty;
        public decimal Monto { get; set; } = 0;
        public string Nombre { get; set; } = string.Empty;
        public int Plazo { get; set; } = 0;
        public decimal Comision { get; set; } = 0;
        public string Documento { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
    }

    public class CrCreditosCargaLoteCargadoRevisadoRequest
    {
        public string ClienteId { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public int Proceso { get; set; } = 0;
        public int Banco { get; set; } = 0;
        public string? Destino { get; set; }
    }

    public class CrCreditosCargaLoteCargadoRevisadoResponse
    {
        public int Linea { get; set; } = 0;
        public string Codigo { get; set; } = string.Empty;
        public string Cod_Referencia { get; set; } = string.Empty;
        public int Proceso { get; set; } = 0;
        public string Cedula { get; set; } = string.Empty;
        public decimal Monto { get; set; } = 0;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public int Plazo { get; set; } = 0;
        public decimal Tasa { get; set; } = 0;
        public decimal Cuota { get; set; } = 0;
        public decimal Comision { get; set; } = 0;
        public string Documento { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string Cta_Bancos { get; set; } = string.Empty;
    }

    public class ProveedorCxpModel
    {
        public string Cod_Proveedor { get; set; } = string.Empty;
        public string CedJur { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class CrCreditosCargaLoteProcesaRequest
    {
        public string Codigo_Linea { get; set; } = string.Empty;
        public decimal Proceso { get; set; } = 0;
        public string Tipo_Documento { get; set; } = string.Empty;
        public decimal Pri_Deduc { get; set; } = 0;
        public int Banco { get; set; } = 0;
        public int? Proveedor { get; set; }
        public string Comision_Ref { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Cod_Destino { get; set; } = string.Empty;
        public string Aplicacion { get; set; } = string.Empty;
    }
    
}
