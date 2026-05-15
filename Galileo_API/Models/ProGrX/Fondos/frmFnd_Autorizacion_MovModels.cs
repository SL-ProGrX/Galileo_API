namespace Galileo.Models.ProGrX.Fondos
{
    public class FndAutorizacionMovFiltros
    {
        public string usuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; } = null;
        public DateTime? fecha_corte { get; set; } = null;

        public string logUsuario { get; set; } = string.Empty;

    }

    public class FndAutorizacionMovData
    {
        public int id_autorizacion { get; set; } = 0;
        public string estado_desc { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string tipo_desc { get; set; } = string.Empty;
        public decimal monto_calculado { get; set; } = 0;
        public decimal monto_solicitado { get; set; } = 0;
        public decimal monto_dif { get; set; } = 0;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; } = null;
        public string cod_plan { get; set; } = string.Empty;
        public string cod_contrato { get; set; } = string.Empty;
        public string resuelve_usuario { get; set; } = string.Empty;
        public DateTime? resuelve_fecha { get; set; } = null;
        public string aplica_usuario { get; set; } = string.Empty;
        public DateTime? aplica_fecha { get; set; } = null;
        public string tcon { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string plan_desc { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;  // Incluido para representar el valor usado en el Select Case
    }
}
