namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXTiposAsientosData
    {
        public string tipo_asiento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public int consecutivo { get; set; } = 0;
    }
}
