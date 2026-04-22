namespace Galileo_API.Models.ProGrX.Cobros
{
    #region Principal
    public class CoControlListaBuscarRequest
    {
        public string? usuario { get; set; } = string.Empty;
        public bool todos_usuarios { get; set; } = false;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public bool todas_fechas { get; set; } = false;
        public bool casos_sin_asignar { get; set; } = false;
        public string? cedula { get; set; } = string.Empty;
        public string? nombre { get; set; } = string.Empty;
        public string? estado { get; set; } = string.Empty;
        public int? cuotas_desde { get; set; }
        public int? cuotas_hasta { get; set; }
        public string? cartera { get; set; } = string.Empty;
        public string? oficina { get; set; } = string.Empty;
        public int? institucion { get; set; }
        public string? tipo_casos { get; set; } = string.Empty;
        public int? dias_atencion { get; set; }
        public string? gestion { get; set; } = string.Empty;
        public string? causa { get; set; } = string.Empty;
        public string? arreglo { get; set; } = string.Empty;
        public bool todas_fechas_pago { get; set; } = true;
        public DateTime? fecha_pago_inicio { get; set; }
        public DateTime? fecha_pago_corte { get; set; }
        public bool incluir_info_contacto { get; set; } = false;
        public string? lista_garantias { get; set; } = string.Empty;
        public string? lista_antiguedades { get; set; } = string.Empty;
        public string? orden { get; set; } = string.Empty;
        public string? orden_tipo { get; set; } = string.Empty;
        public string? filtro { get; set; }
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public int? sortOrder { get; set; }
        public string? sortField { get; set; }
    }

    public class CoControlListaBuscarResponse
    {
        public CoControlListaTotales? totales { get; set; }
        public List<CoControlListaGridRow>? lista { get; set; }
    }

    public class CoControlListaTotales
    {
        public int casos { get; set; } = 0;
        public decimal mora { get; set; } = 0;
        public decimal mora_legal { get; set; } = 0;
    }

    public class CoControlListaGridRow
    {
        public bool caso_seleccionado { get; set; } = false;
        public DateTime? fecha_asignacion { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int cuota_mora { get; set; } = 0;
        public decimal mora { get; set; } = 0;
        public decimal mora_legal { get; set; } = 0;
        public int operaciones { get; set; } = 0;
        public bool mantener { get; set; } = false;
        public bool rebajo_doble { get; set; } = false;
        public int dias_ult_atencion { get; set; } = 0;
        public DateTime? ult_gestion_fecha { get; set; }
        public string ult_gestion_usuario { get; set; } = string.Empty;
        public string gestion_desc { get; set; } = string.Empty;
        public string causa_desc { get; set; } = string.Empty;
        public string arreglo_desc { get; set; } = string.Empty;
        public DateTime? arreglo_vence { get; set; }
        public int mora_dias { get; set; } = 0;
        public string antiguedad { get; set; } = string.Empty;
        public string tel_cel { get; set; } = string.Empty;
        public string tel_hab { get; set; } = string.Empty;
        public string tel_tra { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
    }

    public class CoControlListaUsuarioScrollResponse
    {
        public string usuario { get; set; } = string.Empty;
    }

    public class CoControlListaUsuarioScrollRequest: CoControlListaUsuarioScrollResponse
    {
        public string direccion { get; set; } = string.Empty;
    }

    public class CoControlListaUsuarioBusquedaRequest
    {
        public string filtro { get; set; } = string.Empty;
        public bool solo_activos { get; set; } = false;
        public string excluir_usuario { get; set; } = string.Empty;
    }

    public class CoControlListaUsuarioBusquedaRow: CoControlListaUsuarioScrollResponse
    {
        public string nombre { get; set; } = string.Empty;
    }

    public class CoControlListaPersonaBusquedaRequest: CoControlListaUsuarioScrollResponse
    {
        public bool activo { get; set; } = true;
    }

    public class CoControlListaPersonaBusquedaRow
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CoControlListaNotificarMarcadosRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string tipo { get; set; } = "R";
        public List<CoControlListaTrasladoCasoRequest> casos { get; set; } = new();
    }

    #endregion

    #region Operaciones

    public class CoControlListaOperacionesRequest
    {
        public string cedula { get; set; } = string.Empty;
    }

    public class CoControlListaOperacionesResponse
    {
        public CoControlListaOperacionesResumen? resumen { get; set; }
        public List<CoControlListaOperacionGarantiaRow>? garantias { get; set; }
        public List<CoControlListaOperacionDetalleRow>? operaciones { get; set; }
    }

    public class CoControlListaOperacionesResumen
    {
        public int operaciones_al_dia { get; set; } = 0;
        public int operaciones_mora { get; set; } = 0;
        public int operaciones_cobro_judicial { get; set; } = 0;
        public int operaciones_cartera { get; set; } = 0;
        public decimal saldo_al_dia { get; set; } = 0;
        public decimal saldo_mora { get; set; } = 0;
        public decimal saldo_cobro_judicial { get; set; } = 0;
        public decimal saldo_cartera { get; set; } = 0;
    }

    public class CoControlListaOperacionGarantiaRow
    {
        public string garantia { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
        public int operaciones { get; set; } = 0;
        public decimal morintcor { get; set; } = 0;
        public decimal morintmor { get; set; } = 0;
        public decimal morcargos { get; set; } = 0;
        public decimal morprincipal { get; set; } = 0;
        public int morcuotas { get; set; } = 0;
        public string morctaantigua { get; set; } = string.Empty;
        public string morctaultima { get; set; } = string.Empty;
        public int moradias { get; set; } = 0;
        public string antiguedad { get; set; } = string.Empty;
        public string cod_antiguedad { get; set; } = string.Empty;
    }

    public class CoControlListaOperacionDetalleRow
    {
        public long id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string antiguedad { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public decimal intc { get; set; } = 0;
        public decimal intm { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal poliza { get; set; } = 0;
        public decimal amortiza { get; set; } = 0;
        public decimal mora_financiera { get; set; } = 0;
        public decimal mora_legal { get; set; } = 0;
        public string garantia_detalle { get; set; } = string.Empty;
    }

    #endregion

    #region Datos Persona

    public class CoControlListaDatosPersonalesRequest
    {
        public string cedula { get; set; } = string.Empty;
    }

    public class CoControlListaDatosPersonalesResponse
    {
        public CoControlListaDatosPersonalesData? datos_personales { get; set; }
        public List<CoControlListaTelefonoRow>? telefonos { get; set; }
    }

    public class CoControlListaDatosPersonalesData
    {
        public string prov_desc { get; set; } = string.Empty;
        public string canton_desc { get; set; } = string.Empty;
        public string dist_desc { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public string af_email { get; set; } = string.Empty;
    }

    public class CoControlListaTelefonoRow
    {
        public string numero { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string ext { get; set; } = string.Empty;
        public string contacto { get; set; } = string.Empty;
    }

    #endregion

    #region Gestiones
    public class CoControlListaGestionesRequest
    {
        public string cedula { get; set; } = string.Empty;
    }

    public class CoControlListaGestionesResponse
    {
        public List<CoControlListaGestionRow>? gestiones { get; set; }
        public List<CoControlListaOficialRow>? oficiales { get; set; }
    }

    public class CoControlListaGestionRow
    {
        public int cod_seg { get; set; } = 0;
        public DateTime? fecha { get; set; }
        public DateTime? vencimiento { get; set; }
        public string gestion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public int tiempo_resolucion { get; set; } = 0;
        public string arreglo { get; set; } = string.Empty;
        public DateTime? arreglo_vence { get; set; }
        public string causa { get; set; } = string.Empty;
    }

    public class CoControlListaOficialRow
    {
        public DateTime? fecha_asignacion { get; set; }
        public string usuario { get; set; } = string.Empty;
        public bool mantener { get; set; } = false;
        public bool rebajo_doble { get; set; } = false;
        public bool aplica_mora { get; set; } = false;
    }

    public class CoControlListaNotificacionRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string tipo { get; set; } = "R";
        public string usuario { get; set; } = string.Empty;
    }
    #endregion

    #region Fiadores

    public class CoControlListaFiadoresRequest
    {
        public string cedula { get; set; } = string.Empty;
        public bool solo_operaciones_atrasadas { get; set; } = true;
    }

    public class CoControlListaFiadorRow
    {
        public string estado_mora { get; set; } = string.Empty;
        public int id_solicitud { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string inst { get; set; } = string.Empty;
    }

    #endregion

    #region Traslados

    public class CoControlListaTrasladoCasoRequest
    {
        public string cedula { get; set; } = string.Empty;
    }

    public class CoControlListaAplicarMarcadosRequest
    {
        public string usuario { get; set; } = string.Empty;
        public int mantener { get; set; } = 0;
        public int rebajo_doble { get; set; } = 0;
        public List<CoControlListaTrasladoCasoRequest> casos { get; set; } = new();
    }

    public class CoControlListaTrasladarMarcadosRequest
    {
        public string usuario_destino { get; set; } = string.Empty;
        public List<CoControlListaTrasladoCasoRequest> casos { get; set; } = new();
    }

    #endregion

    #region Gestiones Modal

    public class CoControlListaGestionActualRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CoControlListaGestionOperacionRow
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CoControlListaGestionActualResponse
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado_mora_texto { get; set; } = string.Empty;
        public string estado_mora_tag { get; set; } = string.Empty;

        public string cod_gestion { get; set; } = string.Empty;
        public string gestion_desc { get; set; } = string.Empty;

        public string cod_causa { get; set; } = string.Empty;
        public string causa_desc { get; set; } = string.Empty;

        public string cod_arreglo { get; set; } = string.Empty;
        public string arreglo_desc { get; set; } = string.Empty;
        public DateTime? arreglo_vence { get; set; }

        public decimal monto { get; set; } = 0;
        public bool permite_modificar_monto { get; set; } = false;
        public decimal desviacion_min { get; set; } = 0;
        public decimal desviacion_max { get; set; } = 0;

        public List<CoControlListaGestionOperacionRow> operaciones { get; set; } = new();
    }

    public class CoControlListaGestionDetalleRequest
    {
        public string cod_gestion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CoControlListaGestionDetalleResponse
    {
        public string cod_gestion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public bool permite_modificar_monto { get; set; } = false;
        public decimal desviacion_min { get; set; } = 0;
        public decimal desviacion_max { get; set; } = 0;
    }

    public class CoControlListaGestionProcesarRequest
    {
        public string cedula { get; set; } = string.Empty;
        public string usuario_sesion { get; set; } = string.Empty;
        public string cod_gestion { get; set; } = string.Empty;
        public DateTime? fecha_pago { get; set; }
        public string notas { get; set; } = string.Empty;
        public string oficina { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string operacion { get; set; } = string.Empty;
        public string cod_causa { get; set; } = string.Empty;
        public string cod_arreglo { get; set; } = string.Empty;
    }

    #endregion

    #region Cartera

    public class CoControlListaResumenCarteraUsuarioRequest
    {
        public string usuario { get; set; } = string.Empty;
    }

    public class CoControlListaResumenCarteraTotalesItem
    {
        public decimal saldo { get; set; } = 0;
        public int operaciones { get; set; } = 0;
    }

    public class CoControlListaResumenCarteraTotalesResponse
    {
        public CoControlListaResumenCarteraTotalesItem al_dia { get; set; } = new();
        public CoControlListaResumenCarteraTotalesItem mora { get; set; } = new();
        public CoControlListaResumenCarteraTotalesItem cobro_jud { get; set; } = new();
        public CoControlListaResumenCarteraTotalesItem cartera { get; set; } = new();
    }

    public class CoControlListaResumenCarteraAlDiaRow
    {
        public string id { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
        public string proceso { get; set; } = string.Empty;
        public int operaciones { get; set; } = 0;
    }

    public class CoControlListaResumenCarteraMoraRow
    {
        public string id { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
        public int operaciones { get; set; } = 0;
        public decimal morIntCor { get; set; } = 0;
        public decimal morIntMor { get; set; } = 0;
        public decimal morCargos { get; set; } = 0;
        public decimal morPrincipal { get; set; } = 0;
        public int morCuotas { get; set; } = 0;
        public string morCtaAntigua { get; set; } = string.Empty;
        public string morCtaUltima { get; set; } = string.Empty;
    }

    public class CoControlListaResumenCarteraUsuarioResponse
    {
        public List<CoControlListaResumenCarteraAlDiaRow> lista_al_dia_cobro_jud { get; set; } = new();
        public List<CoControlListaResumenCarteraMoraRow> lista_mora { get; set; } = new();
        public CoControlListaResumenCarteraTotalesResponse totales { get; set; } = new();
    }

    public class CoControlListaAnalisisCarteraProcesarResponse
    {
        public string mensaje { get; set; } = string.Empty;
    }

    #endregion

}
