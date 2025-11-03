namespace PgxAPI.Models.ProGrX.Bancos
{
    public class TES_DropDownListaBancosExplorer
    {
        public string id_banco { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;

    }

    public class TES_ExplorerFiltros
    {
        public string cod_banco { get; set; } = string.Empty;
        public string tipo_doc { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime fecha_desde { get; set; }
        public DateTime fecha_hasta { get; set; }

    }

    public class TES_ListaExplorerDTO
    {
        public string nsolicitud { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string beneficiario { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime? fecha_solicitud { get; set; }
        public DateTime? fecha_anula { get; set; }
        public DateTime? fecha_emision { get; set; }
        public DateTime? fecha_autorizacion { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public decimal monto_total { get; set; }
    }



}




