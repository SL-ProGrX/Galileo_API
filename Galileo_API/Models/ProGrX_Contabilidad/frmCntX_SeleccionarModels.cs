namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXSeleccionarContabilidadItem
    {
        public int cod_contabilidad { get; set; }
        public string nombre { get; set; } = string.Empty;
    }

    public class CntXSeleccionarCargaResponse
    {
        public bool requiereCrearContabilidad { get; set; }
        public string mensaje { get; set; } = string.Empty;
        public List<CntXSeleccionarContabilidadItem> contabilidades { get; set; } = new();
        public CntXParametrosDto? contabilidadSeleccionada { get; set; }
    }
}
