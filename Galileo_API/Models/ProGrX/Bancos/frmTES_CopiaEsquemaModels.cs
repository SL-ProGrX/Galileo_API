using System.ComponentModel.DataAnnotations;

namespace PgxAPI.Models.ProGrX.Bancos
{
    public class tesCopiaEsquemaLista
    {
        public int total { get; set; } = 0;
        public List<tesCopiaEsquemaModels> lista { get; set; } = new List<tesCopiaEsquemaModels>();
    }

    public class tesCopiaEsquemaModels
    {
        public int? nsolicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string beneficiario { get; set; } = string.Empty;
        public float monto { get; set; } = 0;
        public DateTime fecha_Solicitud  { get; set; }
        public string tipo  { get; set; } = string.Empty;
        public int id_Banco  { get; set; } = 0;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public string unidadDesc { get; set; } = string.Empty;
        public string conceptoDesc { get; set; } = string.Empty;
        public string tDocumento { get; set; } = string.Empty;
        public string bancoDesc  { get; set; } = string.Empty;
        public int solicitud { get; set; } = 0;
        [MaxLength(500)]
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;

        public string cuentaIBAN { get; set; } = string.Empty;
        public string cuentaOrigen { get; set; } = string.Empty;
        public string correo { get; set; } = string.Empty;
        public string divisa { get; set; } = string.Empty;
        public string tipoId { get; set; } = string.Empty;

        public string? detalle1 { get; set; }
        public string? detalle2 { get; set; }
        public string? detalle3 { get; set; }
        public string? detalle4 { get; set; }
        public string? detalle5 { get; set; }
    }
}
