namespace Galileo_API.Models.ProGrX_Polizas
{
    /// <summary>
    /// Request para preparar filtros de reporte (spPoliza_Report_Filtro_Add)
    /// del formulario frmPoliza_Reclamo_Informes.
    /// </summary>
    public class PolizaReclamoInformesPrepararFiltrosRequest
    {
        /// <summary>
        /// Código de póliza (se usa si se requiere cargar "todos" motivos/causas desde DB).
        /// </summary>
        public string? codPoliza { get; set; }

        /// <summary>
        /// Indica si se deben incluir todos los estados.
        /// </summary>
        public bool todosEstados { get; set; } = true;

        /// <summary>
        /// Códigos de estados seleccionados. Si viene vacío y todosEstados=false, no se insertan.
        /// </summary>
        public List<object>? estados { get; set; } = new();

        /// <summary>
        /// Indica si se deben incluir todos los motivos.
        /// </summary>
        public bool todosMotivos { get; set; } = true;

        /// <summary>
        /// Códigos de motivos seleccionados.
        /// </summary>
        public List<object>? motivos { get; set; } = new();

        /// <summary>
        /// Indica si se deben incluir todas las causas.
        /// </summary>
        public bool todasCausas { get; set; } = true;

        /// <summary>
        /// Códigos de causas seleccionadas.
        /// </summary>
        public List<object>? causas { get; set; } = new();
    }

}
