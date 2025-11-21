namespace Galileo.Models.ProGrX_Personas
{
    public class AfNatAutorizacion
    {
        public int Cod_Renuncia { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime? FechaIngreso { get; set; }
        public string? Tipo { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public DateTime? Vencimiento { get; set; }
        public string? Estado { get; set; }
        public string Causa_Desc { get; set; } = string.Empty;
        public string Estado_Desc { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class AfNatAutorizacionFiltros
    {
        public DateTime? Inicio { get; set; }
        public DateTime? Corte { get; set; }
        public string Filtro { get; set; } = string.Empty;
        public string FUserReg { get; set; } = string.Empty;
        public string TUsuario { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int Autorizadas { get; set; } 
    }
}