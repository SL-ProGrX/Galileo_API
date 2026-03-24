namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrCatalogoDestinoData
    {
        public string cod_destino { get; set; } = string.Empty; 
        public string descripcion { get; set; } = string.Empty;
        public decimal tasa { get; set; } = 0;
        public int tbp { get; set; } = 0;
        public int int_form { get; set; } = 0;
        public string tipocbrint { get; set; } = string.Empty;
        public int primer_cuota { get; set; } = 0;
        public int envio_tesoreria { get; set; } = 0;
        public int prioridad { get; set; } = 0;
        public bool existe { get; set; } = false;
    }
}
