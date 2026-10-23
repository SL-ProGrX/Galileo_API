namespace Galileo.Models.ProGrX_Inversiones
{
    public sealed class IvrCatReservaDto
    {
        public string Cod_Reserva { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activa { get; set; }
    }

    public sealed class IvrCatReservaGuardarRequest
    {
        public string Cod_Reserva { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool? Activa { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public sealed class IvrCatReservaCuentaDto
    {
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cod_Cuenta_Mask { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }

    public sealed class IvrCatReservaCuentaRequest
    {
        public string Cod_Reserva { get; set; } = string.Empty;
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Cuenta_Anterior { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }
}
