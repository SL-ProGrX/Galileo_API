using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Convenios;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    public class FrmCrConveniosAhorrosDb
    {
        private const string EstadoCerrada = "C";

        private const string MensajeOrdenCerrada =
            "No se puede modificar una orden con estado cerrada.";

        private const string MensajeOrdenNoExiste =
            "No se encontró la orden del convenio indicada.";

        private const string SqlEncabezado = """
            SELECT
                RTRIM(O.COD_CONVENIO) AS cod_convenio,
                RTRIM(ISNULL(C.DESCRIPCION, '')) AS descripcion,
                O.COD_ORDEN AS cod_orden,
                RTRIM(ISNULL(O.ESTADO, '')) AS estado,
                CASE RTRIM(ISNULL(O.ESTADO, ''))
                    WHEN 'A' THEN 'Abierta'
                    WHEN 'C' THEN 'Cerrada'
                    WHEN 'T' THEN 'Tramitada'
                    ELSE ''
                END AS estado_desc
            FROM CRD_CONVENIOS_ORDENES O
            INNER JOIN CRD_CONVENIOS C
                ON O.COD_CONVENIO = C.COD_CONVENIO
            WHERE O.COD_CONVENIO = @cod_convenio
              AND O.COD_ORDEN = @cod_orden;
            """;

        private readonly PortalDB _portalDb;
        public FrmCrConveniosAhorrosDb(IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el encabezado, los planes de ahorro y los totales de una orden.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="codConvenio">Código del convenio.</param>
        /// <param name="codOrden">Número de orden.</param>
        /// <returns>Información utilizada por el diálogo.</returns>
        public ErrorDto<CrConveniosAhorrosData?>
            CR_Convenios_Ahorros_Obtener(
                int codEmpresa,
                string? codConvenio,
                int codOrden)
        {
            var convenio = (codConvenio ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(convenio))
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosAhorrosData?>(
                        "El convenio es requerido.",
                        -2);
            }

            if (codOrden <= 0)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosAhorrosData?>(
                        "El número de orden es requerido.",
                        -2);
            }

            var response = DbHelper.WithConn
                <CrConveniosAhorrosData?>(
                    _portalDb,
                    codEmpresa,
                    connection =>
                        CR_Convenios_Ahorros_Consultar(
                            connection,
                            convenio,
                            codOrden));

            if (response.Code == 0 && response.Result is null)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosAhorrosData?>(
                        MensajeOrdenNoExiste,
                        -2);
            }

            return response;
        }

        /// <summary>
        /// Actualiza el monto de un plan y retorna nuevamente
        /// el contenido del diálogo.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="request">
        /// Identificadores del plan y monto por actualizar.
        /// </param>
        /// <returns>Información recalculada del diálogo.</returns>
        public ErrorDto<CrConveniosAhorrosData?>
            CR_Convenios_Ahorros_Monto_Actualizar(
                int codEmpresa,
                CrConveniosAhorrosActualizarRequest? request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosAhorrosData?>(
                        "La solicitud es requerida.",
                        -2);
            }

            var mensajeValidacion =
                CR_Convenios_Ahorros_Solicitud_Validar(request);

            if (!string.IsNullOrEmpty(mensajeValidacion))
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosAhorrosData?>(
                        mensajeValidacion,
                        -2);
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(_portalDb, codEmpresa);

                var convenio = request.cod_convenio.Trim();
                var plan = request.cod_plan.Trim();

                var encabezado =
                    CR_Convenios_Ahorros_Encabezado_Consultar(
                        connection,
                        convenio,
                        request.cod_orden);

                if (encabezado is null)
                {
                    return DbHelper.CreateErrorResponse
                        <CrConveniosAhorrosData?>(
                            MensajeOrdenNoExiste,
                            -2);
                }

                if (encabezado.estado == EstadoCerrada)
                {
                    return DbHelper.CreateErrorResponse
                        <CrConveniosAhorrosData?>(
                            MensajeOrdenCerrada,
                            -2);
                }

                const string sql = """
                    EXEC spConvenios_Orden_FondosPool
                        @cod_convenio,
                        @cod_orden,
                        @cod_plan,
                        @cod_contrato,
                        @monto;
                    """;

                connection.Execute(
                    sql,
                    new
                    {
                        cod_convenio = convenio,
                        cod_orden = request.cod_orden,
                        cod_plan = plan,
                        cod_contrato = request.cod_contrato,
                        monto = request.monto
                    });

                var resultado =
                    CR_Convenios_Ahorros_Consultar(
                        connection,
                        convenio,
                        request.cod_orden);

                return DbHelper.CreateOkResponse
                    <CrConveniosAhorrosData?>(
                        resultado,
                        "Monto actualizado satisfactoriamente.");
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse
                    <CrConveniosAhorrosData?>(ex.Message);
            }
        }

        /// <summary>
        /// Consulta y arma la información completa del diálogo.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="codConvenio">Código del convenio.</param>
        /// <param name="codOrden">Número de orden.</param>
        /// <returns>
        /// Información completa o null cuando la orden no existe.
        /// </returns>
        private static CrConveniosAhorrosData?
            CR_Convenios_Ahorros_Consultar(
                SqlConnection connection,
                string codConvenio,
                int codOrden)
        {
            var encabezado =
                CR_Convenios_Ahorros_Encabezado_Consultar(
                    connection,
                    codConvenio,
                    codOrden);

            if (encabezado is null)
            {
                return null;
            }

            var detalles =
                CR_Convenios_Ahorros_Detalles_Consultar(
                    connection,
                    codConvenio,
                    codOrden);

            return CR_Convenios_Ahorros_Resultado_Crear(
                encabezado,
                detalles);
        }

        /// <summary>
        /// Consulta el convenio y el estado de la orden.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="codConvenio">Código del convenio.</param>
        /// <param name="codOrden">Número de orden.</param>
        /// <returns>Encabezado de la orden.</returns>
        private static CrConveniosAhorrosData?
            CR_Convenios_Ahorros_Encabezado_Consultar(
                SqlConnection connection,
                string codConvenio,
                int codOrden)
        {
            return connection.QueryFirstOrDefault
                <CrConveniosAhorrosData>(
                    SqlEncabezado,
                    new
                    {
                        cod_convenio = codConvenio,
                        cod_orden = codOrden
                    });
        }

        /// <summary>
        /// Consulta los planes de ahorro asociados a la orden.
        /// </summary>
        /// <param name="connection">Conexión de la empresa.</param>
        /// <param name="codConvenio">Código del convenio.</param>
        /// <param name="codOrden">Número de orden.</param>
        /// <returns>
        /// Filas devueltas por spConvenios_Orden_Ahorros.
        /// </returns>
        private static List<CrConveniosAhorrosDetalleDbData>
            CR_Convenios_Ahorros_Detalles_Consultar(
                SqlConnection connection,
                string codConvenio,
                int codOrden)
        {
            const string sql = """
                EXEC spConvenios_Orden_Ahorros
                    @cod_convenio,
                    @cod_orden;
                """;

            return connection.Query
                <CrConveniosAhorrosDetalleDbData>(
                    sql,
                    new
                    {
                        cod_convenio = codConvenio,
                        cod_orden = codOrden
                    })
                .ToList();
        }

        /// <summary>
        /// Convierte las filas del procedimiento en el resultado
        /// consumido por Angular.
        /// </summary>
        /// <param name="encabezado">Encabezado de la orden.</param>
        /// <param name="detalles">Filas de planes de ahorro.</param>
        /// <returns>Resultado con pendientes y totales calculados.</returns>
        private static CrConveniosAhorrosData
            CR_Convenios_Ahorros_Resultado_Crear(
                CrConveniosAhorrosData encabezado,
                IEnumerable<CrConveniosAhorrosDetalleDbData> detalles)
        {
            foreach (var detalleDb in detalles)
            {
                var mensualidad = detalleDb.mensualidad ?? 0;
                var aporte = detalleDb.aporte ?? 0;
                var monto = detalleDb.monto ?? 0;

                encabezado.ahorros.Add(
                    new CrConveniosAhorrosDetalleData
                    {
                        cod_contrato = detalleDb.cod_contrato,
                        cod_plan =
                            (detalleDb.cod_plan ?? string.Empty).Trim(),
                        descripcion =
                            (detalleDb.descripcion ?? string.Empty).Trim(),
                        mensualidad = mensualidad,
                        pendiente = Math.Max(
                            0,
                            mensualidad - aporte),
                        monto = monto
                    });

                encabezado.mensualidad_total += mensualidad;
                encabezado.monto_total += monto;
            }

            return encabezado;
        }

        /// <summary>
        /// Valida los identificadores requeridos para actualizar un monto.
        /// </summary>
        /// <param name="request">Solicitud recibida por el API.</param>
        /// <returns>
        /// Mensaje de validación o texto vacío cuando es válida.
        /// </returns>
        private static string
            CR_Convenios_Ahorros_Solicitud_Validar(
                CrConveniosAhorrosActualizarRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.cod_convenio))
            {
                return "El convenio es requerido.";
            }

            if (request.cod_orden <= 0)
            {
                return "El número de orden es requerido.";
            }

            if (string.IsNullOrWhiteSpace(request.cod_plan))
            {
                return "El plan de ahorro es requerido.";
            }

            if (request.cod_contrato <= 0)
            {
                return "El contrato es requerido.";
            }

            return string.Empty;
        }

        /// <summary>
        /// Fila interna utilizada para recibir el aporte requerido
        /// por el cálculo de pendiente.
        /// </summary>
        private sealed class CrConveniosAhorrosDetalleDbData
        {
            public int cod_contrato { get; set; } = 0;
            public string? cod_plan { get; set; }
            public string? descripcion { get; set; }
            public decimal? mensualidad { get; set; }
            public decimal? aporte { get; set; }
            public decimal? monto { get; set; }
        }
    }
}