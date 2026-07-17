namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrSeguimientoEtiquetasData
    {
        public short Linea { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public int Id_Solicitud { get; set; }
        public string Tag_Codigo { get; set; } = string.Empty;
        public string Asignado_A { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public string Etiqueta { get; set; } = string.Empty;
    }

    public class CrSeguimientoEtiquetasAplicarRequest
    {
        public int Operacion { get; set; }
        public string Linea { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Asignado { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
    }
}
