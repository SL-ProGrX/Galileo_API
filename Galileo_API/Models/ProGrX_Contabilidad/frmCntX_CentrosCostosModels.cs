namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXCentroCostosData
    {
        public string cod_centro_costo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
    }

    public class CntXCentroCostosUnidadesDto
    {
        public string cod_unidad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool existeX { get; set; } = false;
    }
}