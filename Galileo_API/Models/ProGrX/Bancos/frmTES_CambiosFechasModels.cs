namespace PgxAPI.Models.ProGrX.Bancos
{
    public class TesCambioFechasData
    {
        public int nsolicitud { get; set; }
        public string? tipo { get; set; }
        public string? estado { get; set; }
        public string? ndocumento { get; set; }
        public string? id_banco { get; set; }
        public string? bancoX { get; set; }
        public string? tipoDocX { get; set; }
        public string? detalle_Anulacion { get; set; }
        public string? estado_Asiento { get; set; }
        public Nullable<DateTime> fecha_emision { get; set; }
        public Nullable<DateTime> fecha_solicitud { get; set; }
        public Nullable<DateTime> fecha_anula { get; set; }
    }

    public class TesCambioFechasModel
    {
        public int nsolicitud { get; set; }
        public string? fecha { get; set; }
        public Nullable<DateTime> fechaActual { get; set; }
        public Nullable<DateTime> fechaNueva { get; set; }
        public string? detalle_Anulacion { get; set; }
        public string? usuario { get; set; }
    }
}