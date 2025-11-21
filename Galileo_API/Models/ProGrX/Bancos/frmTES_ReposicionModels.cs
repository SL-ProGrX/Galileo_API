namespace Galileo.Models.ProGrX.Bancos
{
    public class TesReposicionData
    {
        public int nSolicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string beneficiario { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public int id_banco { get; set; } = 0;
        public string bancoX { get; set; } = string.Empty;
        public string tipoDocX { get; set; } = string.Empty;
        public float monto { get; set; } = 0.0f;
        public Nullable<DateTime> fecha_Emision { get; set; } = null;
        public string tipo_Beneficiario { get; set; } = string.Empty;
        public string cta_Ahorros { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string tipoBeneficiario { get; set; } = string.Empty;
        public int reposicionPaso { get; set; } = 0;
        public string verifica { get; set; } = string.Empty;
        public string verificaTag { get; set; } = "N";
        public string usuario { get; set; } = string.Empty;
        public string clave { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }
}
