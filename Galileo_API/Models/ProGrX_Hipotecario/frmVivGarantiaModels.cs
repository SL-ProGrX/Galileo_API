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

        public string gravamenes { get; set; } = string.Empty;
        public string anotaciones_gravamen { get; set; } = string.Empty;
        public decimal monto_no_gravable { get; set; } = 0;
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

    public class FrmVivGarantiaIdGarantiaRequest
    {
        public long id_garantia { get; set; }
    }

    public class FrmVivGarantiaDerechoDuenoItem
    {
        public long id_garantia { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int? provincia_id { get; set; }
        public int? canton_id { get; set; }
        public int? distrito_id { get; set; }
        public string direccion { get; set; } = string.Empty;
        public string desc_provincia { get; set; } = string.Empty;
        public string desc_canton { get; set; } = string.Empty;
        public string desc_distrito { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class FrmVivGarantiaSocioRequest
    {
        public string cedula { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaSociosBuscarRequest
    {
        public string filtro { get; set; } = string.Empty;
        public int first { get; set; }
        public int rows { get; set; }
    }

    public class FrmVivGarantiaSocioItem
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int total { get; set; }
    }

    public class FrmVivGarantiaSociosBuscarResponse
    {
        public List<FrmVivGarantiaSocioItem> value { get; set; } = [];
        public int total { get; set; }
    }

    public class FrmVivGarantiaHistorialRawItem
    {
        public string RegistroUsuario { get; set; } = string.Empty;
        public DateTime? RegistroFecha { get; set; }
        public string GEstado { get; set; } = string.Empty;
        public string EstadoProf { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string AsignacionUsuario { get; set; } = string.Empty;
        public DateTime? AsignacionFecha { get; set; }
        public string EntregaUsuario { get; set; } = string.Empty;
        public DateTime? EntregaFecha { get; set; }
        public string RecepcionUsuario { get; set; } = string.Empty;
        public DateTime? RecepcionFecha { get; set; }
        public string RegistroUsuarioProf { get; set; } = string.Empty;
        public DateTime? RegistroFechaProf { get; set; }
        public string FirmasUsuario { get; set; } = string.Empty;
        public DateTime? FirmasFecha { get; set; }
    }

    public class FrmVivGarantiaHistorialResumenResponse
    {
        public DateTime? fecha_registro { get; set; }
        public string usuario_registro { get; set; } = string.Empty;
        public string estado_actual { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaHistorialProfesionalResponse
    {
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime? asignacion_fecha { get; set; }
        public string asignacion_usuario { get; set; } = string.Empty;
        public DateTime? entrega_fecha { get; set; }
        public string entrega_usuario { get; set; } = string.Empty;
        public DateTime? recepcion_fecha { get; set; }
        public string recepcion_usuario { get; set; } = string.Empty;
        public DateTime? firmas_fecha { get; set; }
        public string firmas_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaHistorialResponse
    {
        public FrmVivGarantiaHistorialResumenResponse resumen { get; set; } = new();
        public FrmVivGarantiaHistorialProfesionalResponse ingeniero { get; set; } = new();
        public FrmVivGarantiaHistorialProfesionalResponse abogado { get; set; } = new();
    }

    public class FrmVivGarantiaFincaAsociadaItem
    {
        public long id_garantia { get; set; }
        public long numero_operacion { get; set; }
        public string numero_finca { get; set; } = string.Empty;
        public string num_plano_catastro { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public decimal valor_terreno { get; set; }
        public decimal valor_construccion { get; set; }
        public decimal area_finca { get; set; }
        public string grado_hipoteca { get; set; } = string.Empty;
        public string tipo_poliza { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string linea_estado { get; set; } = string.Empty;
        public decimal saldo { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string linea_desc { get; set; } = string.Empty;
        public long poliza_id { get; set; }
        public decimal poliza_cuota { get; set; }
        public string poliza_estado { get; set; } = string.Empty;
        public string poliza_codigo { get; set; } = string.Empty;
        public string poliza_desc { get; set; } = string.Empty;
        public string tipo_aplicacion { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaNotasRequest
    {
        public long id_garantia { get; set; }
        public string tipo { get; set; } = string.Empty;
    }

    public class FrmVivGarantiaNotaTramiteRawItem
    {
        public long IdNota { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Nota { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
    }

    public class FrmVivGarantiaNotaTramiteItem
    {
        public long id_nota { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string nota { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
    }
}
