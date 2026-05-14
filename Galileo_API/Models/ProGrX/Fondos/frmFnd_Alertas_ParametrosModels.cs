namespace Galileo.Models.ProGrX.Fondos
{
    public class FndalertasData
    {
        public int idregistro { get; set; }
        public int cod_operadora { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public string? unidadtiempo { get; set; } = string.Empty;
        public int? alertaroja { get; set; }
        public int? alertaamarilla { get; set; }
        public string? contacto_oficina { get; set; } = string.Empty;
        public string? contacto_telefono { get; set; } = string.Empty;
        public string? contacto_email { get; set; } = string.Empty;
        public DateTime? fechainserta { get; set; }
        public string? usuarioinserta { get; set; } = string.Empty;
        public DateTime? fechaactualiza { get; set; }
        public string? usuarioactualiza { get; set; } = string.Empty;
        public string? unidadtiempoesp { get; set; } = string.Empty;

        public string descripcion { get; set; } = string.Empty;
    }

    public class FndAlertasContactosDto
    {
        public int idregistro { get; set; }
        public string email { get; set; } = string.Empty;
        public DateTime fechainserta { get; set; }
        public string usuarioinserta { get; set; } = string.Empty;
    }

}
