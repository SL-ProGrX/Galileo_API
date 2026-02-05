namespace Galileo_API.Models.ProGrX_Nucleo
{
    public class AfiliacionFiltroDto
    {
        public string? cedula { get; set; }
        public string? id_alterno { get; set; }
        public string? nombre { get; set; }
        public string? estado { get; set; }
        public string? tipo_fecha { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
    }

    public class AfiliacionTablaDto
    {
        public long? solicitud_id { get; set; }
        public string? estado_desc { get; set; }
        public string? cedula { get; set; }
        public string? id_colilla { get; set; }
        public string? apellido_1 { get; set; }
        public string? apellido_2 { get; set; }
        public string? nombre { get; set; }
        public string? fecha_nac { get; set; }
        public string? estadocivil_desc { get; set; }
        public string? sexo_desc { get; set; }
        public string? nacionalidad_desc { get; set; }
        public string? institucion_desc { get; set; }
        public string? tel_movil { get; set; }
        public string? tel_habitacion { get; set; }
        public string? tel_trabajo { get; set; }
        public string? email_01 { get; set; }
        public string? email_02 { get; set; }
        public string? provincia_desc { get; set; }
        public string? canton_desc { get; set; }
        public string? distrito_desc { get; set; }
        public string? direccion { get; set; }
        public DateTime? registro_fecha { get; set; }
        public DateTime? resuelto_fecha { get; set; }
        public string? resuelto_usuario { get; set; }
        public bool? i_poliza_vida_familiar { get; set; }
        public bool? i_autorizacion_deduc { get; set; }
    }

    public class AfiliacionCasoDto
    {
        public long? solicitud_id { get; set; }
        public string? estado { get; set; }
        public string? estado_desc { get; set; }
        public string? cedula { get; set; }
        public string? id_alterno { get; set; }
        public string? apellido1 { get; set; }
        public string? apellido2 { get; set; }
        public string? nombre { get; set; }
        public DateTime? fecha_nac { get; set; }
        public string? estado_civil { get; set; }
        public string? genero { get; set; }
        public string? nacionalidad { get; set; }
        public string? tel_movil { get; set; }
        public string? tel_habitacion { get; set; }
        public string? tel_trabajo { get; set; }
        public string? email_01 { get; set; }
        public string? email_02 { get; set; }
        public string? provincia { get; set; }
        public string? canton { get; set; }
        public string? distrito { get; set; }
        public string? direccion { get; set; }
        public string? empresa { get; set; }
        public DateTime? fecha_ingreso_empresa { get; set; }
        public bool? poliza { get; set; }
    }

    public class AfiliacionResumenDto
    {
        public string? estado { get; set; }
        public int? casos { get; set; }
    }
}
