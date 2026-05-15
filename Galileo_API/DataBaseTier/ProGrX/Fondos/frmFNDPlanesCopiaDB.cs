using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndPlanesCopiaDb
    {
        private readonly IConfiguration _config;
        private const string SpPlanesCopia = "spFndPlanesCopia";

        private const string SqlPlanesActivos = @"
                    SELECT
                        cod_plan AS item,
                        descripcion
                    FROM dbo.FND_Planes
                    WHERE ESTADO = 'A'
                    ORDER BY descripcion;";

        private const string SqlPlanSiguiente = @"
                    SELECT TOP 1
                        LTRIM(RTRIM(cod_plan)) AS item,
                        LTRIM(RTRIM(descripcion)) AS descripcion
                    FROM dbo.vFnd_Planes
                    WHERE estado = 'A'
                      AND cod_plan > @Plan
                    ORDER BY cod_plan ASC;";

        private const string SqlPlanAnterior = @"
                    SELECT TOP 1
                        LTRIM(RTRIM(cod_plan)) AS item,
                        LTRIM(RTRIM(descripcion)) AS descripcion
                    FROM dbo.vFnd_Planes
                    WHERE estado = 'A'
                      AND cod_plan < @Plan
                    ORDER BY cod_plan DESC;";

        private const string SqlPrimerPlan = @"
                    SELECT TOP 1
                        LTRIM(RTRIM(cod_plan)) AS item,
                        LTRIM(RTRIM(descripcion)) AS descripcion
                    FROM dbo.vFnd_Planes
                    WHERE estado = 'A'
                    ORDER BY cod_plan ASC;";

        public FrmFndPlanesCopiaDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los planes 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> FND_Planes_Obtener(int CodEmpresa)
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanesActivos);
        }


        /// <summary>
        /// Obtiene Scroll de planes 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="plan"></param>
        /// <param name="scrollCode"></param>
        /// <returns></returns>
        public ErrorDto<DropDownListaGenericaModel> AF_Plan_Scroll_Obtener(int CodEmpresa,
     string plan, int scrollCode)
        {
            var planActual = NormalizarTexto(plan);
            var sql = scrollCode == 1 ? SqlPlanSiguiente : SqlPlanAnterior;

            var result = DbHelper.ExecuteSingleQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                sql,
                default,
                new { Plan = planActual });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al consultar el plan.",
                    result.Code.GetValueOrDefault(-1),
                    new DropDownListaGenericaModel());
            }

            if (result.Result is not null)
            {
                return DbHelper.CreateOkResponse(result.Result, "Consulta realizada correctamente");
            }

            return ObtenerPrimerPlan(CodEmpresa);
        }


        /// <summary>
        /// Copia el plan 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto FND_Planes_Copiar(int CodEmpresa, string usuario, FndPlanesCopiaRequestDto dto)
        {
            if (dto is null)
            {
                return DbHelper.ErrorResponse("Los datos para copiar el plan son requeridos.", -2);
            }

            var result = DbHelper.WithConn(new PortalDB(_config), CodEmpresa, connection =>
                connection.Execute(
                    SpPlanesCopia,
                    CrearParametrosCopia(usuario, dto),
                    commandType: CommandType.StoredProcedure));

            return result.Code == 0
                ? DbHelper.OkResponse("OK")
                : DbHelper.ErrorResponse(result.Description ?? "Error al copiar el plan.", result.Code.GetValueOrDefault(-1));
        }


        private ErrorDto<DropDownListaGenericaModel> ObtenerPrimerPlan(int codEmpresa)
        {
            var result = DbHelper.ExecuteSingleQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                codEmpresa,
                SqlPrimerPlan,
                default);

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al consultar el primer plan.",
                    result.Code.GetValueOrDefault(-1),
                    new DropDownListaGenericaModel());
            }

            return result.Result is null
                ? DbHelper.CreateErrorResponse("No hay planes configurados.", -1, new DropDownListaGenericaModel())
                : DbHelper.CreateOkResponse(result.Result, "Consulta realizada correctamente");
        }

        private static DynamicParameters CrearParametrosCopia(string usuario, FndPlanesCopiaRequestDto dto)
        {
            var parametros = new DynamicParameters();
            parametros.Add("@Operadora", 0, DbType.Int32);
            parametros.Add("@PlanOrigen", NormalizarTexto(dto.planbase), DbType.String);
            parametros.Add("@PlanDestino", NormalizarTexto(dto.plandestino), DbType.String);
            parametros.Add("@Usuario", NormalizarTexto(usuario), DbType.String);
            parametros.Add("@TablaMultas", dto.copiarMultas, DbType.Boolean);
            parametros.Add("@TablaPuntos", dto.copiarPuntos, DbType.Boolean);
            parametros.Add("@General", dto.copiarGeneral, DbType.Boolean);
            parametros.Add("@Cuentas", dto.copiarCuentas, DbType.Boolean);
            parametros.Add("@Destinos", dto.copiarDestinos, DbType.Boolean);
            parametros.Add("@EstPersona", dto.copiarEstadosPersona, DbType.Boolean);
            parametros.Add("@Plazos", dto.copiarPlazos, DbType.Boolean);
            parametros.Add("@NuevoDescipcion", NormalizarTexto(dto.descripciondestino), DbType.String);
            return parametros;
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}