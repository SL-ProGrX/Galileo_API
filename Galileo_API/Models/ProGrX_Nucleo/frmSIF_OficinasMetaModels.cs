
namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class SifOficinasMetaLista
    {
        public int total { get; set; }
        public List<SifOficinasMetaData> lista { get; set; } = new List<SifOficinasMetaData>();
    }

    public class SifOficinasMetaData
    {

        public string cod_oficina { get; set; } = string.Empty;
        public int anio { get; set; }
        public int mes { get; set; }
        public decimal mes_meta { get; set; }
        public decimal mes_real { get; set; }
        public decimal acumulado_meta { get; set; }
        public decimal acumulado_real { get; set; }
        public decimal mes_meta_anterior { get; set; }
    }
}
