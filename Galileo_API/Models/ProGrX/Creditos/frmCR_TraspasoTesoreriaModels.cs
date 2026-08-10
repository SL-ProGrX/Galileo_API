namespace Galileo.Models.ProGrX.Credito
{
    public class TraspasoModel
    {
        public int id_solicitud { get; set; }
        public string? codigo { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public decimal montoapr { get; set; }
        public decimal monto_girado { get; set; }
        public decimal desembolsos_numero { get; set; }
        public string? desembolsos { get; set; }
    }

    public class RemesaModel
    {
        public int cod_remesa { get; set; }
        public string? usuario { get; set; }
        public DateTime fecha { get; set; }
        public string? estado { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string? notas { get; set; }
        public int casos { get; set; }
        public decimal monto { get; set; }
    }

    public class RemesaRequest
    {
        [System.Text.Json.Serialization.JsonRequired]
        public int cod_remesa { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public DateTime fecha_inicio { get; set; }
        [System.Text.Json.Serialization.JsonRequired]
        public DateTime fecha_corte { get; set; }
        public string? notas { get; set; }
    }

    public class CargaOperacionModel
    {
        public int id_solicitud { get; set; }
        public string? codigo { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public decimal montoapr { get; set; }
        public decimal monto_girado { get; set; }
        public int desem_num { get; set; }
        public decimal desem_monto { get; set; }
        public decimal total { get; set; }
        public int duplicado { get; set; }
    }

    public class CargaRequest
    {
        [System.Text.Json.Serialization.JsonRequired]
        public int cod_remesa { get; set; }
        public List<int> operaciones { get; set; } = new();
    }

    public class ReactivacionModel
    {
        public int id_solicitud { get; set; }
        public string? codigo { get; set; }
        public string? descripcion_linea { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public decimal monto_girado { get; set; }
        public string? emitir { get; set; }
        public bool permitido { get; set; }
        public string? motivo { get; set; }
    }

    public class ConsultaModel
    {
        public int id_solicitud { get; set; }
        public string? codigo { get; set; }
        public string? descripcion_linea { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public decimal monto_girado { get; set; }
        public int? cod_remesa { get; set; }
        public string? estado_remesa { get; set; }
        public DateTime? fecha_remesa { get; set; }
        public string? usuario_remesa { get; set; }
        public decimal? monto_remesa { get; set; }
        public decimal? desembolsos_remesa { get; set; }
    }

    public class CambioConceptoModel
    {
        public int id_desembolso { get; set; }
        public int id_solicitud { get; set; }
        public string? codigo { get; set; }
        public decimal monto { get; set; }
        public string? concepto { get; set; }
    }

    public class CambioConceptoRequest
    {
        [System.Text.Json.Serialization.JsonRequired]
        public int id_desembolso { get; set; }
        public string? concepto { get; set; }
    }
}