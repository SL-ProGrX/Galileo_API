namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXUnidadDto
    {
        public string? cod_unidad { get; set; }
        public string? descripcion { get; set; }
        public int? nivel { get; set; }
        public int? unidad_omision { get; set; }
        public int? reporta_renta { get; set; }
        public int? activa { get; set; }
        public string? cta_renta { get; set; }
        public string? cta_renta_gasto { get; set; }
    }

    public class CntXUnidadGuardarDto
    {
        public string cod_unidad { get; set; } = "";
        public string descripcion { get; set; } = "";
        public int? nivel { get; set; }

        public int? unidad_omision { get; set; }
        public int? reporta_renta { get; set; }
        public int? activa { get; set; }
    }

    public class CntXUnidadEliminarDto
    {
        public string cod_unidad { get; set; } = "";
    }

    public class CntXUnidadActivaDto
    {
        public string? cod_unidad { get; set; }
        public string? descripcion { get; set; }
    }

    public class CntXCentroCostoDto
    {
        public string? cod_centro_costo { get; set; }
        public string? descripcion { get; set; }

        public bool asignado { get; set; }
    }

    public class CntXUnidadCCGuardarDto
    {
        public string cod_unidad { get; set; } = "";
        public string cod_centro_costo { get; set; } = "";
        public int? asociado { get; set; } 
    }
}
