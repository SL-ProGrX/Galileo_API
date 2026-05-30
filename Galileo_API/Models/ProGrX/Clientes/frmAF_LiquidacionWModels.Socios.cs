namespace Galileo.Models.ProGrX.Clientes
{
    public class AfLiquidacionRenunciaSinLiquidar
    {
        public string Cedula { get; set; } = string.Empty;
        public string Id_Alterno { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class AfLiquidacionSocio
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    public class AfLiquidacionSocioDetalle
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaIngreso { get; set; }
        public string EstadoActual { get; set; } = string.Empty;
        public int Boleta { get; set; }
        public string EstadoPersona { get; set; } = string.Empty;
    }

    public class AfLiquidacionSocioExiste
    {
        public int Existe { get; set; }
    }

    public class AfSocioDatosBasicos
    {
        public string Cedula { get; set; } = string.Empty;
        public int Nacta { get; set; }
        public int Id_Promotor { get; set; }
        public int Id_Boleta_Af { get; set; }
    }
}