namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXAsientosVerificarRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public int opcion { get; set; } = 0;
        public bool check { get; set; } = false;
    }
}
