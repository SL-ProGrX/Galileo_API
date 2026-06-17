namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrConstanciasInicialDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime? corte { get; set; }
        public string emitido_por { get; set; } = string.Empty;
        public string puesto { get; set; } = string.Empty;
        public List<CrConstanciasCuentaIbanDto> cuentas_iban { get; set; } = new();
        public List<CrConstanciasParentescoDto> parentescos { get; set; } = new();
        public List<CrConstanciasCicloDto> ciclos { get; set; } = new();
    }

    public class CrConstanciasCuentaIbanDto
    {
        public string cedula { get; set; } = string.Empty;
        public string cuenta_cliente { get; set; } = string.Empty;
        public string iban { get; set; } = string.Empty;
        public string iban_mask { get; set; } = string.Empty;
        public string IdX { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
    }

    public class CrConstanciasParentescoDto
    {
        public string IdX { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
    }

    public class CrConstanciasCicloDto
    {
        public string IdX { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
    }

    public class CrConstanciasEducacionDto
    {
        public string IdX { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string ItmX { get; set; } = string.Empty;
    }

    public class CrConstanciasPadronDto
    {
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CrConstanciasBitacoraRequest
    {
        public string gestion { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrConstanciasReporteRequest
    {
        public string tipo_reporte { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime? corte { get; set; }
        public string dirigido_a { get; set; } = string.Empty;
        public string emitido_por { get; set; } = string.Empty;
        public string puesto { get; set; } = string.Empty;
        public bool usa_identificacion_alterna { get; set; }
        public string iban { get; set; } = string.Empty;
        public string iban_texto { get; set; } = string.Empty;
        public string universidad { get; set; } = string.Empty;
        public string universidad_desc { get; set; } = string.Empty;
        public string nivel { get; set; } = string.Empty;
        public string nivel_desc { get; set; } = string.Empty;
        public string carrera { get; set; } = string.Empty;
        public string carrera_desc { get; set; } = string.Empty;
        public string especialidad { get; set; } = string.Empty;
        public string especialidad_desc { get; set; } = string.Empty;
        public string beneficiario_id { get; set; } = string.Empty;
        public string beneficiario { get; set; } = string.Empty;
        public string parentesco { get; set; } = string.Empty;
        public string parentesco_desc { get; set; } = string.Empty;
        public string ciclo { get; set; } = string.Empty;
        public string ciclo_anio { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
    public class CrConstanciasPadronBusquedaDto
    {
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }
}