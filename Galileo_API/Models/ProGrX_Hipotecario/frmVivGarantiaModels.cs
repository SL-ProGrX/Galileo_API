using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class FrmVivGarantiaCargaRequest
    {
        public long operacion { get; set; } = 0;
        public string expediente { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaOperacionResponse
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public long id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string desc_linea { get; set; } = string.Empty;
        public string expediente { get; set; } = string.Empty;
        public string estadosol { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaGeneralItem
    {
        public long id_garantia { get; set; } = 0;
        public string numero_finca { get; set; } = string.Empty;
        public string num_plano_catastro { get; set; } = string.Empty;
        public string tipo_derecho { get; set; } = string.Empty;
        public string desc_grado_hipoteca { get; set; } = string.Empty;
        public decimal area_finca { get; set; } = 0;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class FrmVivGarantiaPrincipalResponse
    {
        public FrmVivGarantiaOperacionResponse operacion { get; set; } = new();
        public List<DropDownListaGenericaModel> grados_hipoteca { get; set; } = [];
        public List<DropDownListaGenericaModel> tipos_poliza { get; set; } = [];
        public List<DropDownListaGenericaModel> provincias { get; set; } = [];
        public List<DropDownListaGenericaModel> zonas { get; set; } = [];
    }

    public class FrmVivGarantiaOperacionGarantiaItem
    {
        public long id_garantia { get; set; } = 0;
        public int? ubicacion_canton { get; set; }
        public int? ubicacion_distrito { get; set; }
        public int? id_zona { get; set; }
        public long numero_operacion { get; set; } = 0;
        public string numero_finca { get; set; } = string.Empty;
        public string tipo_derecho { get; set; } = string.Empty;
        public string num_plano_catastro { get; set; } = string.Empty;
        public string grado_hipoteca { get; set; } = string.Empty;
        public string desc_grado_hipoteca { get; set; } = string.Empty;
        public decimal area_finca { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string anotaciones_finca { get; set; } = string.Empty;
        public string gravamenes { get; set; } = string.Empty;
        public string anotaciones_gravamen { get; set; } = string.Empty;
        public string observacion_avaluo { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string desc_zona { get; set; } = string.Empty;
        public string desc_provincia { get; set; } = string.Empty;
        public string desc_canton { get; set; } = string.Empty;
        public string desc_distrito { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaDetalleRequest
    {
        public long id_garantia { get; set; } = 0;
    }

    public class FrmVivGarantiaDetalleResponse
    {
        public long id_garantia { get; set; } = 0;
        public string numero_finca { get; set; } = string.Empty;
        public string tipo_derecho { get; set; } = string.Empty;
        public string num_plano_catastro { get; set; } = string.Empty;
        public decimal area_finca { get; set; } = 0;
        public string grado_hipoteca { get; set; } = string.Empty;

        public int? ubicacion_provincia { get; set; }
        public int? ubicacion_canton { get; set; }
        public int? ubicacion_distrito { get; set; }
        public int? id_zona { get; set; }
        public string direccion { get; set; } = string.Empty;

        public string tipo_poliza { get; set; } = string.Empty;
        public string anotaciones_finca { get; set; } = string.Empty;
        public string observacion_avaluo { get; set; } = string.Empty;

        public bool cobertura_primer_grado { get; set; }
        public bool registrar_calculo_avaluo { get; set; }
        public bool registrar_calculo_honorarios { get; set; }
        public bool registrar_detalle_manual { get; set; }

        public DateTime? fecha_inspeccion { get; set; }
        public decimal viaticos { get; set; }
        public decimal valor_terreno { get; set; }
        public decimal valor_construccion { get; set; }
        public decimal valor_total_inmueble { get; set; }

        public string ingeniero_nombre { get; set; } = string.Empty;
        public string abogado_nombre { get; set; } = string.Empty;
        public string tipo_poliza_avaluo { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaProvinciaRequest
    {
        public int provincia { get; set; } = 0;
    }

    public class FrmVivGarantiaCantonRequest
    {
        public int provincia { get; set; } = 0;
        public int canton { get; set; } = 0;
    }

}
