using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.DataBaseTier.ProGrX.Clientes
{
    public class FrmFndMonitorVencimientoBd
    {
        private readonly IConfiguration _config;

        private const string SqlTiposPlan = @"
                    SELECT COD_TIPO_PLAN AS item,
                           DESCRIPCION AS descripcion
                    FROM dbo.FND_PLANES_TIPO_PLAN;";

        private const string SqlPlanesPorCodigo = @"
                    SELECT cod_plan AS Cod_Plan,
                           descripcion AS Descripcion
                    FROM dbo.fnd_planes
                    WHERE Sinpe_Cuenta = 0
                    ORDER BY cod_plan;";

        private const string SqlPlanesPorDescripcion = @"
                    SELECT cod_plan AS Cod_Plan,
                           descripcion AS Descripcion
                    FROM dbo.fnd_planes
                    WHERE Sinpe_Cuenta = 0
                    ORDER BY descripcion;";

        private const string SpVencimientosConsulta = "spFnd_Vencimientos_Consulta";

        public FrmFndMonitorVencimientoBd(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los tipos de plan disponibles para el monitor de vencimientos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Listado de tipos de plan.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Planes_TipoPlan_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                SqlTiposPlan);
        }


        /// <summary>
        /// Obtiene los planes disponibles para el monitor de vencimientos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Filtros y ordenamiento de planes.</param>
        /// <returns>Listado de planes.</returns>
        public ErrorDto<List<FndPlanesItem>> Fnd_Planes_Obtener(int CodEmpresa, FndPlanesObtenerRequest request)
        {
            var sql = DebeOrdenarPorDescripcion(request)
                ? SqlPlanesPorDescripcion
                : SqlPlanesPorCodigo;

            return DbHelper.ExecuteListQuery<FndPlanesItem>(
                CreatePortalDb(),
                CodEmpresa,
                sql);
        }


        /// <summary>
        /// Consulta los vencimientos de fondos según los filtros indicados.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Filtros de consulta de vencimientos.</param>
        /// <returns>Listado de vencimientos.</returns>
        public ErrorDto<List<FndVencimientosConsultaResult>> Fnd_Vencimientos_Consulta(int CodEmpresa, FndVencimientosConsultaRequest request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de vencimientos son requeridos.",
                    -2,
                    new List<FndVencimientosConsultaResult>());
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<FndVencimientosConsultaResult>(
                    SpVencimientosConsulta,
                    CrearParametrosVencimientos(request),
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<FndVencimientosConsultaResult>())
                : DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al consultar vencimientos.",
                    result.Code.GetValueOrDefault(-1),
                    new List<FndVencimientosConsultaResult>());
        }

        /// <summary>
        /// Indica si la consulta de planes debe ordenarse por descripción.
        /// </summary>
        private static bool DebeOrdenarPorDescripcion(FndPlanesObtenerRequest? request)
        {
            return string.Equals(
                NormalizarTexto(request?.OrdenarPor),
                "descripcion",
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Crea parámetros seguros para consultar vencimientos.
        /// </summary>
        private static object CrearParametrosVencimientos(FndVencimientosConsultaRequest request)
        {
            return new
            {
                Plan = NormalizarTexto(request.Plan),
                Inicio = request.FechaIni,
                Corte = request.FechaFin,
                request.TipoFondo,
                request.TipoCDP
            };
        }

        /// <summary>
        /// Crea una instancia de acceso al portal usando la configuración inyectada.
        /// </summary>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Normaliza valores de texto recibidos desde filtros o formularios.
        /// </summary>
        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}