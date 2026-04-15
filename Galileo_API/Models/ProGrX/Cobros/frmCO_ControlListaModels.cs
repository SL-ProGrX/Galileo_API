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

}
