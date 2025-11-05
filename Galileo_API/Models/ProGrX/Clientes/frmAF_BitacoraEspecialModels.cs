namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AFBitacoraEspecialLista
    {
        public int total { get; set; }
        public List<AFBitacoraEspecialData>? lista { get; set; }
    }

    public class AFBitacoraEspecialData
    {
        public int id_bitacora { get; set; }
        public string? cedula { get; set; }
        public string? usuario { get; set; }
        public DateTime? fecha { get; set; }
        public int? movimiento { get; set; }
        public string? detalle { get; set; }
        public string? revisado_usuario { get; set; }
        public DateTime? revisado_fecha { get; set; }
        public string? cedula_ { get; set; }
        public string? nombre { get; set; }
        public string? movimientoDesc { get; set; }
        public int? revisado { get; set; }
    }

    public class AFBitacoraEspecialFiltros
    {
        public bool? chkFechas { get; set; } = false;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public bool? chkUsuario { get; set; } = false;
        public string? usuario { get; set; }
        public string? cedula { get; set; }
        public bool? chkMovimiento { get; set; } = false;
        public List<DropDownListaGenericaModel>? movimientos { get; set; }
        public bool? chkRevisados { get; set; } = false;
        public string? revision { get; set; }
        public bool? buscarUsuarioFechaRev { get; set; } = false;
    }
}