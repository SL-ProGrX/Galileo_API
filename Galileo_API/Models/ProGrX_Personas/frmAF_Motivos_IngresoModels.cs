namespace PgxAPI.Models.ProGrX_Personas
{
    public class MotivoIngresoData
    {
        public string Cod_Motivo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = false;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class MotivoIngresoLista
    {
        public int Total { get; set; }
        public List<MotivoIngresoData> Lista { get; set; } = new List<MotivoIngresoData>();
    }
}