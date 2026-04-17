using Galileo.Models;

namespace Galileo.Models.ProGrX.Cajas
{
    public class CajasServiciosConceptosLista
    {
        public int total { get; set; }
        public List<CajasServiciosConceptosData> lista { get; set; } = new List<CajasServiciosConceptosData>();
    }
    public class CajasServiciosCabysLista
    {
        public int total { get; set; }
        public List<DropDownListaGenericaModel> lista { get; set; } = new();
    }
    public class CajasServiciosConceptosData
    {
        public string cod_recaudador { get; set; } = string.Empty;
        public string cod_servicio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = true;
        public string contrato { get; set; } = string.Empty;
        public DateTime? vence_fecha { get; set; } = null;
        public bool vence_activo { get; set; } = true;
        public string cod_concepto { get; set; } = string.Empty;
        public string concepto_desc { get; set; } = string.Empty;
        public bool intercambio { get; set; } = false;
        public bool valor_transito_valida { get; set; } = false;
        public bool genera_factura { get; set; } = false;
        public string cabys { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_comision { get; set; } = string.Empty;
        public string cod_cuenta_iv { get; set; } = string.Empty;
        public string unidad_desc { get; set; } = string.Empty;
        public string centro_costo_desc { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
        public string cuenta_comision_desc { get; set; } = string.Empty;
        public string cuenta_iv_desc { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public string registro_fecha { get; set; } = string.Empty;
        public string modifica_usuario { get; set; } = string.Empty;
        public string modifica_fecha { get; set; } = string.Empty;
        public bool isNew { get; set; } = false;
    }
    public class CajasServiciosComisionesData
    {
        public string cod_recaudador { get; set; } = string.Empty;
        public string cod_servicio { get; set; } = string.Empty;

        public string concepto { get; set; } = string.Empty;

        public int linea { get; set; }

        public decimal monto_inicial { get; set; }
        public decimal monto_corte { get; set; }
        public decimal monto_minimo_comision { get; set; }
        public decimal porcentaje_comision { get; set; }
        public decimal porcentaje_imp_ventas { get; set; }

        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }

        public bool isNew { get; set; }
    }
    public class CajasServiciosCajasVinculadasData
    {
        public string concepto { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
        public string desc_caja { get; set; } = string.Empty;
        public short asignada { get; set; }
    }
}