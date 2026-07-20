namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrComisionesBancosItem
    {
        public int id_banco { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public bool cheques { get; set; } = false;
        public bool transferencias { get; set; } = false;
    }

    public class CrComisionesBancosInicializarRequest
    {
        public string usuario { get; set; } = string.Empty;
    }

    public class CrComisionesBancosActualizarRequest
    {
        public int id_banco { get; set; } = 0;
        public string tipo { get; set; } = string.Empty;
        public bool valor { get; set; } = false;
    }
}