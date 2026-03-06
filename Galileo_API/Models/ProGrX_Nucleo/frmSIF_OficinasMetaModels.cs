
namespace Galileo.Models.ProGrX_Nucleo
{
    public class SifOficinasMetaLista
    {
        public int total { get; set; }
        public List<SifOficinasMetaData> lista { get; set; } = new List<SifOficinasMetaData>();
    }

    public class SifOficinasMetaData
    {

        public string cod_oficina { get; set; } = string.Empty;
        public required int anio { get; set; }
        public required int mes { get; set; }
        public required decimal mes_meta { get; set; }
        public required decimal mes_real { get; set; }
        public required decimal acumulado_meta { get; set; }
        public required decimal acumulado_real { get; set; }
        public required decimal mes_meta_anterior { get; set; }
    }
}