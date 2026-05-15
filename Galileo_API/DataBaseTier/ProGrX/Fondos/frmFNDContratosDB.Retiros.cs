using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndContratosDB
    {

        private const string SqlRetirosTotal = @"
                    SELECT COUNT(consec)
                    FROM dbo.fnd_liquidacion
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato
                      AND (@hasFilter = 0 OR
                          (CONVERT(varchar(50), consec) LIKE @filtro OR
                           usuario LIKE @filtro OR
                           CONVERT(varchar(30), fecha, 120) LIKE @filtro));";

        private const string SqlRetirosBuscar = @"
                    SELECT
                        consec,
                        fecha,
                        aportes_liq,
                        rendi_liq,
                        estado,
                        usuario
                    FROM dbo.fnd_liquidacion
                    WHERE cod_operadora = @Operadora
                      AND cod_plan = @Plan
                      AND cod_contrato = @Contrato
                      AND (@hasFilter = 0 OR
                          (CONVERT(varchar(50), consec) LIKE @filtro OR
                           usuario LIKE @filtro OR
                           CONVERT(varchar(30), fecha, 120) LIKE @filtro))
                    ORDER BY
                        CASE WHEN @sortCode = 1 AND @isAsc = 1 THEN consec END ASC,
                        CASE WHEN @sortCode = 1 AND @isAsc = 0 THEN consec END DESC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 1 THEN fecha END ASC,
                        CASE WHEN @sortCode = 2 AND @isAsc = 0 THEN fecha END DESC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 1 THEN usuario END ASC,
                        CASE WHEN @sortCode = 3 AND @isAsc = 0 THEN usuario END DESC,
                        consec ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private static readonly IReadOnlyDictionary<string, int> RetirosSortMap = new Dictionary<string, int>
        {
            ["consec"] = 1,
            ["fecha"] = 2,
            ["usuario"] = 3
        };

        #region Retiros

        /// <summary>
        /// Obtiene los retiros o liquidaciones asociados a un contrato.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="plan">Código del plan.</param>
        /// <param name="contrato">Número de contrato.</param>
        /// <param name="filtros">Filtros de búsqueda, ordenamiento y paginación.</param>
        /// <returns>Listado paginado de retiros o liquidaciones.</returns>
        public ErrorDto<FndContratosLiquidacionesListaData> Fnd_Contratos_Retiros_Obtener(int CodEmpresa, int operadora, string plan, int contrato, Models.FiltrosLazyLoadData filtros)
        {
            var response = DbHelper.CreateOkResponse(new FndContratosLiquidacionesListaData
            {
                total = 0,
                lineas = new List<FndContratosLiquidacionesModels>()
            });

            try
            {
                var spec = LazyLoadHelper.Build(filtros, RetirosSortMap, "consec");
                var parametros = CrearParametrosRetirosBusqueda(operadora, plan, contrato, spec);

                var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection => new FndContratosLiquidacionesListaData
                {
                    total = connection.QueryFirstOrDefault<int>(SqlRetirosTotal, parametros),
                    lineas = connection.Query<FndContratosLiquidacionesModels>(SqlRetirosBuscar, parametros).ToList()
                });

                if (result.Code != 0)
                {
                    return DbHelper.CreateErrorResponse(
                        result.Description ?? "Error al obtener retiros del contrato.",
                        result.Code.GetValueOrDefault(-1),
                        new FndContratosLiquidacionesListaData
                        {
                            total = 0,
                            lineas = new List<FndContratosLiquidacionesModels>()
                        });
                }

                response.Result = result.Result ?? new FndContratosLiquidacionesListaData
                {
                    total = 0,
                    lineas = new List<FndContratosLiquidacionesModels>()
                };
            }
            catch (Exception ex)
            {
                response = DbHelper.CreateErrorResponse(
                    ex.Message,
                    -1,
                    new FndContratosLiquidacionesListaData
                    {
                        total = 0,
                        lineas = new List<FndContratosLiquidacionesModels>()
                    });
            }

            return response;
        }

        /// <summary>
        /// Crea los parámetros seguros para la búsqueda paginada de retiros.
        /// </summary>
        /// <param name="operadora">Código de operadora.</param>
        /// <param name="plan">Código del plan.</param>
        /// <param name="contrato">Número de contrato.</param>
        /// <param name="spec">Especificación de lazy load ya validada.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static object CrearParametrosRetirosBusqueda(int operadora, string plan, int contrato, LazyLoadSpec spec)
        {
            return new
            {
                Operadora = operadora,
                Plan = NormalizarTexto(plan),
                Contrato = contrato,
                hasFilter = spec.HasFilter ? 1 : 0,
                filtro = spec.HasFilter ? spec.Params.Get<string>("@filtro") : null,
                sortCode = spec.SortCode,
                isAsc = spec.IsAsc ? 1 : 0,
                offset = spec.Offset,
                fetch = spec.PageSize
            };
        }
        #endregion
    }
}