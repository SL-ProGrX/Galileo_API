namespace Galileo_API.Models.ProGrX_EstudioCrd
{

    public class FrmPreaAbandonaMotivosListaResponse
    {
        public List<FrmPreaAbandonaMotivoDto> lista { get; set; } = [];
    }

    public class FrmPreaAbandonaMotivoDto
    {
        public int id_motivo { get; set; }
        public string motivo { get; set; } = string.Empty;
        public bool activo { get; set; }
    }

    public class FrmPreaAbandonaMotivosRegistrarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public int id_motivo { get; set; } = 0;
        public bool activo { get; set; } = false
    }

    public class FrmPreaAbandonaMotivosRegistrarResponse
    {
        public int id_motivo { get; set; }
        public bool activo { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }

}
