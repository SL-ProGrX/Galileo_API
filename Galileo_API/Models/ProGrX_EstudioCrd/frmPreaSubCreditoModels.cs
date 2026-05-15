using Galileo.Models;

namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaSubCreditoCargarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaSubCreditoCargarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public int aprobado { get; set; } = 0;
        public int pendiente { get; set; } = 0;
        public int maestro { get; set; } = 0;
        public int comite { get; set; } = 0;
        public string mensaje_validacion { get; set; } = string.Empty;
        public List<DropDownListaGenericaModel> bancos { get; set; } = new();
        public List<DropDownListaGenericaModel> operaciones { get; set; } = new();
        public List<FrmPreaSubCreditoTipoDocumentoItem> tipos_documento { get; set; } = new();
        public List<FrmPreaSubCreditoCuentaItem> cuentas { get; set; } = new();
    }

    public class FrmPreaSubCreditoAplicarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public int comite { get; set; } = 0;
        public int banco { get; set; } = 0;
        public int emitir_transferencia { get; set; } = 0;
        public string tipo_documento { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public int operacion { get; set; } = 0;
    }

    public class FrmPreaSubCreditoAplicarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string id_solicitud { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    public class FrmPreaSubCreditoTipoDocumentoItem
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int emitir_transferencia { get; set; } = 0;
    }

    public class FrmPreaSubCreditoValidacionData
    {
        public int aprobado { get; set; } = 0;
        public int pendiente { get; set; } = 0;
        public int maestro { get; set; } = 0;
        public int comite { get; set; } = 0;
    }

    public class FrmPreaSubCreditoOperacionData
    {
        public string operacion { get; set; } = string.Empty;
    }

    public class FrmPreaSubCreditoCuentaItem
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FrmPreaSubCreditoPersonaData
    {
        public string cedula { get; set; } = string.Empty;
    }

    public class FrmPreaSubCreditoCuentasRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public int banco { get; set; } = 0;
    }

    public class FrmPreaSubCreditoCuentasResponse
    {
        public int banco { get; set; } = 0;
        public List<FrmPreaSubCreditoCuentaItem> cuentas { get; set; } = new();
    }
}
