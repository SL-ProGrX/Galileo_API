namespace Galileo.Models.INV
{
    /// <summary>
    /// Existencia del producto en una bodega. Homologado a rs!cod_bodega / rs!descripcion del VB6.
    /// </summary>
    public class InvExistenciaProductoBodegaDto
    {
        public string cod_bodega { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal existencia { get; set; }
    }

    /// <summary>
    /// Resultado de la consulta: existencias por bodega y total general.
    /// </summary>
    public class InvExistenciaProductoResultadoDto
    {
        public List<InvExistenciaProductoBodegaDto> bodegas { get; set; } = [];
        public decimal total_existencia { get; set; }
    }

    /// <summary>
    /// Datos de consulta de existencia al corte.
    /// </summary>
    public class InvExistenciaProductoConsulta
    {
        public string cod_producto { get; set; } = string.Empty;
        public string fecha_corte { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}
