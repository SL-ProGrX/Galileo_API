namespace Galileo_API.Models.ProGrX_Polizas
{
    public class PolizaLookupResponseDto
    {
        public string CodPoliza { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public class PolizaScrollRequestDto
    {
        public string CodEmpresa { get; set; } = string.Empty;
        public string CodPolizaActual { get; set; } = string.Empty;
        public int Direccion { get; set; } // 1 = siguiente, -1 = anterior
    }
}
