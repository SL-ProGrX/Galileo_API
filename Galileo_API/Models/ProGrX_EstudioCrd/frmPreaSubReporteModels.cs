using Galileo.Models;

namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaSubReporteCargarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaSubReporteCargarResponse
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string titulo { get; set; } = string.Empty;

        public bool chk_resumen { get; set; } = true;
        public bool chk_detalle { get; set; } = true;
        public bool chk_ficha_convenio { get; set; } = false;
        public bool chk_estado_cuenta { get; set; } = false;
        public bool chk_deducciones { get; set; } = false;
        public bool chk_impresora { get; set; } = true;

        public bool chk_sub_expediente { get; set; } = true;
        public bool chk_sub_expediente_resumen { get; set; } = true;
        public bool chk_sub_expediente_detalle { get; set; } = false;
        public bool chk_sub_expediente_estado { get; set; } = false;

        public bool habilita_resumen { get; set; } = true;
        public bool habilita_detalle { get; set; } = true;
        public bool habilita_ficha_convenio { get; set; } = true;
        public bool habilita_estado_cuenta { get; set; } = false;
        public bool habilita_sub_expediente { get; set; } = true;
        public bool habilita_sub_expediente_resumen { get; set; } = true;
        public bool habilita_sub_expediente_detalle { get; set; } = false;
        public bool habilita_sub_expediente_estado { get; set; } = false;
    }

    public class FrmPreaSubReporteBaseData
    {
        public string cod_preanalisis { get; set; } = string.Empty;
    }

    public class FrmPreaSubReporteImprimirRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;

        public bool chk_resumen { get; set; } = false;
        public bool chk_detalle { get; set; } = false;
        public bool chk_ficha_convenio { get; set; } = false;
        public bool chk_estado_cuenta { get; set; } = false;
        public bool chk_deducciones { get; set; } = false;

        public bool chk_sub_expediente { get; set; } = false;
        public bool chk_sub_expediente_resumen { get; set; } = false;
        public bool chk_sub_expediente_detalle { get; set; } = false;
        public bool chk_sub_expediente_estado { get; set; } = false;
    }

    public class FrmPreaSubReporteImprimirResponse
    {
        public List<FrmReporteGlobal> reportes { get; set; } = new();
    }

    public class FrmPreaSubReporteInformacionBaseData
    {
        public string cod_preanalisis { get; set; } = string.Empty;
        public string? fecha_ingreso { get; set; }
        public string? fecha_sistema { get; set; }
        public string? fecha_nacimiento { get; set; }
        public string? estado_actual { get; set; }
    }

    public class AgregaRepExpedienteParametros
    {
        public bool chkResumen { get; set; } = false;
        public bool chkDetalle { get; set; } = false;
        public bool chkFichaConvenio { get; set; } = false;
        public bool chkEstadoCuenta { get; set; } = false;
        public bool chkDeducciones { get; set; } = false;
    }
}
