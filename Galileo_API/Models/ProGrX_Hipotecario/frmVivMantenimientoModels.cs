namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class VivMantenimientoNodoData
    {
        public string key { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public string tag { get; set; } = string.Empty;
        public string icon { get; set; } = string.Empty;
        public string formulario { get; set; } = string.Empty;
        public string ruta { get; set; } = string.Empty;
        public bool leaf { get; set; }
    }

    public class VivMantenimientoListaData
    {
        public string key { get; set; } = string.Empty;
        public string columna_1 { get; set; } = string.Empty;
        public string columna_2 { get; set; } = string.Empty;
        public string columna_3 { get; set; } = string.Empty;
        public string columna_4 { get; set; } = string.Empty;
        public string columna_5 { get; set; } = string.Empty;
        public string columna_6 { get; set; } = string.Empty;
        public string columna_7 { get; set; } = string.Empty;
        public string columna_8 { get; set; } = string.Empty;
        public string columna_9 { get; set; } = string.Empty;
        public string columna_10 { get; set; } = string.Empty;
        public string columna_11 { get; set; } = string.Empty;
    }

    public class VivMantenimientoZonaContactoAsignarRequest
    {
        public required long idZona { get; set; }
        public required long idContacto { get; set; }
        public required bool asignar { get; set; }
        public string usuario { get; set; } = string.Empty;
    }
}
