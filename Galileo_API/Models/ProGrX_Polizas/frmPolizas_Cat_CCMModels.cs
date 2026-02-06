namespace Galileo_API.Models.ProGrX_Polizas
{
    public class PolizasCoberturasMotivosCausasDto
    {
        public int Id_Registro_Mc { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short Activo { get; set; }
        public string? Registro_Usuario { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Modifica_Usuario { get; set; }
        public DateTime? Modifica_Fecha { get; set; }
        public string Cod_Poliza { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }

    public class PolizasConceptosConfigAddParams
    {
        public int Id { get; set; }
        public string Cod_Poliza { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public short Activo { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
    }

    public class PolizasConceptosConfigAddResult
    {
        public short Pass { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string Movimiento { get; set; } = string.Empty;
        public int IdLLave { get; set; }
    }

    public class PolizasConceptosConfigDelParams
    {
        public int Id { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }
}
