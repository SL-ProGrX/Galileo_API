using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public sealed class CntXRepBalanceSituacionInicializarResponse
    {
        public List<DropDownListaGenericaModel> unidades { get; set; } = [];

        public List<DropDownListaGenericaModel> centros_costo { get; set; } = [];

        public string cuenta_minima { get; set; } = string.Empty;

        public string cuenta_maxima { get; set; } = string.Empty;
    }

    public sealed class CntXRepBalanceSituacionCuentaData
    {
        public string cod_cuenta { get; set; } = string.Empty;

        public string cod_cuenta_mask { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class CntXRepBalanceSituacionPrepararRequest
    {
        public int cod_contabilidad { get; set; } = 0;

        public string usuario { get; set; } = string.Empty;

        public int anio_inicio { get; set; } = 0;

        public int mes_inicio { get; set; } = 0;

        public int anio_corte { get; set; } = 0;

        public int mes_corte { get; set; } = 0;

        public string cuenta_inicio { get; set; } = string.Empty;

        public string cuenta_corte { get; set; } = string.Empty;

        public string unidad { get; set; } = "0x0";

        public string centro_costo { get; set; } = "0x0";
    }

    public sealed class CntXRepBalanceSituacionPrepararResponse
    {
        public DateTime fecha_inicio { get; set; } = DateTime.Now;

        public DateTime fecha_corte { get; set; } = DateTime.Now;

        public string cuenta_inicio { get; set; } = string.Empty;

        public string cuenta_corte { get; set; } = string.Empty;
    }
}