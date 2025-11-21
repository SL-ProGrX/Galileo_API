namespace Galileo.Models.ProGrX.Clientes
{
    public class AfParametrosDto
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
    }

    public class AfParametrosLista
    {
        public int total { get; set; }
        public List<AfParametrosDto> lista { get; set; } = new List<AfParametrosDto>();
    }
}