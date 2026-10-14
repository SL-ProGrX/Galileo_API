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
        private const string Right = "right";
        private const string Center = "center";

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
            string align = "left",
            CrLiqDetalleFormato formato = CrLiqDetalleFormato.Texto)
            => new(header, campo, align, formato);

        private static CrLiqDetalleColumnaSpec Monto(string header, string campo)
            => Col(header, campo, Right, CrLiqDetalleFormato.Monto);

        private static CrLiqDetalleColumnaSpec Fecha(string header, string campo, string align = "left")
            => Col(header, campo, align, CrLiqDetalleFormato.Fecha);

        // Cargos de formalización: spConvenios_Orden_Cargos_Formaliza (no filtra por institución).
        private static readonly CrLiqDetalleConsultaSpec CargosResumen = new(
            "spConvenios_Orden_Cargos_Formaliza", null, true, "cargo", null, false, -1,
            new[]
            {
                Col("Cargo", "cod_cargo"),
                Col("Descripción", "Descripcion"),
                Monto("Monto", "cargo")
            });

        private static readonly CrLiqDetalleConsultaSpec CargosDetalle = new(
            "spConvenios_Orden_Cargos_Formaliza", null, true, "cargo", null, false, -1,
            new[]
            {
                Col("Cargo", "cod_cargo"),
                Col("Descripción", "Descripcion"),
                Monto("Recaudado", "cargo"),
                Col("Operación", "id_Solicitud"),
                Col("Línea", "LineaDesc"),
                Col("Garantía", "Garantia"),
                Col("Identificación", "Cedula", Center),
                Col("Nombre", "Nombre"),
                Col("Mnt. Crd", "montoapr", "left", CrLiqDetalleFormato.Crudo),
                Col("Mnt. Girado.", "monto_girado", Right, CrLiqDetalleFormato.Crudo),
                Fecha("Formalizado", "fechaforp", Right),
                Fecha("Revisado", "Fecha_Revision"),
                Col("Línea.Cod.", "Codigo")
            });

        // Recaudación (también Comisión Recaudación y Reservas).
        private static readonly CrLiqDetalleConsultaSpec RecaudacionResumen = new(
            "spConvenios_Orden_Recaudacion", "spConvenios_Orden_Recaudacion_Institucion", true,
            "TOTAL_RECAUDADO", "Comision", true, 5,
            new[]
            {
                Col("Línea", "Codigo"),
                Col("Descripción", "LineaX"),
                Monto("Recaudado", "TOTAL_RECAUDADO"),
                Monto("Comisión", "Comision"),
                Monto("Neto", "TOTAL_NETO"),
                Col("Institución", null)
            });

        private static readonly CrLiqDetalleConsultaSpec RecaudacionDetalle = new(
            "spConvenios_Orden_Recaudacion", "spConvenios_Orden_Recaudacion_Institucion", true,
            "Monto", "Comision", false, -1,
            new[]
            {
                Col("Operación", "id_Solicitud"),
                Col("Línea", "Codigo"),
                Monto("Recaudado", "Monto"),
                Fecha("Fecha", "fecha", Right),
                Col("Descripción", "LineaX"),
                Col("Garantía", "GarantiaDesc"),
                Col("Identificación", "Cedula", Center),
                Col("Nombre", "Nombre"),
                Col("Concepto", "concepto"),
                Col("Tipo Doc.", "Tcon"),
                Col("Documento", "Ncon"),
                Col("Institución", "Institucion")
            });

        private static readonly CrLiqDetalleConsultaSpec RecaudacionInstitucion = new(
            "spConvenios_Orden_Recaudacion_Institucion_Rsm", null, false,
            "TOTAL_RECAUDADO", "Comision", false, -1,
            new[]
            {
                Col("Código", "Codigo"),
                Col("Descripción", "Institucion"),
                Monto("Recaudado", "TOTAL_RECAUDADO"),
                Monto("Comisión", "Comision"),
                Monto("Neto", "TOTAL_NETO")
            });

        // Devoluciones (también Comisión Devolución).
        private static readonly CrLiqDetalleConsultaSpec DevolucionResumen = new(
            "spConvenios_Orden_Devolucion", "spConvenios_Orden_Devolucion_Institucion", true,
            "TOTAL_RECAUDADO", "Comision", true, 5,
            new[]
            {
                Col("Concepto", "Retencion_Codigo"),
                Col("Descripción", "RetencionConcepto"),
                Monto("Recaudado", "TOTAL_RECAUDADO"),
                Monto("Comisión", "Comision"),
                Monto("Neto", "TOTAL_NETO"),
                Col("Institución", null)
            });

        private static readonly CrLiqDetalleConsultaSpec DevolucionDetalle = new(
            "spConvenios_Orden_Devolucion", "spConvenios_Orden_Devolucion_Institucion", true,
            "Monto", "Comision", false, -1,
            new[]
            {
                Col("Num.Liq.", "Consec"),
                Col("Concepto", "Retencion_Codigo"),
                Monto("Recaudado", "Monto"),
                Fecha("Fecha", "fecha", Right),
                Col("Descripción", "RetencionConcepto"),
                Col("Identificación", "Cedula", Center),
                Col("Nombre", "Nombre"),
                Col("Operadora", "cod_Operadora"),
                Col("Plan", "PlanDesc"),
                Col("Contrato", "cod_Contrato"),
                Col("Institución", "Institucion")
            });

        private static readonly CrLiqDetalleConsultaSpec DevolucionInstitucion = new(
            "spConvenios_Orden_Devolucion_Institucion_Rsm", null, false,
            "TOTAL_RECAUDADO", "Comision", false, -1,
            new[]
            {
                Col("Código", "Codigo"),
                Col("Descripción", "Institucion"),
                Monto("Recaudado", "TOTAL_RECAUDADO"),
                Monto("Comisión", "Comision"),
                Monto("Neto", "TOTAL_NETO")
            });

        // Nuevos créditos (también Comisión Créditos).
        private static readonly CrLiqDetalleConsultaSpec CreditosResumen = new(
            "spConvenios_Orden_Creditos", "spConvenios_Orden_Creditos_Institucion", true,
            "Monto", "Comision", true, 6,
            new[]
            {
                Col("Línea", "Codigo"),
                Col("Descripción", "LineaDesc"),
                Monto("Monto", "Monto"),
                Monto("Comisión", "Comision"),
                Monto("Neto", "Neto"),
                Col("Destino", "Destino"),
                Col("Institución", "Institucion")
            });

        private static readonly CrLiqDetalleConsultaSpec CreditosDetalle = new(
            "spConvenios_Orden_Creditos", "spConvenios_Orden_Creditos_Institucion", true,
            "Monto", "Comision", false, -1,
            new[]
            {
                Col("Operación", "id_Solicitud"),
                Col("Línea", "Codigo", Center),
                Col("Descripción", "LineaDesc"),
                Col("Destino", "Destino"),
                Monto("Monto", "Monto"),
                Monto("Comisión", "Comision"),
                Monto("Neto", "Neto"),
                Fecha("Fec.Rev.", "Fecha_Revision"),
                Fecha("Fec.Form.", "fechaforp"),
                Col("No.Documento", "Documento"),
                Col("Garantía", "Garantia"),
                Col("Identificación", "Cedula", Center),
                Col("Nombre", "Nombre"),
                Col("Institución", "InstitucionDesc")
            });

        private static readonly CrLiqDetalleConsultaSpec CreditosInstitucion = new(
            "spConvenios_Orden_Creditos_Institucion_Rsm", null, false,
            "Monto", "Comision", false, -1,
            new[]
            {
                Col("Código", "cod_Institucion"),
                Col("Descripción", "Institucion"),
                Monto("Monto", "Monto"),
                Monto("Comisión", "Comision"),
                Monto("Neto", "Neto")
            });
    }
}
