namespace PgxAPI.Models.INV
{
    public class UnidadMedicionConv
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class UnidadesConvLista
    {
        public int total { get; set; }
        public List<UnidadMedicionConvData> lista { get; set; } = new List<UnidadMedicionConvData>();
    }

    public class UnidadMedicionConvData
    {
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_unidad_d { get; set; } = string.Empty;
        public float factor { get; set; } = 0;
    }

}
