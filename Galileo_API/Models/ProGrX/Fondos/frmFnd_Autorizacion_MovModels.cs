namespace Galileo.Models.ProGrX.Fondos
{
    public class FndAutorizacionMovFiltros
    {
        public string Usuario { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime? Fecha_Inicio { get; set; } = null;
        public DateTime? Fecha_Corte { get; set; } = null;

        public string LogUsuario { get; set; } = string.Empty;

    }

    public class FndAutorizacionMovData
    {
        public int Id_Autorizacion { get; set; } = 0;
        public string Estado_Desc { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo_Desc { get; set; } = string.Empty;
        public decimal Monto_Calculado { get; set; } = 0;
        public decimal Monto_Solicitado { get; set; } = 0;
        public decimal Monto_Dif { get; set; } = 0;
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; } = null;
        public string Cod_Plan { get; set; } = string.Empty;
        public string Cod_Contrato { get; set; } = string.Empty;
        public string Resuelve_Usuario { get; set; } = string.Empty;
        public DateTime? Resuelve_Fecha { get; set; } = null;
        public string Aplica_Usuario { get; set; } = string.Empty;
        public DateTime? Aplica_Fecha { get; set; } = null;
        public string Tcon { get; set; } = string.Empty;
        public string Ncon { get; set; } = string.Empty;
        public string Plan_Desc { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Nota_Solicita { get; set; } = string.Empty;
        public string Nota_Resolucion { get; set; } = string.Empty;
    }
}
