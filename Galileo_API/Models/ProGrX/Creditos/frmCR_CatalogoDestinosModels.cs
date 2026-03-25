namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrCatalogoDestinoData
    {
        public string cod_destino { get; set; } = string.Empty; 
        public string descripcion { get; set; } = string.Empty;
        public decimal tasa { get; set; } = 0;
        public bool tbp { get; set; } = false;
        public bool int_form { get; set; } = false;
        public string tipocbrint { get; set; } = string.Empty;
        public bool primer_cuota { get; set; } = false;
        public bool envio_tesoreria { get; set; } = false;
        public int prioridad { get; set; } = 0;
        public bool existe { get; set; } = false;
    }
}
