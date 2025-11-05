namespace PgxAPI.Models.ProGrX.Bancos.Autorizadores
{
    public class TesAutorizadoresLista 
    {
        public int total { get; set; } = 0;
        public List<DropDownListaGenericaModel>? lista { get; set; }
    }

    public class TesAutorizadoresDto
    {
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string? notas { get; set; }
        public string clave { get; set; } = string.Empty;
        public string? estado { get; set; }
        public float? rango_gen_inicio { get; set; }
        public float? rango_gen_corte { get; set; }
        public float? firmas_gen_inicio { get; set; }
        public float? firmas_gen_corte { get; set; }
    }
}