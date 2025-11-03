namespace PgxAPI.Models.CPR
{
    public class cprPlanComprasDTO
    {
        public int id_pc { get; set; }
        public int id_periodo { get; set; }
        public string cod_unidad { get; set; } = string.Empty;
        public string? cod_unidad_destino { get; set; }
        public string? estado { get; set; }
        public string? notas { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public string? pres_estado { get; set; }
        public decimal? pres_monto { get; set; }
        public decimal? pres_ejecutado { get; set; }
        public decimal? pres_reservado { get; set; }
        public decimal? pres_transito { get; set; }
        public Nullable<DateTime> pres_actualiza { get; set; }
        public string? pres_extraordinario { get; set; }
    }

    public class cprPlanDTDTO
    {
        public int id_plan { get; set; }
        public int id_pc { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public decimal monto_unitario { get; set; }
        public int cantidad_total { get; set; }
        public int cantidad_despachada { get; set; }
        public int cantidad_reservada { get; set; }
        public int cantidad_transito { get; set; }
        public decimal total_presupuesto { get; set; }
        public decimal total_ejecutado { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }

    public class cprPlanDTCortesDTO
    {
        public DateTime corte { get; set; }
        public int cantidad { get; set; }
        public decimal monto { get; set; }
    }

    public class cprPlanDTUpsert
    {
        public int id_pc { get; set; }
        public string cod_producto { get; set; } = string.Empty;
        public decimal monto_unitario { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CprResumenPlanLista
    {
        public int Total { get; set; }
        public List<cprResumenPlanDTO>? Lineas { get; set; } = new List<cprResumenPlanDTO>();
    }

    public class cprResumenPlanDTO
    {
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cantidad { get; set; }
        public decimal monto { get; set; }
        public DateTime corte { get; set; }
    }

    public class cprPlanContableLista
    {
        public int Total { get; set; }
        public List<cprPlanContableDTO>? Lineas { get; set; } = new List<cprPlanContableDTO>();
    }

    public class cprPlanContableDTO
    {
        public string cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string unidad { get; set; } = string.Empty;
        public string centro_costo { get; set; } = string.Empty;
        public decimal total { get; set; }
        public DateTime corte { get; set; }
    }

    public class cprPlanFiltros
    {
        public int? planCompras { get; set; } = 0;
        public string? periodo { get; set; } = string.Empty;
        public string? filtro { get; set; } = string.Empty;
        public int? pagina { get; set; } = 0;
        public int? paginacion { get; set; } = 30;
    }

    public class cprBitacoraLista
    {
        public int Total { get; set; }
        public List<cprBitacoraDTO>? Lineas { get; set; }
    }

    public class cprBitacoraDTO
    {
        public int id_bitacora { get; set; }
        public DateTime fechahora { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    public class cprPlanDTTotalesData
    {
        public int qty_solicitada { get; set; }
        public int qty_plan_compras { get; set; }
        public int qty_recervada { get; set; }
        public int qty_entregada { get; set; }
    }
}
