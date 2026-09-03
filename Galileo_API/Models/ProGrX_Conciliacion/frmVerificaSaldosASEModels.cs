using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Conciliacion
{
    public class AseVerificaSaldosInicialData
    {
        public DateTime fecha_corte { get; set; }
        public DateTime fecha_actual { get; set; }
    }

    public class AseVerificaSaldosPeriodoData
    {
        public int id_per_historico { get; set; }
        public int anio { get; set; }
        public int mes { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

    public class AseVerificaSaldosListaRequest
    {
        public string tipo_busqueda { get; set; } = string.Empty;
        public int? id_per_historico { get; set; }
        public bool? excluir_operaciones_nuevas { get; set; }
        public FiltrosLazyLoadData filtros { get; set; } = new();
    }

    public class AseVerificaSaldosListaResult
    {
        public int total { get; set; }
        public List<AseVerificaSaldosData> lista { get; set; } = new();
        public AseVerificaSaldosResumen resumen { get; set; } = new();
    }

    public class AseVerificaSaldosData
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal saldo_inicial { get; set; }
        public decimal saldo_final { get; set; }
        public decimal debitos { get; set; }
        public decimal creditos { get; set; }
        public decimal diferencia { get; set; }
    }

    public class AseVerificaSaldosResumen
    {
        public int procesados { get; set; }
        public int total_evaluados { get; set; }
        public int diferencias { get; set; }
        public decimal porcentaje { get; set; }
        public decimal total_saldo_inicial { get; set; }
        public decimal total_saldo_final { get; set; }
        public decimal total_debitos { get; set; }
        public decimal total_creditos { get; set; }
        public decimal total_diferencia { get; set; }
    }
}