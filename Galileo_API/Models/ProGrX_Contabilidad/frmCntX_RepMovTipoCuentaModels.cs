using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public sealed class CntXRepMovTipoCuentaInicializarResponse
    {
        public DateTime fecha_servidor { get; set; } = DateTime.Now;    

        public List<DropDownListaGenericaModel> unidades { get; set; } = [];

        public List<DropDownListaGenericaModel> centros_costo { get; set; } = [];

        public string cuenta_minima { get; set; } = string.Empty;

        public string cuenta_maxima { get; set; } = string.Empty;
    }

    public sealed class CntXRepMovTipoCuentaData
    {
        public string cod_cuenta { get; set; } = string.Empty;

        public string cod_cuenta_mask { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class CntXRepMovTipoCuentaPrepararRequest
    {
        public int cod_contabilidad { get; set; } = 0;

        public string usuario { get; set; } = string.Empty;

        public DateTime fecha_inicio { get; set; } = DateTime.Now;

        public DateTime fecha_corte { get; set; } = DateTime.Now;

        public string cuenta_inicio { get; set; } = string.Empty;

        public string cuenta_corte { get; set; } = string.Empty;

        public bool mostrar_cuentas_cero { get; set; } = true;

        public bool mostrar_divisa_origen { get; set; } = false;

        public bool mostrar_pendientes { get; set; } = true;

        public string unidad { get; set; } = string.Empty;

        public string centro_costo { get; set; } = "0x0";
    }
}