namespace Galileo.Models.GEN
{
    /// <summary>
    /// Fila de enlace institución - línea de crédito.
    /// Homologado a rs! de sbCargaLsw (frmGenEnlacesCredito).
    /// </summary>
    public class GenEnlacesCreditoData
    {
        public required int cod_institucion { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string cod_credito { get; set; } = string.Empty;
    }

    public class GenEnlacesCreditoLista
    {
        public int total { get; set; }
        public List<GenEnlacesCreditoData> lista { get; set; } = new List<GenEnlacesCreditoData>();
    }
}
