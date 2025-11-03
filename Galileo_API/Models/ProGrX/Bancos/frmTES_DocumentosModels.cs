namespace PgxAPI.Models.ProGrX.Bancos
{
    public class TesDocAnulaConceptosData
    {
        public int id_conceptos { get; set; }
        public string descripcion { get; set; }
        public bool activo { get; set; }
    }

    public class TesTiposDocDTO
    {
        public string tipo { get; set; }
        public string descripcion { get; set; }
        public string movimiento { get; set; }
        public bool generacion { get; set; }

        public string tipo_asiento { get; set; }
        public string? tipo_asiento_desc { get; set; }

        public bool asiento_transac { get; set; }
        public bool asiento_banco { get; set; }
        public bool asiento_formato { get; set; }
        public string asiento_mascara { get; set; }
        public string registro_usuario { get; set; }
        public DateTime registro_fecha { get; set; }

        public bool int_reclasifica_id { get; set; }
    }

    public class TesDocAnulaConcepRespuesta
    {
        public int codigo { get; set; }
        public string mensaje { get; set; }
        public string movimiento { get; set; }
        public int pass { get; set; }
    }
}
