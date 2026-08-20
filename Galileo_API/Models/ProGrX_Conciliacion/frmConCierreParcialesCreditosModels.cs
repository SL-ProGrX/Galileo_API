namespace Galileo_API.Models.ProGrX_Conciliacion
{
    public class ConCierreParcialesCreditosUltimoCorteData
    {
        public DateTime? Corte { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
    }

    public class ConCierreParcialesCreditosCierreParcialRequest
    {
        public DateTime Fecha_Corte { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class ConCierreParcialesCreditosProyeccionRequest
    {
        public DateTime Fecha_Inicio { get; set; }
        public short Meses { get; set; }
    }

    public class ConCierreParcialesCreditosProductoAcumuladoRequest
    {
        public DateTime Fecha_Corte { get; set; }
    }
}
