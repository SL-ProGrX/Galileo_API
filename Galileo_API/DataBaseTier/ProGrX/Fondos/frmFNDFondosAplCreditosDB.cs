using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Fondos;
using Galileo.Models.Security;

namespace Galileo_API.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndFondosAplCreditosDb
    {
        private readonly IConfiguration _config;
        private readonly MSecurityMainDb _securityMainDb;
        private readonly int vModulo = 18;

        private const string SpListaFondosCreditos = "spFnd_FondosVrsCreditos_Lista";
        private const string SpAplicacionGeneral = "spFnd_Aplicacion_Creditos_General";
        private const string SpAplicacionPlan = "spFnd_Aplicacion_Creditos";

        private const string SqlPlanesPorCodigo = @"
                    SELECT
                        cod_plan AS CodPlan,
                        descripcion AS Descripcion
                    FROM dbo.fnd_planes
                    WHERE Cod_operadora = @CodOperadora
                    ORDER BY cod_plan;";

        private const string SqlPlanesPorDescripcion = @"
                    SELECT
                        cod_plan AS CodPlan,
                        descripcion AS Descripcion
                    FROM dbo.fnd_planes
                    WHERE Cod_operadora = @CodOperadora
                    ORDER BY descripcion;";

        private const string SqlResumenContratos = @"
                    SELECT
                        R.COD_OPERADORA AS CodOperadora,
                        R.COD_PLAN AS CodPlan,
                        R.COD_MONEDA AS CodMoneda,
                        R.PLAN_DESC AS PlanDesc,
                        R.TOTAL AS Total,
                        R.CONTRATOS AS Contratos
                    FROM dbo.vFnd_Contratos_Resumen R
                    INNER JOIN dbo.FND_PLANES P
                        ON R.cod_Operadora = P.COD_OPERADORA
                       AND R.COD_PLAN = P.COD_PLAN
                    WHERE P.indAplicarAmora = 1;";

        public FrmFndFondosAplCreditosDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _securityMainDb = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Obtiene la lista de planes de fondos para aplicación de créditos, ordenados por el campo especificado.
        /// </summary>
        /// <param name="codOperadora">Código de la operadora.</param>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="orderBy">Campo por el cual se ordena ("CodPlan" o "Descripcion").</param>
        /// <returns>ErrorDto con la lista de planes.</returns>
        public ErrorDto<List<FndFondosAplCreditosPlanModel>> FondosAplCreditos_Planes_Obtener(int codOperadora, int codEmpresa, string orderBy)
        {
            var query = ObtenerSqlPlanes(orderBy);

            return DbHelper.ExecuteListQuery<FndFondosAplCreditosPlanModel>(
                new PortalDB(_config),
                codEmpresa,
                query,
                new { CodOperadora = codOperadora });
        }

        /// <summary>
        /// Ejecuta el procedimiento almacenado para obtener la lista de fondos versus créditos según los parámetros enviados.
        /// </summary>
        /// <param name="param">Parámetros de la consulta.</param>
        /// <returns>ErrorDto con la lista de resultados.</returns>
        public ErrorDto<List<FndFondosAplCreditosListaResult>> FondosAplCreditos_Lista(FndFondosAplCreditosListaParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de consulta son requeridos.",
                    -2,
                    new List<FndFondosAplCreditosListaResult>());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), param.CodEmpresa, connection =>
                connection.Query<FndFondosAplCreditosListaResult>(
                    SpListaFondosCreditos,
                    CrearParametrosLista(param),
                    commandType: System.Data.CommandType.StoredProcedure).ToList());

            return new ErrorDto<List<FndFondosAplCreditosListaResult>>
            {
                Code = result.Code ?? 0,
                Description = result.Description,
                Result = result.Result ?? []
            };
        }

        /// <summary>
        /// Ejecuta la aplicación automática de fondos a créditos (versión general) y registra el movimiento en bitácora.
        /// </summary>
        /// <param name="param">Parámetros de la aplicación general.</param>
        /// <returns>ErrorDto con el resultado de la aplicación.</returns>
        public ErrorDto<FndFondosAplCreditosAplicacionGeneralResult> FondosAplCreditos_AplicacionGeneral(FndFondosAplCreditosAplicacionGeneralParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de aplicación general son requeridos.",
                    -2,
                    new FndFondosAplCreditosAplicacionGeneralResult());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), param.CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndFondosAplCreditosAplicacionGeneralResult>(
                    SpAplicacionGeneral,
                    CrearParametrosAplicacionGeneral(param),
                    commandType: System.Data.CommandType.StoredProcedure));

            if ((result.Code ?? 0) != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al aplicar fondos a créditos.",
                    result.Code ?? 0,
                    new FndFondosAplCreditosAplicacionGeneralResult());
            }

            var data = result.Result ?? new FndFondosAplCreditosAplicacionGeneralResult();
            RegistrarBitacora(
                param.CodEmpresa,
                param.Usuario,
                $"Fondos: General a Créditos - Masivo.. {data.TipoDoc}_{data.NumDoc}");

            return DbHelper.CreateOkResponse(data);
        }

        /// <summary>
        /// Ejecuta la aplicación automática de fondos a créditos para un plan específico y registra el movimiento en bitácora.
        /// </summary>
        /// <param name="param">Parámetros de la aplicación.</param>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>ErrorDto con el resultado de la aplicación.</returns>
        public ErrorDto<FndFondosAplCreditosAplicacionResult> FondosAplCreditos_Aplicacion(FndFondosAplCreditosAplicacionParams param, int codEmpresa)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de aplicación son requeridos.",
                    -2,
                    new FndFondosAplCreditosAplicacionResult());
            }

            var result = DbHelper.WithConn(new PortalDB(_config), codEmpresa, connection =>
                connection.QueryFirstOrDefault<FndFondosAplCreditosAplicacionResult>(
                    SpAplicacionPlan,
                    CrearParametrosAplicacion(param),
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al aplicar fondos a créditos.",
                    result.Code ?? 0,
                    new FndFondosAplCreditosAplicacionResult());
            }

            var data = result.Result ?? new FndFondosAplCreditosAplicacionResult();
            RegistrarBitacora(
                codEmpresa,
                param.Usuario,
                $"Fondos: {NormalizarTexto(param.CodPlan)} a Créditos - Masivo.. {data.TipoDoc}_{data.NumDoc}");

            return DbHelper.CreateOkResponse(data);
        }

        /// <summary>
        /// Obtiene el resumen de contratos de fondos para aplicación a créditos.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>ErrorDto con la lista de resumen de contratos.</returns>
        public ErrorDto<List<FndFondosAplCreditosResumenResult>> FondosAplCreditos_Resumen_Obtener(int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<FndFondosAplCreditosResumenResult>(
                new PortalDB(_config),
                codEmpresa,
                SqlResumenContratos);
        }
        
        private static string ObtenerSqlPlanes(string? orderBy)
        {
            return string.Equals(NormalizarTexto(orderBy), "descripcion", StringComparison.OrdinalIgnoreCase)
                ? SqlPlanesPorDescripcion
                : SqlPlanesPorCodigo;
        }

        private static object CrearParametrosLista(FndFondosAplCreditosListaParams param)
        {
            return new
            {
                Operadora = param.CodOperadora,
                Plan = NormalizarTexto(param.CodPlan),
                Tipo = NormalizarTexto(param.Tipo)
            };
        }

        private static object CrearParametrosAplicacionGeneral(FndFondosAplCreditosAplicacionGeneralParams param)
        {
            return new
            {
                Usuario = NormalizarTexto(param.Usuario),
                Aplica_Mora = param.AplicaMora,
                Aplica_CtaTransito = param.AplicaCtaTransito,
                Aplica_Extra = param.AplicaExtra,
                param.Institucion
            };
        }

        private static object CrearParametrosAplicacion(FndFondosAplCreditosAplicacionParams param)
        {
            return new
            {
                Operadora = param.CodOperadora,
                Plan = NormalizarTexto(param.CodPlan),
                Usuario = NormalizarTexto(param.Usuario),
                Aplica_Mora = param.AplicaMora,
                Aplica_CtaTransito = param.AplicaCtaTransito,
                Aplica_Extra = param.AplicaExtra
            };
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalleMovimiento)
        {
            _securityMainDb.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = NormalizarTexto(usuario),
                DetalleMovimiento = detalleMovimiento,
                Movimiento = "Aplica",
                Modulo = vModulo
            });
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();
    }
}
