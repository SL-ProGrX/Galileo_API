namespace Galileo.Models.AH
{
    public class FrmAhExcedentesCapIndCargarResponse
    {
        public List<FrmAhExcedentesCapIndListadoDto> capitalizaciones { get; set; } = [];
        public int lineas { get; set; } = 100;
        public int casos { get; set; } = 0;
    }

    public class FrmAhExcedentesCapIndListaRequest
    {
        public string filtro { get; set; } = string.Empty;
        public int lineas { get; set; } = 100;
    }

    public class FrmAhExcedentesCapIndListadoDto
    {
        public int exc_cap_ind { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal porcentaje { get; set; } = 0;
        public int vencimiento { get; set; } = 0;
    }

    public class FrmAhExcedentesCapIndCedulaDto
    {
        public bool socio_valido { get; set; } = false;
        public bool existe_capitalizacion { get; set; } = false;
        public int exc_cap_ind { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal porcentaje { get; set; } = 0;
        public int vencimiento { get; set; } = 0;
    }

    public class FrmAhExcedentesCapIndGuardarRequest
    {
        public int exc_cap_ind { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public decimal porcentaje { get; set; } = 0;
        public int vencimiento { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesCapIndProcesoResponse
    {
        public int aplicado { get; set; } = 0;
        public int exc_cap_ind { get; set; } = 0;
        public string mensaje { get; set; } = string.Empty;
    }

    internal class FrmAhExcedentesCapIndSocioInternoDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    internal class FrmAhExcedentesCapIndDeleteInternoDto
    {
        public string cedula { get; set; } = string.Empty;
        public decimal porcentaje { get; set; } = 0;
        public int vencimiento { get; set; } = 0;
    }
}