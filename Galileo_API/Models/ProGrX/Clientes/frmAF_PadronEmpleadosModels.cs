namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AfPadronEmpleadosFiltro
    {
        public List<object>? estado { get; set; }
        public List<object>? institucion { get; set; }
        public DateTime? ing_fecha_inicio { get; set; }
        public DateTime? ing_fecha_corte { get; set; }
        public bool ing_chk_fecha { get; set; }
        public DateTime? reg_fecha_inicio { get; set; }
        public DateTime? reg_fecha_corte { get; set; }
        public bool reg_chk_fecha { get; set; }
    }

    public class AfPadronEmpleadosDto
    {
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public int institucion { get; set; }
        public string? departamento { get; set; }
        public string? seccion { get; set; }
        public string? id_alterno { get; set; }
        public DateTime? fecha_ingreso { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public string? estadopersona { get; set; }
    }
}