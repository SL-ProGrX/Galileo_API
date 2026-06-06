namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrPrendasTipoData
    {
        public string tipo_prenda { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string formulario { get; set; } = string.Empty;
        public decimal porc_cobertura { get; set; } = 0;
        public bool activa { get; set; } = false;
    }

    public class CrPrendasTipoGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public CrPrendasTipoData tipo { get; set; } = new();
    }

    public class CrPrendasTipoEliminarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string tipo_prenda { get; set; } = string.Empty;
    }

    public class CrPrendasTipoAsignacionData
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public bool asignado { get; set; } = false;
    }

    public class CrPrendasTipoAsignacionObtenerRequest
    {
        public string tipo_prenda { get; set; } = string.Empty;
        public string categoria { get; set; } = string.Empty;
        public string filtro { get; set; } = string.Empty;
    }

    public class CrPrendasTipoAsignacionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string tipo_prenda { get; set; } = string.Empty;
        public string categoria { get; set; } = string.Empty;
        public string idx { get; set; } = string.Empty;
        public bool asignado { get; set; } = false;
    }
}