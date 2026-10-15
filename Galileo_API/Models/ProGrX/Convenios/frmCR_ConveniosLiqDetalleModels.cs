namespace Galileo_API.Models.ProGrX.Convenios
{
    /// <summary>
    /// Encabezado del detalle de liquidación (sbConsultaConvenio).
    /// </summary>
    public class CrLiqDetalleCabecera
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cod_orden { get; set; }
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
    }

    /// <summary>
    /// Institución disponible para filtrar (cboInstitucion).
    /// </summary>
    public class CrLiqDetalleInstitucion
    {
        public int codigo { get; set; }
        public string institucion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Columna del listado; el VB6 arma los ColumnHeaders según el rubro y el tipo de informe.
    /// </summary>
    public class CrLiqDetalleColumna
    {
        public string field { get; set; } = string.Empty;
        public string header { get; set; } = string.Empty;

        /// <summary>left, right o center (vbRightJustify / vbCenter del VB6).</summary>
        public string align { get; set; } = "left";
    }

    public class CrLiqDetalleFila
    {
        /// <summary>Valores por columna, indexados por <see cref="CrLiqDetalleColumna.field"/>.</summary>
        public Dictionary<string, object?> valores { get; set; } = new();

        /// <summary>Fila de total por institución (se muestra en negrita).</summary>
        public bool es_total { get; set; }

        /// <summary>Columna que se pinta en azul (Institución en la fila de total); vacío si no aplica.</summary>
        public string? campo_azul { get; set; }
    }

    /// <summary>
    /// Resultado de sbCargos / sbRecaudacion / sbDevolucion / sbCreditosNuevos.
    /// </summary>
    public class CrLiqDetalleConsulta
    {
        public string titulo { get; set; } = string.Empty;
        public string etiqueta_total_1 { get; set; } = string.Empty;
        public string etiqueta_total_2 { get; set; } = string.Empty;
        public string etiqueta_total_3 { get; set; } = string.Empty;

        /// <summary>false en Cargos: sólo se muestra el primer total.</summary>
        public bool muestra_comision { get; set; }

        public decimal total_1 { get; set; }
        public decimal total_2 { get; set; }
        public decimal total_3 { get; set; }
        public List<CrLiqDetalleColumna> columnas { get; set; } = new();
        public List<CrLiqDetalleFila> filas { get; set; } = new();
    }

    public class CrLiqDetalleArchivo
    {
        public string nombre { get; set; } = string.Empty;
        public byte[] contenido { get; set; } = Array.Empty<byte>();
    }
}
