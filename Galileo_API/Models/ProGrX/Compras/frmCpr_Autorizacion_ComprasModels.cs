namespace PgxAPI.Models.CPR
{

    public class CprSolicitud_Autoriza
    {
        public int cpr_id { get; set; }
        public string cpr_id_madre { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_unidad_solicitante { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string i_solicitud_multiple { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime modifica_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime autoriza_fecha { get; set; }
        public string autoriza_nota { get; set; } = string.Empty;
        public string presupuesto_estado { get; set; } = string.Empty;
        public DateTime presupuesto_fecha { get; set; }
        public string presupuesto_usuario { get; set; } = string.Empty;
        public string i_plan_compras { get; set; } = string.Empty;
        public string i_presupuestado { get; set; } = string.Empty;
        public string i_contrato_requiere { get; set; } = string.Empty;
        public string monto { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string val_id { get; set; } = string.Empty;
        public string recomendacion { get; set; } = string.Empty;
        public DateTime adjudica_fecha { get; set; }
        public string adjudica_usuario { get; set; } = string.Empty;
        public string adjudica_orden { get; set; } = string.Empty;
        public string adjudica_proveedor { get; set; } = string.Empty;
        public string int_sitio_entrega { get; set; } = string.Empty;
        public string int_forma_pago { get; set; } = string.Empty;

    }

    public class CprSolicitud_Filtros
    {


        public string? estado { get; set; }
        public string? fecha { get; set; }
        public string? fechaInico { get; set; }
        public string? fechaCorte { get; set; }
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string vfiltro { get; set; } = string.Empty;

        public string cod_unidad { get; set; } = string.Empty;
    }


}
