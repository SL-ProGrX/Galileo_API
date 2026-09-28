using System.Globalization;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public sealed class FrmInvTransacReporteOrdenDB
    {
        private const int CodigoValidacion = -2;
        private const int EmpresaMaxima = 999999;
        private const int LongitudBoleta = 10;
        private const string NombreReporte = "Inventario_TransacBoleta";
        private const string ReporteTrasladoVenta = "TrasladoV";
        private const string OrdenLinea = "L";

        private static readonly Dictionary<string, string> SubtitulosTipo =
            new(StringComparer.Ordinal)
            {
                ["S"] = "SALIDAS",
                ["E"] = "ENTRADAS",
                ["T"] = "TRASLADOS",
                ["R"] = "REQUISICION",
            };

        private static readonly HashSet<string> OrdenesValidos =
            new(StringComparer.Ordinal) { "D", "C", OrdenLinea };

        private static readonly HashSet<string> ReportesValidos =
            new(StringComparer.Ordinal) { string.Empty, "TrasladoC", ReporteTrasladoVenta };

        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos de la ventana Orden del Reporte de transacciones.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTransacReporteOrdenDB(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Valida la boleta y resuelve el reporte, título y orden del detalle
        /// de la boleta de transacción. Equivale a sbReporte del VB6.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Boleta, tipo, reporte (TrasladoC/TrasladoV) y orden (D, C, L).</param>
        /// <returns>Configuración del reporte a generar.</returns>
        public ErrorDto<InvTransacReporteOrdenReporteDto?>
            INV_TransacReporteOrden_Reporte_Obtener(
                int CodEmpresa,
                InvTransacReporteOrdenRequest request)
        {
            var mensaje = INV_TransacReporteOrden_Request_Validar(CodEmpresa, request);

            if (mensaje is not null)
            {
                return INV_TransacReporteOrden_Validacion_Crear(mensaje);
            }

            var tipo = request.tipo.Trim().ToUpperInvariant();
            var existe = INV_TransacReporteOrden_Boleta_Existe(
                CodEmpresa, tipo, request.boleta.Trim());

            if (existe.Code != 0)
            {
                return DbHelper.CreateErrorResponse<InvTransacReporteOrdenReporteDto?>(
                    existe.Description ?? "Error consultando la boleta.");
            }

            if (string.IsNullOrEmpty(existe.Result))
            {
                return INV_TransacReporteOrden_Validacion_Crear(
                    "La boleta indicada no existe.");
            }

            return DbHelper.CreateOkResponse<InvTransacReporteOrdenReporteDto?>(
                new InvTransacReporteOrdenReporteDto
                {
                    nombre_reporte = NombreReporte,
                    titulo = INV_TransacReporteOrden_Titulo_Crear(tipo, request.reporte),
                    boleta = existe.Result,
                    tipo = tipo,
                    orden = INV_TransacReporteOrden_Orden_Normalizar(request.orden),
                });
        }

        /// <summary>
        /// Consulta la boleta en PV_INVTRANSAC o PV_REQUISICIONES según el tipo.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="tipo">Tipo normalizado (E, S, T, R).</param>
        /// <param name="boleta">Boleta numérica.</param>
        /// <returns>Boleta con diez posiciones o vacío si no existe.</returns>
        private ErrorDto<string?> INV_TransacReporteOrden_Boleta_Existe(
            int CodEmpresa,
            string tipo,
            string boleta)
        {
            const string queryTransac = """
                SELECT RIGHT(REPLICATE('0', 10) + CAST(BOLETA AS VARCHAR), 10)
                FROM PV_INVTRANSAC
                WHERE BOLETA = @boleta AND TIPO = @tipo
                """;

            const string queryRequisicion = """
                SELECT RIGHT(REPLICATE('0', 10) + CAST(COD_REQUISICION AS VARCHAR), 10)
                FROM PV_REQUISICIONES
                WHERE COD_REQUISICION = @requisicion
                """;

            return tipo == "R"
                ? DbHelper.ExecuteSingleQuery<string>(
                    _portalDb, CodEmpresa, queryRequisicion, string.Empty,
                    new { requisicion = long.Parse(boleta, CultureInfo.InvariantCulture) })
                : DbHelper.ExecuteSingleQuery<string>(
                    _portalDb, CodEmpresa, queryTransac, string.Empty,
                    new { boleta = boleta.PadLeft(LongitudBoleta, '0'), tipo });
        }

        /// <summary>
        /// Valida empresa, tipo, boleta, reporte y orden recibidos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del reporte.</param>
        /// <returns>Mensaje de validación o null cuando los datos son válidos.</returns>
        private static string? INV_TransacReporteOrden_Request_Validar(
            int CodEmpresa,
            InvTransacReporteOrdenRequest? request)
        {
            if (CodEmpresa is <= 0 or > EmpresaMaxima)
            {
                return "El c&oacute;digo de la empresa es requerido.";
            }

            if (request is null)
            {
                return "Los datos del reporte son requeridos.";
            }

            if (!SubtitulosTipo.ContainsKey((request.tipo ?? string.Empty).Trim().ToUpperInvariant()))
            {
                return "El tipo de movimiento no es v&aacute;lido.";
            }

            if (!INV_TransacReporteOrden_Boleta_Valida(request.boleta))
            {
                return "La boleta no es v&aacute;lida.";
            }

            return ReportesValidos.Contains((request.reporte ?? string.Empty).Trim())
                ? null
                : "El reporte solicitado no es v&aacute;lido.";
        }

        /// <summary>
        /// Verifica que la boleta sea numérica y de máximo diez posiciones.
        /// </summary>
        /// <param name="boleta">Boleta recibida.</param>
        /// <returns>True cuando la boleta es válida.</returns>
        private static bool INV_TransacReporteOrden_Boleta_Valida(string? boleta)
        {
            var valor = (boleta ?? string.Empty).Trim();

            return valor.Length is > 0 and <= LongitudBoleta
                && valor.All(char.IsAsciiDigit);
        }

        /// <summary>
        /// Arma el título del reporte según el tipo y el reporte solicitado.
        /// </summary>
        /// <param name="tipo">Tipo normalizado.</param>
        /// <param name="reporte">Reporte solicitado (TrasladoC, TrasladoV o vacío).</param>
        /// <returns>Título del reporte.</returns>
        private static string INV_TransacReporteOrden_Titulo_Crear(
            string tipo,
            string? reporte)
        {
            var titulo = $"BOLETA DE {SubtitulosTipo[tipo]}";

            return string.Equals(reporte?.Trim(), ReporteTrasladoVenta, StringComparison.Ordinal)
                ? $"{titulo} PARA VENTA"
                : titulo;
        }

        /// <summary>
        /// Normaliza el orden del detalle; por defecto Línea de Registro.
        /// </summary>
        /// <param name="orden">Orden recibido (D, C, L).</param>
        /// <returns>Orden válido.</returns>
        private static string INV_TransacReporteOrden_Orden_Normalizar(string? orden)
        {
            var valor = (orden ?? string.Empty).Trim().ToUpperInvariant();

            return OrdenesValidos.Contains(valor) ? valor : OrdenLinea;
        }

        /// <summary>
        /// Crea una respuesta de validación.
        /// </summary>
        /// <param name="descripcion">Descripción del error.</param>
        /// <returns>Respuesta con código de validación.</returns>
        private static ErrorDto<InvTransacReporteOrdenReporteDto?>
            INV_TransacReporteOrden_Validacion_Crear(string descripcion)
        {
            return DbHelper.CreateErrorResponse<InvTransacReporteOrdenReporteDto?>(
                descripcion, CodigoValidacion);
        }
    }
}
