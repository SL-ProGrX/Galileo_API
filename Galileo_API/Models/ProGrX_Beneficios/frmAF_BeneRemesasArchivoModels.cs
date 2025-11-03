namespace PgxAPI.Models.AF
{
    public class TipoDocumentosLista
    {
        public string item { get; set; }
        public string descripcion { get; set; }
    }

    public class RmsRemesasDataLista
    {
        public int total { get; set; }
        public List<RmsRemesasData> lista { get; set; }
    }

    public class RmsRemesasData
    {
        public int? IdRemesa { get; set; }
        public int CodRemesa { get; set; }
        public int IdTipoDocumento { get; set; }
        public int? CodDepartamentoOrigen { get; set; }
        public int? CodDepartamentoDestino { get; set; }
        public string? RegistroUsuario { get; set; }
        public Nullable<DateTime> RegistroFecha { get; set; }
        public string? NotaOrigen { get; set; }
        public string? NotaDestino { get; set; }
        public bool? IdEstado { get; set; }
        public bool? Activa { get; set; }
    }

    public class RmsRemesasDetalleDataLista
    {
        public int total { get; set; }
        public List<RmsRemesasDetalleData> lista { get; set; }
    }

    public class RmsRemesasDetalleData
    {
        public int IdRemesaDetalle { get; set; }
        public int IdRemesa { get; set; }
        public string Documento { get; set; }
        public DateTime DocumentoRegistroFecha { get; set; }
        public string DocumentoRegistroUsuario { get; set; }
        public string DocumentoIdAsociado { get; set; }
        public string DocumentoNombreAsociado { get; set; }
        public string RegistroUsuario { get; set; }
        public DateTime RegistroFecha { get; set; }
        public string? RecibeUsuario { get; set; }
        public DateTime? RecibeFecha { get; set; }
        public string? NotaOrigen { get; set; }
        public string? NotaDestino { get; set; }
        public int IdEstado { get; set; }
        public string Mascara { get; set; }
        public Guid GuidId { get; set; }
    }

    public class RmsRemesaFiltro
    {
        public string? Filtro { get; set; }
        public int? IdEstado { get; set; }
        public int? IdTipoDocumento { get; set; }
        public Nullable<DateTime> FechaInicio { get; set; }
        public Nullable<DateTime> FechaFin { get; set; }
    }

    public class RmsRemesaDocuementos
    {
        public int id_beneficio { get; set; }
        public string n_expediente { get; set; }
        public DateTime registra_fecha { get; set; }
        public string registra_user { get; set; }
        public string estado { get; set; }
        public string estado_desc { get; set; }
        public string cedula { get; set; }
        public string nombre { get; set; }
        public string notaOrigen { get; set; }
    }

    public class RmsCargaFiltros
    {
        public string? cod_categoria { get; set; }
        public Nullable<DateTime> fecha_inicio { get; set; }
        public Nullable<DateTime> fecha_corte { get; set; }
    }
}
