namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdCuentaData
    {
        public int noperacion { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int cod_comite { get; set; }
        public string comite_desc { get; set; } = string.Empty;
        public int cod_director { get; set; }
        public string director_desc { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal saldo { get; set; }
        public string estado { get; set; } = string.Empty;
        public string proceso { get; set; } = string.Empty;
        public string aprueba { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string proceso_desc { get; set; } = string.Empty;
        public string tipo_desc { get; set; } = string.Empty;
        public string aprobacion_desc { get; set; } = string.Empty;
        public string liquidable { get; set; } = string.Empty;
        public DateTime? liquida_fecha { get; set; } 
        public string liquida_usuario { get; set; } = string.Empty;
        public DateTime? liquida_vence { get; set; }
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public int? id_banco { get; set; }
        public string banco_desc { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
        public int? tesoreria_nsolicitud { get; set; }
        public DateTime? tesoreria_fecha { get; set; }
        public string tesoreria_usuario { get; set; } = string.Empty;
        public int? cod_remesa { get; set; }
        public string notas { get; set; } = string.Empty;
        public int? ajuste_asoc { get; set; }
        public int? cant_asociados { get; set; }
        public decimal? monto_cargos { get; set; }
        public decimal? monto_refunde { get; set; }
        public int? asoc_total { get; set; }
        public DateTime? activa_fecha { get; set; }
        public string activa_usuario { get; set; } = string.Empty;
    }

    public class AfCdActividadData
    {
        public int cod_actividad { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string tipo { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }

    public class AfCdCuentaAdjuntosData
    {
        public int idArchivoAdjunto { get; set; } = 0;
        public int noperacion { get; set; }
        public string nombreArchivo { get; set; } = string.Empty;
        public int idtipoarchivo { get; set; }
        public string nota { get; set; } = string.Empty;
        public DateTime registrofecha { get; set; }
        public string registroUsuario { get; set; } = string.Empty;
        public string nombreTipoArchivo { get; set; } = string.Empty;
    }

    public class AfCdCuentaBitacoraData
    {
        public int idRegistro { get; set; } = 0;
        public string nota { get; set; } = string.Empty;
        public DateTime registroFecha { get; set; }
        public string registroUsuario { get; set; } = string.Empty;
        public string nombreTipoProceso { get; set; } = string.Empty;
        public string nombreEstado { get; set; } = string.Empty;
    }

    public class AfCdComiteData
    {
        public int noperacion { get; set; } = 0;
        public int cod_comite { get; set; } = 0;
        public string comite { get; set; } = string.Empty;
        public int? tesoreria_nsolicitud { get; set; }
        public DateTime? tesoreria_fecha { get; set; }
        public string tesoreria_usuario { get; set; } = string.Empty;
        public DateTime? liquida_fecha { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public int? id_banco { get; set; }
    }

    public class AfCdCuentaBancariaData
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
        public int? prioridad { get; set; }
    }
}
