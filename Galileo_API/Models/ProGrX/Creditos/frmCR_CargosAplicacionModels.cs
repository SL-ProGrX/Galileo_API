namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrCargosAplicacionCargoData
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal valor { get; set; } = 0;
    }

    public class CrCargosAplicacionOperacionData
    {
        public int operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string linea_desc { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public int opex { get; set; } = 0;
    }

    public class CrCargosAplicacionAplicarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public int operacion { get; set; } = 0;
        public string cod_cargo { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
    }
}