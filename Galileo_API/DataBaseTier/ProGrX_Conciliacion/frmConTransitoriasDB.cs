using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Conciliacion;

namespace Galileo_API.DataBaseTier.ProGrX_Conciliacion
{
    public sealed class FrmConTransitoriasDb
    {
        private static readonly HashSet<string> OrigenesPermitidos =
            new(StringComparer.Ordinal)
            {
                "CRD",
                "DSM",
                "FND",
                "LIQ"
            };

        private readonly PortalDB _portalDb;

        public FrmConTransitoriasDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la fecha del servidor y los origenes funcionales de cuentas
        /// transitorias definidos por el formulario legado.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <returns>Datos requeridos para inicializar el formulario.</returns>
        public ErrorDto<ConTransitoriasInicializaData>
            Conciliacion_ConTransitorias_Inicializar(int codEmpresa)
        {
            ErrorDto<DateTime?> fechaResultado =
                DbHelper.ExecuteSingleQuery<DateTime?>(
                    _portalDb,
                    codEmpresa,
                    "select getdate();",
                    null);

            if (fechaResultado.Code < 0)
            {
                return DbHelper.CreateErrorResponse(
                    fechaResultado.Description ??
                    "No fue posible obtener la fecha del servidor.",
                    fechaResultado.Code ?? -1,
                    new ConTransitoriasInicializaData());
            }

            return DbHelper.CreateOkResponse(
                new ConTransitoriasInicializaData
                {
                    fecha_servidor = fechaResultado.Result,
                    origenes =
                    [
                        new()
                        {
                            codigo = "CRD",
                            descripcion = "Desembolsos de Créditos"
                        },
                        new()
                        {
                            codigo = "DSM",
                            descripcion = "Desembolsos Créditos a Terceros"
                        },
                        new()
                        {
                            codigo = "FND",
                            descripcion = "Liq/Retiros de Ahorros"
                        },
                        new()
                        {
                            codigo = "LIQ",
                            descripcion = "Liquidación de Asociados"
                        }
                    ]
                });
        }

        /// <summary>
        /// Consulta las cuentas transitorias para el origen y rango de fechas
        /// indicados.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa.</param>
        /// <param name="request">Origen y rango de fechas de la consulta.</param>
        /// <returns>Movimientos de cuentas transitorias.</returns>
        public ErrorDto<List<ConTransitoriasData>>
            Conciliacion_ConTransitorias_Consultar(
                int codEmpresa,
                ConTransitoriasConsultaRequest? request)
        {
            string origen = request?.origen?.Trim().ToUpperInvariant() ??
                string.Empty;

            if (!OrigenesPermitidos.Contains(origen))
            {
                return Conciliacion_ConTransitorias_ErrorLista_Crear(
                    "Debe seleccionar un origen válido.");
            }

            if (
                request?.fecha_inicio is null ||
                request.fecha_corte is null)
            {
                return Conciliacion_ConTransitorias_ErrorLista_Crear(
                    "Debe indicar la fecha inicial y la fecha corte.");
            }

            DateTime fechaInicio = request.fecha_inicio.Value.Date;
            DateTime fechaCorte = request.fecha_corte.Value.Date;

            if (fechaInicio > fechaCorte)
            {
                return Conciliacion_ConTransitorias_ErrorLista_Crear(
                    "La fecha inicial no puede ser mayor que la fecha corte.");
            }

            const string sql = """
                exec spSys_Cuentas_Transitorias
                    @Origen,
                    @FechaInicio,
                    @FechaCorte;
                """;

            return DbHelper.ExecuteListQuery<ConTransitoriasData>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    Origen = origen,
                    FechaInicio = fechaInicio,
                    FechaCorte = fechaCorte == DateTime.MaxValue.Date
                        ? DateTime.MaxValue
                        : fechaCorte.AddDays(1).AddSeconds(-1)
                });
        }

        /// <summary>
        /// Crea una respuesta de validacion con una lista vacia.
        /// </summary>
        /// <param name="mensaje">Detalle de la validacion.</param>
        /// <returns>Respuesta de validacion para la consulta.</returns>
        private static ErrorDto<List<ConTransitoriasData>>
            Conciliacion_ConTransitorias_ErrorLista_Crear(string mensaje)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                -2,
                new List<ConTransitoriasData>());
        }
    }
}
