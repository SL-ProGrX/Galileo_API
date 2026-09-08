using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Conciliacion
{
    public class AseVerificaAsientosDocumentoInicialData
    {
        public DateTime fecha_servidor { get; set; }
    }
    public class AseVerificaAsientosDocumentoListaRequest
    {
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public FiltrosLazyLoadData? filtros { get; set; } = new();
    }
    public class AseVerificaAsientosDocumentoData
    {
        public DateTime? fecha { get; set; }
        public string tipo_documento { get; set; } = string.Empty;
        public string cod_transaccion { get; set; } = string.Empty;
        public decimal debitos { get; set; }
        public decimal creditos { get; set; }
        public decimal diferencia { get; set; }
        public string? concepto_desc { get; set; }
        public string? registro_usuario { get; set; }
    }
    public class AseVerificaAsientosDocumentoListaResult
    {
        public int total { get; set; }
        public List<AseVerificaAsientosDocumentoData> lista { get; set; } = new();
    }
}