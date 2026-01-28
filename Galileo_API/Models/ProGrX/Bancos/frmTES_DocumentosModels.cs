namespace Galileo.Models.ProGrX.Bancos
{
    public class TesDocAnulaConceptosData
    {
        public int id_conceptos { get; set; } = 0;
        public string? descripcion { get; set; }
        public bool activo { get; set; } = false;
    }

    public class TesTiposDocDto
    {
        public string? tipo { get; set; }
        public string? descripcion { get; set; }
        public string? movimiento { get; set; }
        public bool generacion { get; set; } = false;
        public string? tipo_asiento { get; set; }
        public string? tipo_asiento_desc { get; set; }
        public bool asiento_transac { get; set; } = false;
        public bool asiento_banco { get; set; } = false;
        public bool asiento_formato { get; set; } = false;
        public string? asiento_mascara { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime registro_fecha { get; set; } = DateTime.Now;
        public bool int_reclasifica_id { get; set; } = false;
    }

    public class TesDocAnulaConcepRespuesta
    {
        public int codigo { get; set; } = 0;
        public string? mensaje { get; set; }
        public string? movimiento { get; set; }
        public int pass { get; set; } = 0;
    }
}