
namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXRepGeneralResultadosContextoResponseDto
    {
        public int cod_contabilidad { get; set; }
        public string nombre_empresa { get; set; } = string.Empty;
        public string mascara_cod { get; set; } = string.Empty;
        public int periodo_anio { get; set; }
        public int periodo_mes { get; set; }
        public string periodo_desc { get; set; } = string.Empty;
        public int periodo_abierto { get; set; }
        public int preliminar_habilitado { get; set; }
    }

    public class CntXRepGeneralResultadosValidarRequestDto
    {
        public int? cod_contabilidad { get; set; }
        public int? periodo_anio { get; set; }
        public int? periodo_mes { get; set; }
        public int? chk_preliminar { get; set; }
        public int? chk_titulos { get; set; }
        public int? chk_mov_cero { get; set; }
        public int? chk_orden { get; set; }
        public int? chk_cuentas_contables { get; set; }
        public string reporte { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string nivel { get; set; } = string.Empty;
        public string unidad { get; set; } = string.Empty;
        public string centro_costo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CntXRepGeneralResultadosValidarResponseDto
    {
        public string periodo_desc { get; set; } = string.Empty;
        public int periodo_abierto { get; set; }
        public int es_mes_fiscal { get; set; }
        public int fx_asiento_cierre { get; set; }
        public int fx_muestra_titulo { get; set; }
        public string fx_subtitulo { get; set; } = string.Empty;
        public string fx_unidad { get; set; } = string.Empty;
    }

    public class CntXRepGeneralResultadosReporteResponseDto
    {
        public string nombre_reporte { get; set; } = string.Empty;
        public string nombre_dataset { get; set; } = string.Empty;
        public string filtros { get; set; } = string.Empty;
        public string fecha { get; set; } = string.Empty;
        public string empresa { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string mascara { get; set; } = string.Empty;
        public string subtitulo { get; set; } = string.Empty;
        public string fx_unidad { get; set; } = string.Empty;
        public int fx_asiento_cierre { get; set; }
        public int fx_muestra_titulo { get; set; }
        public int es_preliminar { get; set; }
    }

    internal class CntXRepGeneralResultadosInternoDto
    {
        public string nombre_reporte { get; set; } = string.Empty;
        public string nombre_dataset { get; set; } = string.Empty;
        public string filtros { get; set; } = string.Empty;
        public string fx_unidad { get; set; } = string.Empty;
        public int fx_asiento_cierre { get; set; }
        public int fx_muestra_titulo { get; set; }
        public int es_preliminar { get; set; }
    }
}
