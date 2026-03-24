using Galileo.Models;

namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCCuentasCargoData
    {
        public int? operacion { get; set; }
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal? monto { get; set; }
        public string tipo { get; set; } = string.Empty;
        public decimal? valor { get; set; }
        public short? modifica { get; set; }
        public string detalle { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public bool? isNew { get; set; }
    }

    public class CxCCuentasCargoOperacionDto
    {
        public int operacion { get; set; }
        public string cedula { get; set; } = string.Empty;
        public decimal monto_operacion { get; set; }
        public decimal rebajos_totales { get; set; }
        public decimal ingresos_totales { get; set; }
        public List<CxCCuentasCargoData> lista { get; set; } = new();
        public int total { get; set; }
    }

    public class CxCCuentasCargosListaResult
    {
        public int total { get; set; }
        public List<CxCCuentasCargoData> lista { get; set; } = new();
    }

    public class CxCCuentasCargoDisponibleDto
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CxCCuentasCargoGuardarRequest
    {
        public CxCCuentasCargoData cargo { get; set; } = new();
    }

    public class CxCCuentasCargoEliminarRequest
    {
        public int? operacion { get; set; }
        public string cod_cargo { get; set; } = string.Empty;
    }
}