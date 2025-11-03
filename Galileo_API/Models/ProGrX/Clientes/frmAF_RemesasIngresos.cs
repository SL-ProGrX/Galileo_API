namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AFI_RemesaIngDTO
    {
        public int cod_remesa { get; set; }
        public DateTime fecha { get; set; }
        public string usuario { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string notas { get; set; }
        public string estado { get; set; }
        public DateTime? microfilm_fecha { get; set; }
        public string microfilm_usuario { get; set; }
    }

    public class AFI_RemesaIngRequestDTO
    {
        public int CodRemesa { get; set; }
        public string Usuario { get; set; } = "";
        public DateTime FechaInicio { get; set; }
        public DateTime FechaCorte { get; set; }
        public string Notas { get; set; } = "";
        public string Estado { get; set; } = ""; // "A" o "C"
    }

    public class IngresosPendientesDTO
    {
        public int consec { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime fecha_ingreso { get; set; }
    }

    public class RemesaConsultaDTO
    {
        public int CodRemesa { get; set; }
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
    }
}
