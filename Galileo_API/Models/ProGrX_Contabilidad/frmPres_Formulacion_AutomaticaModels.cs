namespace PgxAPI.Models.ProGrX_Contabilidad
{
    public class PresFormulacionAutoDto
    {
        public int id_registro { get; set; }
        public string? cod_modelo { get; set; }
        public int cod_contabilidad { get; set; }
        public string? cod_cuenta { get; set; }
        public string? descripcion { get; set; }
        public decimal monto { get; set; }
        public DateTime corte { get; set; }
        public string? cod_unidad { get; set; }
        public string? cod_centro_costo { get; set; }
    }
}