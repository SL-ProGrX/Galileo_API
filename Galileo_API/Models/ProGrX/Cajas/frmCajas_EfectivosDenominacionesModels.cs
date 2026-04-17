namespace Galileo.Models.ProGrX.Cajas
{
    public class CajasEfectivosDenominacionesData
    {
        public string cod_divisa { get; set; } = string.Empty;
        public decimal denominacion { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; } = true;
        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
        public bool isNew { get; set; } = false;
    }

}