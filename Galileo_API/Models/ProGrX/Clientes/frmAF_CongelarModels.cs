namespace Galileo.Models.ProGrX.Clientes
{
    public class AFCongelarFiltros
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime fecha_desde { get; set; }
        public DateTime fecha_hasta { get; set; }
        public bool chkTodasFechas { get; set; }
    }

    public class TesListaCongelarDto
    {
        public int cod_congelar { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string causa { get; set; } = string.Empty;
        public DateTime? fecha_inicia { get; set; }
    }

    public class AFCongelarDto
    {
        public int cod_congelar { get; set; }
        public string? cedula { get; set; }
        public string? cod_causa { get; set; }
        public string? notas { get; set; }
        public DateTime? fecha_crea { get; set; }
        public string? usuario_crea { get; set; }
        public string? estado { get; set; }
        public DateTime? fecha_finaliza { get; set; }
        public int per_liquidacion { get; set; }
        public int per_cobro_fndsol { get; set; }
        public int per_mostrar_ec { get; set; }
        public int per_cobro_cuotacr { get; set; }
        public int per_abono_cajas { get; set; }
        public int per_cierra_accreditos { get; set; }
        public int per_cobro_judicial { get; set; }
        public int per_traspaso_deudas { get; set; }
        public int per_reversiones { get; set; }
        public int per_readecuaciones { get; set; }
        public int per_deducciones_aportes { get; set; }
        public int per_deducciones_creditos { get; set; }
        public int per_generacion_mora { get; set; }
        public DateTime? fecha_inicia { get; set; }
        public string? nombre { get; set; }
        public string? causaid { get; set; }
        public string? causadesc { get; set; }
    }

    public class AFCongelaListaData
    {
        public int cod_congelar { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
        public Nullable<DateTime> fecha_finaliza { get; set; }
        public string nombre { get; set; } = string.Empty;
    }

    public class AFCongelaCausaDto
    {
        public string cod_causa { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public bool isNew { get; set; }
    }
}