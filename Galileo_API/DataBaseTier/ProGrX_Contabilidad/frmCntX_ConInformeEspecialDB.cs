using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Data;
using static Galileo_API.Models.ProGrX_Contabilidad.FrmCntxConInformeEspecialModels;
using ClosedXML.Excel;


namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntxConInformeEspecialDB
    {
        private readonly PortalDB _portalDb;

        public FrmCntxConInformeEspecialDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Genera un archivo Excel con el balance consolidado especial para la contabilidad, año y mes especificados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<ArchivoGeneradoModel> Cnt_ConsolidadoEspecial_Excel_Generar(int CodEmpresa, CntConsolidadoEspecialGenerarRequest request, string usuario)
        {
            using var connection =
                DbHelper.OpenConnection(_portalDb, CodEmpresa);

            try
            {
                var validacion = ValidarSolicitud(request);

                if (!string.IsNullOrEmpty(validacion))
                {
                    return DbHelper.CreateErrorResponse(
                        validacion,
                        -1,
                        new ArchivoGeneradoModel());
                }

                var context = CrearContexto(request, usuario);

                var registros = ObtenerRegistros(
                    connection,
                    context);

                var archivo = GenerarArchivoExcel(
                    registros,
                    request.Anio,
                    request.Mes);

                return DbHelper.CreateOkResponse(archivo);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse(
                    "Error al generar el balance consolidado.",
                    -1,
                    new ArchivoGeneradoModel());
            }
        }

        /// <summary>
        /// Valida los parámetros de la solicitud para generar el balance consolidado especial.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string ValidarSolicitud(CntConsolidadoEspecialGenerarRequest request)
        {
            if (request.Contabilidad <= 0)
            {
                return "Debe indicar la contabilidad.";
            }

            if (request.Anio <= 0)
            {
                return "Debe indicar el año.";
            }

            if (request.Mes is < 1 or > 12)
            {
                return "El mes debe estar entre 1 y 12.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Crea un contexto con la información de la solicitud y el usuario para generar el balance consolidado especial.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static CntConsolidadoEspecialContext CrearContexto(CntConsolidadoEspecialGenerarRequest request, string usuario)
        {
            return new CntConsolidadoEspecialContext
            {
                Contabilidad = request.Contabilidad,
                Anio = request.Anio,
                Mes = request.Mes,
                Usuario = usuario.Trim()
            };
        }

        /// <summary>
        /// Obtiene los registros del balance consolidado especial desde la base de datos utilizando un procedimiento almacenado.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private static IReadOnlyCollection<CntConsolidadoEspecialRegistro>
            ObtenerRegistros(
                IDbConnection connection,
                CntConsolidadoEspecialContext context)
        {
            const string procedure = "spCntX_Balance_Consolidado_Especial";

            var parametros = new
            {
                context.Contabilidad,
                context.Anio,
                context.Mes,
                context.Usuario,
                Tipo = "G"
            };

            var registros =
                connection.Query<CntConsolidadoEspecialRegistro>(
                    procedure,
                    parametros,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 1200);

            return registros.AsList();
        }

        /// <summary>
        /// Representa una columna en el archivo Excel, incluyendo su encabezado, la función para obtener el valor de un registro y el formato opcional.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Encabezado"></param>
        /// <param name="ObtenerValor"></param>
        /// <param name="Formato"></param>
        public sealed record ColumnaExcel<T>(
            string Encabezado,
            Func<T, object?> ObtenerValor,
            string? Formato = null
        );

        /// <summary>
        /// Define las columnas que se incluirán en el archivo Excel para el balance consolidado especial, incluyendo sus encabezados, funciones para obtener los valores de los registros y formatos opcionales.
        /// </summary>
        private static readonly IReadOnlyCollection<ColumnaExcel<CntConsolidadoEspecialRegistro>>
            ColumnasConsolidado =
        [
            new("Cuenta", x => x.COD_CUENTA_MASK),
            new("Descripción", x => x.Cuenta_Desc),

            new("U_Central Año 0", x => x.U_Central_Anio_0, "#,##0.00"),
            new("U_Hotelera Año 0", x => x.U_Hotel_Anio_0, "#,##0.00"),
            new("U_Jaúles Año 0", x => x.U_Jaules_Anio_0, "#,##0.00"),
            new("Consolidado Año 0", x => x.Consolidado_Anio_0, "#,##0.00"),

            new("U_Central Año 1", x => x.U_Central_Anio_1, "#,##0.00"),
            new("U_Hotelera Año 1", x => x.U_Hotel_Anio_1, "#,##0.00"),
            new("U_Jaúles Año 1", x => x.U_Jaules_Anio_1, "#,##0.00"),
            new("Consolidado Año 1", x => x.Consolidado_Anio_1, "#,##0.00"),
            new("Variación Año 1", x => x.Variacion_Anio_1, "#,##0.00"),
            new("Variación % Año 1", x => x.Variacion_Porc_Anio_1, "#,##0.00"),

            new("U_Central Año 2", x => x.U_Central_Anio_2  , "#,##0.00"),
            new("U_Hotelera Año 2", x => x.U_Hotel_Anio_2, "#,##0.00"),
            new("U_Jaúles Año 2", x => x.U_Jaules_Anio_2, "#,##0.00"),
            new("Consolidado Año 2", x => x.Consolidado_Anio_2, "#,##0.00"),
            new("Variación Año 2", x => x.Variacion_Anio_2, "#,##0.00"),
            new("Variación % Año 2", x => x.Variacion_Porc_Anio_2, "#,##0.00"),

            new("Tipo de Cuenta", x => x.Tipo_Cuenta_Desc),
            new("Divisa", x => x.COD_DIVISA),
            new("Año Referencia", x => x.Anio),
            new("Mes Referencia", x => x.Mes),
            new("Nivel", x => x.Nivel),
            new("Acepta Mov.", x => x.Acepta_Movimientos),
            new("Clasificación", x => x.Clasificacion)
        ];

        private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// Genera un archivo Excel con el balance consolidado especial utilizando la biblioteca ClosedXML, agregando encabezados, registros y aplicando formatos a la hoja.
        /// </summary>
        /// <param name="registros"></param>
        /// <param name="anio"></param>
        /// <param name="mes"></param>
        /// <returns></returns>
        private static ArchivoGeneradoModel GenerarArchivoExcel(IReadOnlyCollection<CntConsolidadoEspecialRegistro> registros, int anio, short mes)
        {
            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Consolidado");

            AgregarEncabezados(worksheet);
            AgregarRegistros(worksheet, registros);
            AplicarFormatoHoja(
          worksheet,
          registros.Count);

            var nombreArchivo = $"ProGrX_Balance_Consolidado_{anio}_{mes}.xlsx";

            var rutaDescargasServidor = Path.Combine(
         Environment.GetFolderPath(
             Environment.SpecialFolder.UserProfile),
         "Downloads");

            Directory.CreateDirectory(rutaDescargasServidor);

            var rutaCompleta = Path.Combine(
                rutaDescargasServidor,
                nombreArchivo);

            workbook.SaveAs(rutaCompleta);

            return new ArchivoGeneradoModel
            {
                NombreArchivo =
            rutaCompleta,
                ContentType = ExcelContentType
            };
        }

        /// <summary>
        /// Agrega los encabezados de las columnas al archivo Excel en la primera fila, utilizando la información definida en la colección ColumnasConsolidado.
        /// </summary>
        /// <param name="worksheet"></param>
        private static void AgregarEncabezados(IXLWorksheet worksheet)
        {
            var numeroColumna = 1;

            foreach (var columna in ColumnasConsolidado)
            {
                var celda = worksheet.Cell(1, numeroColumna);

                celda.Value = columna.Encabezado;

                numeroColumna++;
            }
        }
        
        /// <summary>
        /// Agrega los registros del balance consolidado especial al archivo Excel, comenzando desde la segunda fila, utilizando la información de cada registro y aplicando los formatos definidos en la colección ColumnasConsolidado.
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="registros"></param>
        private static void AgregarRegistros(IXLWorksheet worksheet, IReadOnlyCollection<CntConsolidadoEspecialRegistro> registros)
        {
            var numeroFila = 2;

            foreach (var registro in registros)
            {
                AgregarRegistro(
                    worksheet,
                    registro,
                    numeroFila);

                numeroFila++;
            }
        }

        /// <summary>
        /// Agrega un registro del balance consolidado especial al archivo Excel, utilizando la información de cada columna y aplicando los formatos definidos en la colección ColumnasConsolidado.
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="registro"></param>
        /// <param name="numeroFila"></param>
        private static void AgregarRegistro(IXLWorksheet worksheet, CntConsolidadoEspecialRegistro registro, int numeroFila)
        {
            var numeroColumna = 1;

            foreach (var columna in ColumnasConsolidado)
            {
                var celda =
                    worksheet.Cell(numeroFila, numeroColumna);

                AsignarValor(
                    celda,
                    columna.ObtenerValor(registro));

                AplicarFormatoNumero(
                    celda,
                    columna.Formato);

                numeroColumna++;
            }
        }
        
        /// <summary>
        /// Aplica un formato numérico a una celda del archivo Excel si se proporciona un formato válido, utilizando la propiedad NumberFormat de la celda.
        /// </summary>
        /// <param name="celda"></param>
        /// <param name="formato"></param>
        private static void AplicarFormatoNumero(IXLCell celda, string? formato)
        {
            if (!string.IsNullOrWhiteSpace(formato))
            {
                celda.Style.NumberFormat.Format = formato;
            }
        }
        
        /// <summary>
        /// Asigna un valor a una celda del archivo Excel, manejando diferentes tipos de datos y convirtiéndolos a XLCellValue según corresponda, incluyendo valores nulos, números, booleanos y fechas.
        /// </summary>
        /// <param name="celda"></param>
        /// <param name="valor"></param>
        private static void AsignarValor(IXLCell celda, object? valor)
        {
            celda.Value = valor switch
            {
                null => (XLCellValue)string.Empty,
                decimal numeroDecimal => (XLCellValue)numeroDecimal,
                double numeroDouble => (XLCellValue)numeroDouble,
                int numeroEntero => (XLCellValue)numeroEntero,
                short numeroCorto => (XLCellValue)numeroCorto,
                long numeroLargo => (XLCellValue)numeroLargo,
                bool valorBooleano => (XLCellValue)valorBooleano,
                DateTime fecha => (XLCellValue)fecha,
                _ => (XLCellValue)(valor.ToString() ?? string.Empty),
            };
        }
       
        /// <summary>
        /// Aplica formato a la hoja del archivo Excel, incluyendo congelar la primera fila, aplicar formato a los encabezados, aplicar colores a las columnas según el período y establecer anchos de columna específicos.
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="cantidadRegistros"></param>
        private static void AplicarFormatoHoja(IXLWorksheet worksheet, int cantidadRegistros)
        {
            var ultimaFila = Math.Max(cantidadRegistros + 1, 2);

            worksheet.SheetView.FreezeRows(1);

            AplicarFormatoEncabezados(worksheet);
            AplicarColoresPorPeriodo(worksheet, ultimaFila);
            AplicarAnchosColumnas(worksheet);
        }

        /// <summary>
        /// Aplica colores de fondo a las columnas del archivo Excel según el período (Año 0, Año 1, Año 2), utilizando la propiedad Fill.BackgroundColor de las celdas en los rangos correspondientes.
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="ultimaFila"></param>
        private static void AplicarColoresPorPeriodo(IXLWorksheet worksheet, int ultimaFila)
        {
            // Año 0: columnas C hasta F.
            worksheet.Range($"C1:F{ultimaFila}")
                .Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#C6F5F7");

            // Año 1: columnas G hasta L.
            worksheet.Range($"G1:L{ultimaFila}")
                .Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#C9F8C9");

            // Año 2: columnas M hasta R.
            worksheet.Range($"M1:R{ultimaFila}")
                .Style.Fill.BackgroundColor =
                    XLColor.FromHtml("#C6F5F7");
        }
       
        /// <summary>
        /// Aplica formato a los encabezados de las columnas del archivo Excel, incluyendo alineación vertical y horizontal, y un borde inferior para resaltar los encabezados.
        /// </summary>
        /// <param name="worksheet"></param>
        private static void AplicarFormatoEncabezados(IXLWorksheet worksheet)
        {
            var encabezados = worksheet.Range("A1:Y1");

            encabezados.Style.Alignment.Vertical =
                XLAlignmentVerticalValues.Center;

            encabezados.Style.Alignment.Horizontal =
                XLAlignmentHorizontalValues.Left;

            encabezados.Style.Border.BottomBorder =
                XLBorderStyleValues.Thin;
        }
        
        /// <summary>
        /// Aplica anchos específicos a las columnas del archivo Excel para mejorar la legibilidad, estableciendo anchos fijos para las columnas A y B, y un ancho uniforme para las columnas C hasta R.
        /// </summary>
        /// <param name="worksheet"></param>
        private static void AplicarAnchosColumnas(IXLWorksheet worksheet)
        {
            worksheet.Column("A").Width = 24;
            worksheet.Column("B").Width = 55;

            worksheet.Columns("C:R").Width = 18;

        }

    }
}
