namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaEstadoPreanalisisCargarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string cod_preanalisis_ref { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string cod_estado_v2 { get; set; } = string.Empty;
        public string cod_linea { get; set; } = string.Empty;
        public bool requiere_autorizadores { get; set; }
        public bool autorizadores_marcados { get; set; }
        public List<FrmPreaEstadoPreanalisisCausaDto> causas { get; set; } = [];
    }

    public class FrmPreaEstadoPreanalisisCausaDto
    {
        public string cod_causas { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string registro_fecha { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FrmPreaEstadoPreanalisisGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }

    public class FrmPreaEstadoPreanalisisGuardarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string mensaje { get; set; } = "La informacion fue actualizada correctamente.";
    }

    public class FrmPreaEstadoPreanalisisCausaRegistrarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string cod_causas { get; set; } = string.Empty;
        public bool? activo { get; set; }
    }

    public class FrmPreaEstadoPreanalisisCausaRegistrarResponse
    {
        public string cod_causas { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }
}
