namespace Galileo_API.Models.ProGrX.Fondos
{
    public class FrmFndBitacoraMovimientoDto
    {
        public string movimiento { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FrmFndBitacoraCambiosRequest
    {
        public string? Cedula { get; set; }
        public DateTime? FechaIni { get; set; }
        public DateTime? FechaFin { get; set; }
        public List<string>? Movimientos { get; set; }
        public string? CodPlan { get; set; }
        public int? CodOperadora { get; set; }
        public int? CodContrato { get; set; }
        public string? SoloNoRevisados { get; set; }

        // Soporte para la lógica real del V6 aunque el Angular actual aún no los envía.
        public string? Usuario { get; set; }
        public bool BuscarUsuarioFechaRevision { get; set; }
    }

    public class FrmFndBitacoraCambiosDto
    {
        public int id_Bitacora { get; set; }
        public int cod_Operadora { get; set; }
        public string cod_Plan { get; set; } = string.Empty;
        public int cod_Contrato { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string revisado_Usuario { get; set; } = string.Empty;
        public DateTime? revisado_Fecha { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string movimientoDesc { get; set; } = string.Empty;
        public int revisado { get; set; }
    }

    public class FrmFndBitacoraCambioRevisarRequest
    {
        public int id_Bitacora { get; set; }
        public string revisado_Usuario { get; set; } = string.Empty;
    }

    public class FrmFndBitacoraSifRegistraTagsRequest
    {
        public string codigo { get; set; } = string.Empty;
        public string tag { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string modulo { get; set; } = string.Empty;
        public string llave_01 { get; set; } = string.Empty;
        public string llave_02 { get; set; } = string.Empty;
        public string llave_03 { get; set; } = string.Empty;
    }
}
