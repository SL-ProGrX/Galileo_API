namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrComitesParametroModel
    {
        public string Cod_Parametro { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
    }

    public class CrComitesParametroActualizarRequest
    {
        public string Cod_Parametro { get; set; } = string.Empty;
        public string Valor { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }
}
