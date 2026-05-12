namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaSeguimientoCausasListaResponse
    {
        public List<FrmPreaSeguimientoCausasDto> lista { get; set; } = [];
    }

    public class FrmPreaSeguimientoCausasDto
    {
        public string cod_causas { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string registro_fecha { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FrmPreaSeguimientoCausasRegistrarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string cod_causas { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
    }

    public class FrmPreaSeguimientoCausasRegistrarResponse
    {
        public string cod_causas { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }
}
