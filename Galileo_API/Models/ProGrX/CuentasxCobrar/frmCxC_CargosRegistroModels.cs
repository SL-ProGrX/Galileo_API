namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCCargosRegistroOperacionData
    {
        public int operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string num_documento { get; set; } = string.Empty;
        public string desc_proceso { get; set; } = string.Empty;
    }

    public class CxCCargosRegistroCargoData
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
    }

    public class CxCCargosRegistroCargoReposicionData
    {
        public decimal cargo { get; set; } = 0;
    }

    public class CxCCargosRegistroAplicarRequest
    {
        public int operacion { get; set; } = 0;
        public string cod_cargo { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
        public bool cargo_reposicion { get; set; } = false;
    }
}