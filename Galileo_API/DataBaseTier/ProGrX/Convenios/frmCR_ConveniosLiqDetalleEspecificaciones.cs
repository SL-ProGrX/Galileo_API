namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    internal enum CrLiqDetalleRubro
    {
        Cargos,
        Recaudacion,
        Devolucion,
        Creditos
    }

    internal enum CrLiqDetalleFormato
    {
        Texto,
        Monto,
        Fecha,
        Crudo
    }

    /// <param name="Header">Encabezado de la columna (ColumnHeaders.Add).</param>
    /// <param name="Campo">Campo del SP; null cuando la celda queda vacía en las filas de detalle.</param>
    internal sealed record CrLiqDetalleColumnaSpec(
        string Header,
        string? Campo,
        string Align = "left",
        CrLiqDetalleFormato Formato = CrLiqDetalleFormato.Texto);

    /// <param name="Sp">SP para todas las instituciones.</param>
    /// <param name="SpInstitucion">SP filtrado por institución; null si el VB6 no filtra.</param>
    /// <param name="TieneInforme">Si el SP recibe el tipo de informe (R / D).</param>
    /// <param name="CampoTotal">Campo que acumula el total 1.</param>
    /// <param name="CampoComision">Campo que acumula la comisión; null si no aplica.</param>
    /// <param name="AgrupaInstitucion">Genera filas de total cuando cambia la institución (informe Resumen).</param>
    /// <param name="IndiceInstitucion">Índice de la columna Institución en la fila de total.</param>
    internal sealed record CrLiqDetalleConsultaSpec(
        string Sp,
        string? SpInstitucion,
        bool TieneInforme,
        string CampoTotal,
        string? CampoComision,
        bool AgrupaInstitucion,
        int IndiceInstitucion,
        IReadOnlyList<CrLiqDetalleColumnaSpec> Columnas);

    /// <summary>
    /// Columnas y SP de cada combinación rubro / informe, tomados de sbCargos, sbRecaudacion,
    /// sbDevolucion y sbCreditosNuevos del formulario VB6.
    /// </summary>
    internal static class CrLiqDetalleEspecificaciones
    {
        private const string Left = "left";
        private const string Right = "right";
        private const string Center = "center";

        // Nombres de campos del SP (lado derecho del mapeo).
        private const string CampoCargo = "cargo";
        private const string CampoMonto = "Monto";
        private const string CampoCedula = "Cedula";
        private const string CampoNombre = "Nombre";
        private const string CampoCodigo = "Codigo";
        private const string CampoComision = "Comision";
        private const string CampoInstitucion = "Institucion";
        private const string CampoDestino = "Destino";
        private const string CampoTotalRecaudado = "TOTAL_RECAUDADO";
        private const string CampoTotalNeto = "TOTAL_NETO";

        // Encabezados de columna (lado izquierdo del mapeo).
        private const string HeaderMonto = "Monto";
        private const string HeaderDescripcion = "Descripción";
        private const string HeaderRecaudado = "Recaudado";
        private const string HeaderComision = "Comisión";
        private const string HeaderLinea = "Línea";
        private const string HeaderIdentificacion = "Identificación";
        private const string HeaderNombre = "Nombre";
        private const string HeaderInstitucion = "Institución";
        private const string HeaderDestino = "Destino";

        public static CrLiqDetalleConsultaSpec Obtener(CrLiqDetalleRubro rubro, char informe)
            => rubro switch
            {
                CrLiqDetalleRubro.Cargos => informe == 'R' ? CargosResumen : CargosDetalle,
                CrLiqDetalleRubro.Recaudacion => informe switch
                {
                    'R' => RecaudacionResumen,
                    'D' => RecaudacionDetalle,
                    _ => RecaudacionInstitucion
                },
                CrLiqDetalleRubro.Devolucion => informe switch
                {
                    'R' => DevolucionResumen,
                    'D' => DevolucionDetalle,
                    _ => DevolucionInstitucion
                },
                _ => informe switch
                {
                    'R' => CreditosResumen,
                    'D' => CreditosDetalle,
                    _ => CreditosInstitucion
                }
            };

        private static CrLiqDetalleColumnaSpec Col(
            string header,
            string? campo,
            string align = Left,
            CrLiqDetalleFormato formato = CrLiqDetalleFormato.Texto)
            => new(header, campo, align, formato);

        private static CrLiqDetalleColumnaSpec Monto(string header, string campo)
            => Col(header, campo, Right, CrLiqDetalleFormato.Monto);

        private static CrLiqDetalleColumnaSpec Fecha(string header, string campo, string align = Left)
            => Col(header, campo, align, CrLiqDetalleFormato.Fecha);

        // Cargos de formalización: spConvenios_Orden_Cargos_Formaliza (no filtra por institución).
        private static readonly CrLiqDetalleConsultaSpec CargosResumen = new(
            "spConvenios_Orden_Cargos_Formaliza", null, true, CampoCargo, null, false, -1,
            new[]
            {
                Col("Cargo", "cod_cargo"),
                Col(HeaderDescripcion, "Descripcion"),
                Monto(HeaderMonto, CampoCargo)
            });

        private static readonly CrLiqDetalleConsultaSpec CargosDetalle = new(
            "spConvenios_Orden_Cargos_Formaliza", null, true, CampoCargo, null, false, -1,
            new[]
            {
                Col("Cargo", "cod_cargo"),
                Col(HeaderDescripcion, "Descripcion"),
                Monto(HeaderRecaudado, CampoCargo),
                Col("Operación", "id_Solicitud"),
                Col(HeaderLinea, "LineaDesc"),
                Col("Garantía", "Garantia"),
                Col(HeaderIdentificacion, CampoCedula, Center),
                Col(HeaderNombre, CampoNombre),
                Col("Mnt. Crd", "montoapr", Left, CrLiqDetalleFormato.Crudo),
                Col("Mnt. Girado.", "monto_girado", Right, CrLiqDetalleFormato.Crudo),
                Fecha("Formalizado", "fechaforp", Right),
                Fecha("Revisado", "Fecha_Revision"),
                Col("Línea.Cod.", CampoCodigo)
            });

        // Recaudación (también Comisión Recaudación y Reservas).
        private static readonly CrLiqDetalleConsultaSpec RecaudacionResumen = new(
            "spConvenios_Orden_Recaudacion", "spConvenios_Orden_Recaudacion_Institucion", true,
            CampoTotalRecaudado, CampoComision, true, 5,
            new[]
            {
                Col(HeaderLinea, CampoCodigo),
                Col(HeaderDescripcion, "LineaX"),
                Monto(HeaderRecaudado, CampoTotalRecaudado),
                Monto(HeaderComision, CampoComision),
                Monto("Neto", CampoTotalNeto),
                Col(HeaderInstitucion, null)
            });

        private static readonly CrLiqDetalleConsultaSpec RecaudacionDetalle = new(
            "spConvenios_Orden_Recaudacion", "spConvenios_Orden_Recaudacion_Institucion", true,
            CampoMonto, CampoComision, false, -1,
            new[]
            {
                Col("Operación", "id_Solicitud"),
                Col(HeaderLinea, CampoCodigo),
                Monto(HeaderRecaudado, CampoMonto),
                Fecha("Fecha", "fecha", Right),
                Col(HeaderDescripcion, "LineaX"),
                Col("Garantía", "GarantiaDesc"),
                Col(HeaderIdentificacion, CampoCedula, Center),
                Col(HeaderNombre, CampoNombre),
                Col("Concepto", "concepto"),
                Col("Tipo Doc.", "Tcon"),
                Col("Documento", "Ncon"),
                Col(HeaderInstitucion, CampoInstitucion)
            });

        private static readonly CrLiqDetalleConsultaSpec RecaudacionInstitucion = new(
            "spConvenios_Orden_Recaudacion_Institucion_Rsm", null, false,
            CampoTotalRecaudado, CampoComision, false, -1,
            new[]
            {
                Col("Código", CampoCodigo),
                Col(HeaderDescripcion, CampoInstitucion),
                Monto(HeaderRecaudado, CampoTotalRecaudado),
                Monto(HeaderComision, CampoComision),
                Monto("Neto", CampoTotalNeto)
            });

        // Devoluciones (también Comisión Devolución).
        private static readonly CrLiqDetalleConsultaSpec DevolucionResumen = new(
            "spConvenios_Orden_Devolucion", "spConvenios_Orden_Devolucion_Institucion", true,
            CampoTotalRecaudado, CampoComision, true, 5,
            new[]
            {
                Col("Concepto", "Retencion_Codigo"),
                Col(HeaderDescripcion, "RetencionConcepto"),
                Monto(HeaderRecaudado, CampoTotalRecaudado),
                Monto(HeaderComision, CampoComision),
                Monto("Neto", CampoTotalNeto),
                Col(HeaderInstitucion, null)
            });

        private static readonly CrLiqDetalleConsultaSpec DevolucionDetalle = new(
            "spConvenios_Orden_Devolucion", "spConvenios_Orden_Devolucion_Institucion", true,
            CampoMonto, CampoComision, false, -1,
            new[]
            {
                Col("Num.Liq.", "Consec"),
                Col("Concepto", "Retencion_Codigo"),
                Monto(HeaderRecaudado, CampoMonto),
                Fecha("Fecha", "fecha", Right),
                Col(HeaderDescripcion, "RetencionConcepto"),
                Col(HeaderIdentificacion, CampoCedula, Center),
                Col(HeaderNombre, CampoNombre),
                Col("Operadora", "cod_Operadora"),
                Col("Plan", "PlanDesc"),
                Col("Contrato", "cod_Contrato"),
                Col(HeaderInstitucion, CampoInstitucion)
            });

        private static readonly CrLiqDetalleConsultaSpec DevolucionInstitucion = new(
            "spConvenios_Orden_Devolucion_Institucion_Rsm", null, false,
            CampoTotalRecaudado, CampoComision, false, -1,
            new[]
            {
                Col("Código", CampoCodigo),
                Col(HeaderDescripcion, CampoInstitucion),
                Monto(HeaderRecaudado, CampoTotalRecaudado),
                Monto(HeaderComision, CampoComision),
                Monto("Neto", CampoTotalNeto)
            });

        // Nuevos créditos (también Comisión Créditos).
        private static readonly CrLiqDetalleConsultaSpec CreditosResumen = new(
            "spConvenios_Orden_Creditos", "spConvenios_Orden_Creditos_Institucion", true,
            CampoMonto, CampoComision, true, 6,
            new[]
            {
                Col(HeaderLinea, CampoCodigo),
                Col(HeaderDescripcion, "LineaDesc"),
                Monto(HeaderMonto, CampoMonto),
                Monto(HeaderComision, CampoComision),
                Monto("Neto", "Neto"),
                Col(HeaderDestino, CampoDestino),
                Col(HeaderInstitucion, CampoInstitucion)
            });

        private static readonly CrLiqDetalleConsultaSpec CreditosDetalle = new(
            "spConvenios_Orden_Creditos", "spConvenios_Orden_Creditos_Institucion", true,
            CampoMonto, CampoComision, false, -1,
            new[]
            {
                Col("Operación", "id_Solicitud"),
                Col(HeaderLinea, CampoCodigo, Center),
                Col(HeaderDescripcion, "LineaDesc"),
                Col(HeaderDestino, CampoDestino),
                Monto(HeaderMonto, CampoMonto),
                Monto(HeaderComision, CampoComision),
                Monto("Neto", "Neto"),
                Fecha("Fec.Rev.", "Fecha_Revision"),
                Fecha("Fec.Form.", "fechaforp"),
                Col("No.Documento", "Documento"),
                Col("Garantía", "Garantia"),
                Col(HeaderIdentificacion, CampoCedula, Center),
                Col(HeaderNombre, CampoNombre),
                Col(HeaderInstitucion, "InstitucionDesc")
            });

        private static readonly CrLiqDetalleConsultaSpec CreditosInstitucion = new(
            "spConvenios_Orden_Creditos_Institucion_Rsm", null, false,
            CampoMonto, CampoComision, false, -1,
            new[]
            {
                Col("Código", "cod_Institucion"),
                Col(HeaderDescripcion, CampoInstitucion),
                Monto(HeaderMonto, CampoMonto),
                Monto(HeaderComision, CampoComision),
                Monto("Neto", "Neto")
            });
    }
}
