namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AF_CRAutorizacion
    {
        public int Cod_Renuncia { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public DateTime Resuelto_Fecha { get; set; }
        public DateTime Vencimiento { get; set; }
        public string Tipo { get; set; }
        public string Estado { get; set; }
        public string Resuelto_User { get; set; }
        public string Autoriza_Notas { get; set; }
    }

    public class AF_CRAutorizacionFiltros
    {
        public DateTime? Inicio { get; set; }
        public DateTime? Corte { get; set; }
        public string EstadoAutorizacion { get; set; } 
    }
}
