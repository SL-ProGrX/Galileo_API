namespace Galileo_API.Models.ProGrX_Polizas
{
    public sealed class CcPolizaBeneficiarioDto
    {
        public string cedula { get; init; } = string.Empty;
        public string nombre { get; init; } = string.Empty;
        public decimal porcentaje { get; init; }
        public string tipo_id_desc { get; init; } = string.Empty;
        public int tipo_id { get; init; }
        public string parentesco_desc { get; init; } = string.Empty;
        public string cod_parentesco { get; init; } = string.Empty;
    }

    public sealed class CcPolizaBeneficiarioGuardarDto
    {
        public int tipo_id { get; init; }
        public string identificacion { get; init; } = string.Empty;
        public string nombre { get; init; } = string.Empty;
        public string cod_parentesco { get; init; } = string.Empty;
        public decimal porcentaje { get; init; }
    }

    public sealed class CcPolizaBeneficiariosGuardarRequest
    {
        public string cedula { get; init; } = string.Empty;
        public string cod_poliza { get; init; } = string.Empty;
        public List<CcPolizaBeneficiarioGuardarDto> beneficiarios { get; init; } = [];
    }

    public sealed class CcPolizaBeneficiariosCatalogosDto
    {
        public List<CcPolizaBeneficiariosListaDto> polizas { get; init; } = [];
        public List<CcPolizaBeneficiariosListaDto> tipos_id { get; init; } = [];
        public List<CcPolizaBeneficiariosListaDto> parentescos { get; init; } = [];
    }

    public sealed class CcPolizaBeneficiariosListaDto
    {
        public string item { get; init; } = string.Empty;
        public string descripcion { get; init; } = string.Empty;
    }

    public sealed class CcPolizaBeneficiariosPadronDto
    {
        public string apellido_1 { get; init; } = string.Empty;
        public string apellido_2 { get; init; } = string.Empty;
        public string nombre { get; init; } = string.Empty;
    }
}
