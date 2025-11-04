namespace PgxAPI.Models.Security
{
    public class ServicioSuscripcion
    {
        public string Cod_Servicio { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Aplica_Por_Usuario { get; set; }
        public string PorUsuario { get; set; } = string.Empty;
        public double Costo { get; set; }
        public int Activo { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime Registro_Fecha { get; set; }
    }
}
