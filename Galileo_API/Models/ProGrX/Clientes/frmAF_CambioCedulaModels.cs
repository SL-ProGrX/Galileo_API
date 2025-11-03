namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AF_CambioCedulaDTO
    {
        public string? cedulaNueva { get; set; }
        public string? cedulaAnterior { get; set; }
        public string? tipo { get; set; }
        public string? nombre { get; set; }
        public string? nombreNuevo { get; set; }
    }


    public class AF_CedulaCambioDTO
    {
        public string? cedulaActual { get; set; }
        public string? tipoId { get; set; }
        public string? tipoId_Desc { get; set; }
        public string? nombre { get; set; }
        public string? estado { get; set; }
        public string? estado_Persona { get; set; }
    }

    public class AF_UsuarioLogonDTO
    {
        public string? usuario { get; set; }
        public string? clave { get; set; }
    }

}
