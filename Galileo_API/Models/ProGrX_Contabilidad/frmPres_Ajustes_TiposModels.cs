namespace PgxAPI.Models.PRES
{

    public class PresAjustestTiposDTO
    {
        public string Cod_Ajuste { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public bool Ajuste_Libre_Positivo { get; set; }
        public bool Ajuste_Libre_Negativo { get; set; }
        public bool Ajuste_Entre_Cuentas { get; set; }
        public bool Ajuste_Cta_Dif_Naturaleza { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class PresAjustestTiposLista
    {
        public int total { get; set; }
        public List<PresAjustestTiposDTO> lista { get; set; } = new List<PresAjustestTiposDTO>();
    }

}
