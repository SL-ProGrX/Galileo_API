namespace Galileo.Models.CPR
{
    public class CprProveedoresFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class CprProveedoresLista
    {
        public int total { get; set; }
        public List<CprProveedoresDto> proveedores { get; set; } = new List<CprProveedoresDto>();
    }

    public class CprProveedoresDto
    {
        public int proveedor_codigo { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string cedjur { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string? observacion { get; set; }
        public string? telefono { get; set; }
        public string? email { get; set; }
        public string estado { get; set; } = string.Empty;
        public int? cod_proveedor { get; set; }
        public DateTime registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }

    public class CprProveedorBitacoraData
    {
        public int cpr_id { get; set; } = 0;
        public string estado { get; set; } = "P";
        public Nullable<DateTime> valora_fecha { get; set; } = DateTime.Now;
        public string valora_usuario { get; set; } = "NA";
        public int valora_puntaje { get; set; } = 0;
    }
}