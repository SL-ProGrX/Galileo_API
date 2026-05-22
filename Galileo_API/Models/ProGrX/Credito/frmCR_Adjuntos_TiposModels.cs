namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrAdjuntoTipoData
    {
        public string cod_adjunto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class CrAdjuntoTipoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public CrAdjuntoTipoData tipo { get; set; } = new();
    }

    public class CrAdjuntoTipoEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_adjunto { get; set; } = string.Empty;
    }
}