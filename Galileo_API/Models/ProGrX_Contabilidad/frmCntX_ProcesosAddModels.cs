namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CtnXProcesosAddDto
    {
        public string cod_proceso { get; set; } = string.Empty;
        public int cod_contabilidad { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public string sp_name { get; set; } = string.Empty;
        public int consecutivo { get; set; } = 0;
        public bool activo { get; set; } = false;
    }

    public class CntXProcesarRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public int periodo_anio { get; set; } = 0;
        public int periodo_mes { get; set; } = 0;
        public string usuario { get; set; } = ""; 
        public List<CtnXProcesosAddDto> lista { get; set; } = new List<CtnXProcesosAddDto>();
    }


}
