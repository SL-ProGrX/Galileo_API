namespace Galileo_API.Models
{
    public class MProcesoMensualModels
    {
        public class ProcesoMensualFndAsientoRequest
        {
            public decimal Proceso { get; set; }
            public int CodInstitucion { get; set; }
            public int Operadora { get; set; }
            public string Plan { get; set; } = string.Empty;
            public string Cuenta { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public string NumeroDocumento { get; set; } = string.Empty;
        }
    }
}
