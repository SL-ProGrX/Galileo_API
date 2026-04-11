namespace Galileo_API.Models.ProGrX.Cobros
{
    public class OperacionBusquedaDto
    {
        public int operacion { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public decimal montoapr { get; set; }
        public decimal saldo { get; set; }
    }

    public class OperacionConsultarDto
    {
        public int operacion { get; set; }
        public string descripcion { get; set; } = string.Empty; // NORMAL
        public string estado { get; set; } = string.Empty;      // NO

        public int codInstitucion { get; set; }

        public string deductora { get; set; }

        public string linea { get; set; } = string.Empty;
        public string lineaDescripcion { get; set; } = string.Empty;

        public string identificacion { get; set; } = string.Empty;
        public string identificacionDescripcion { get; set; } = string.Empty;
    }
}
