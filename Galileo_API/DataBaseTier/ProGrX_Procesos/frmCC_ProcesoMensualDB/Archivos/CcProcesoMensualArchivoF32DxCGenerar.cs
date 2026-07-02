using System.Data;
using System.Globalization;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualArchivosModels;
using Microsoft.Extensions.Options;
using ClosedXML.Excel;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Archivos
{
    public class CcProcesoMensualArchivoF32DxCGenerar : CcProcesoMensualArchivoConMovimientosGeneratorBase<CcProcesoMensualArchivoF32DxCGenerar.CcProcesoMensualArchivoF32RegistroDbModel>

    {
        private const string CodigoDeduccionCantidad = "DE31";
        private const string Encabezado = "empleado;concepto;cantidad;monto";

        private string _rutaArchivoExcel = string.Empty;

        private readonly ArchivosGeneradosOptions _archivosOptions;

        public CcProcesoMensualArchivoF32DxCGenerar(IOptions<ArchivosGeneradosOptions> archivosOptions) : base(archivosOptions)
        {
            _archivosOptions = archivosOptions.Value;
        }

        public override IReadOnlyCollection<string> CodigosPlanillaEnvio { get; } = ["32"];

        protected override string CodigoPlanillaEnvio => "32";
        protected override string CodigoFormato => "F32";
        protected override string ExtensionArchivo => ".csv";
        protected override string ContentType => ContentTypeCsv;

        protected override string QueryRegistros => @"
            SELECT
                P.Cod_Deduccion AS CodDeduccion,
                P.Monto_Actual AS MontoActual,
                P.Movimiento,
                S.CedulaR AS CedulaColilla,
                S.Nombre
            FROM prm_planilla P
            INNER JOIN Socios S
                ON P.cedula = S.cedula
            WHERE P.Proceso = @FechaProceso
              AND P.movimiento IN @Movimientos
              AND P.cod_institucion = @CodInstitucion
            ORDER BY P.tipo, P.movimiento, P.cedula";
        protected override void PrepararConfiguracion(IDbConnection connection,
           CcProcesoMensualArchivoConfiguracionModel configuracion,
           CcProcesoMensualGeneraArchivoRequest request)
        {
            var fechaServidor = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerFechaServidor(connection);

            var nombreArchivoExcel = CrearNombreArchivoExcel(
                request.CodInstitucion,
                request.FechaProceso,
                configuracion.CodigoInstDeduc,
                fechaServidor);
            var rutaBase = _archivosOptions.RutaBase;
            var rutaDirectorio = Helpers.CcProcesoMensualArchivoRutaHelperDb.ObtenerRutaPlanilla(request, rutaBase);

            _rutaArchivoExcel = Helpers.CcProcesoMensualArchivoRutaHelperDb.CombinarArchivo(rutaBase,
                rutaDirectorio,
                nombreArchivoExcel);
        }

        protected override string CrearEncabezado()
        {
            return Encabezado;
        }

        protected override string CrearLineaArchivo(
            CcProcesoMensualArchivoF32RegistroDbModel registro,
            CcProcesoMensualGeneraArchivoRequest request)
        {
            var empleado = (registro.CedulaColilla ?? string.Empty).Trim();
            var concepto = registro.CodDeduccion;
            var monto = registro.MontoActual.ToString("0");

            if (string.Equals(
                registro.CodDeduccion?.Trim(),
                CodigoDeduccionCantidad,
                StringComparison.OrdinalIgnoreCase))
            {
                return empleado
                    + ";"
                    + concepto
                    + ";"
                    + monto
                    + ";0";
            }

            return empleado
                + ";"
                + concepto
                + ";0;"
                + monto;
        }
        protected override void GenerarArchivosAdicionales( IReadOnlyCollection<CcProcesoMensualArchivoF32RegistroDbModel> registros, CcProcesoMensualGeneraArchivoRequest request)
        {
            if (string.IsNullOrWhiteSpace(_rutaArchivoExcel))
            {
                return;
            }

            GenerarArchivoExcel(registros, _rutaArchivoExcel);
        }

        private static readonly CultureInfo CulturaMonto = new("es-CR");
        private static void GenerarArchivoExcel( IReadOnlyCollection<CcProcesoMensualArchivoF32RegistroDbModel> registros, string rutaArchivoExcel)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet 1");

            worksheet.Cell(1, 1).Value = "empleado";
            worksheet.Cell(1, 2).Value = "concepto";
            worksheet.Cell(1, 3).Value = "cantidad";
            worksheet.Cell(1, 4).Value = "Monto";

            var fila = 2;

            foreach (var registro in registros)
            {
                var empleado = (registro.CedulaColilla ?? string.Empty).Trim();
                var concepto = (registro.CodDeduccion ?? string.Empty).Trim();
               
                var montoTexto = Math.Round(registro.MontoActual, 2)
          .ToString("0.00", CulturaMonto)
          .Replace(".", ",");

                worksheet.Cell(fila, 1).Value = empleado;
                worksheet.Cell(fila, 2).Value = concepto;

                if (string.Equals(
                    concepto,
                    CodigoDeduccionCantidad,
                    StringComparison.OrdinalIgnoreCase))
                {
                    worksheet.Cell(fila, 3).Value = montoTexto;
                    worksheet.Cell(fila, 4).Value = "0,00";
                }
                else
                {
                    worksheet.Cell(fila, 3).Value = "0,00";
                    worksheet.Cell(fila, 4).Value = montoTexto;
                }

                fila++;
            }

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(rutaArchivoExcel);
        }

        protected override List<string> ObtenerArchivosGenerados(
            string rutaArchivoPrincipal)
        {
            return string.IsNullOrWhiteSpace(_rutaArchivoExcel)
                ? [rutaArchivoPrincipal]
                : [rutaArchivoPrincipal, _rutaArchivoExcel];
        }

        private static string CrearNombreArchivoExcel(
            int codInstitucion,
            decimal fechaProceso,
            string codigoInstDeduc,
            DateTime fechaServidor)
        {
            var codigoInstitucion = string.IsNullOrWhiteSpace(codigoInstDeduc)
                ? codInstitucion.ToString("00", CultureInfo.InvariantCulture)
                : codigoInstDeduc.Trim();

            var fechaProcesoTexto = Helpers.CcProcesoMensualArchivoRutaHelperDb.FormatearFechaProceso(fechaProceso);
            var fechaServidorTexto = fechaServidor.ToString("ddMMyyyy", CultureInfo.InvariantCulture);

            return $"E-{codigoInstitucion}_{fechaProcesoTexto} [{fechaServidorTexto}-F32].xlsx";
        }
        public sealed class CcProcesoMensualArchivoF32RegistroDbModel
        {
            public string CedulaColilla { get; set; } = string.Empty;
            public string CodDeduccion { get; set; } = string.Empty;
            public decimal MontoActual { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
        }
    }
}
