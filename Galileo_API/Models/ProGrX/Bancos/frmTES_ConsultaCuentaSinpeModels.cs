namespace Galileo_API.Models.ProGrX.Bancos
{
    public class TesConsultaCuentaSinpeModels
    {
        public required string cedula { get; set; }
        public string nombre { get; set; } = string.Empty;
        public int tipoId { get; set; } = 0;
        public string cuentaBancoSif { get; set; } = string.Empty;
        public required string idBanco { get; set; }
        public required string cuentaIban { get; set; }
        public string? detalle { get; set; }
        public bool cerrada { get; set; } = false;
        public string usuario { get; set; } = string.Empty;
        public int error { get; set; } = 0;

    }
}
