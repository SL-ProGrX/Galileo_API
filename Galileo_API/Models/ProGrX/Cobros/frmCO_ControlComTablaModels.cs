namespace Galileo.Models.ProGrX.Cobros
{
    public class CoControlComTablaLista
    {
        public List<CoControlComTablaData> lista { get; set; } = new();
        public int total { get; set; } = 0;
    }

    public class CoControlComTablaData
    {
        public int id_linea { get; set; }
        public int inicio { get; set; }
        public int corte { get; set; }
        public decimal porcentaje { get; set; } = 0;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public bool isNew { get; set; } = false;
    }
}