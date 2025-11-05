namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AFCambioCedulaDto
    {
        public string? cedulaNueva { get; set; }
        public string? cedulaAnterior { get; set; }
        public string? tipo { get; set; }
        public string? nombre { get; set; }
        public string? nombreNuevo { get; set; }
    }

    public class AFCedulaCambioDto
    {
        public string? cedulaActual { get; set; }
        public string? tipoId { get; set; }
        public string? tipoId_Desc { get; set; }
        public string? nombre { get; set; }
        public string? estado { get; set; }
        public string? estado_Persona { get; set; }
    }

    public class AFUsuarioLogonDto
    {
        public string? usuario { get; set; }
        public string? clave { get; set; }
    }
}