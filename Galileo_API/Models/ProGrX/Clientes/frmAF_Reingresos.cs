namespace PgxAPI.Models.ProGrX.Clientes
{
    namespace TuProyecto.Core.Models
    {
        public class AF_Prsona_ActivacionDTO
        {
            public string cedula { get; set; } = string.Empty;
            public int pri_deduc { get; set; }
            public string usuario { get; set; } = string.Empty;
            public int id_promotor { get; set; }
            public string cod_oficina { get; set; } = string.Empty;
            public string boleta { get; set; } = string.Empty;
        }
    }


}
