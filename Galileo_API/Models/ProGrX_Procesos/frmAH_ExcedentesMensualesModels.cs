namespace PgxAPI.Models
{

    public class excParametrosDTO
    {
        public string Cod_Parametro { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }


    public class excPeriodosDTO
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;

    }

    public class excPeriodosCorte
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;

    }

    public class ResumenExcedenteMDTO
    {
        public string id_periodo { get; set; } = string.Empty;
        public string corte { get; set; } = string.Empty;
        public string corte_date_str { get; set; } = string.Empty;
        public string corte_datetime_str { get; set; } = string.Empty;
        public string casos { get; set; } = string.Empty;

        public string total { get; set; } = string.Empty;
        public string bruto { get; set; } = string.Empty;

    }


}
