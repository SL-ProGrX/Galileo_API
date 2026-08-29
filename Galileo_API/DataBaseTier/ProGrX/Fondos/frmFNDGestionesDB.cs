using System.Data;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndGestionesDb
    {
        private readonly IConfiguration _config;

        private const string SqlBuscarContratos = @"
                    SELECT TOP (@Top)
                        O.Descripcion AS Operadora,
                        F.Cod_Operadora,
                        F.Cod_plan,
                        P.Descripcion,
                        F.Cod_Contrato,
                        F.Cedula,
                        S.Nombre
                    FROM dbo.Fnd_Contratos F
                    INNER JOIN dbo.Fnd_Operadoras O
                        ON F.Cod_operadora = O.Cod_operadora
                    INNER JOIN dbo.Fnd_planes P
                        ON F.Cod_operadora = P.Cod_operadora
                       AND F.Cod_plan = P.Cod_plan
                    INNER JOIN dbo.Socios S
                        ON F.Cedula = S.Cedula
                    WHERE F.Estado <> 'L'
                      AND dbo.fxFndColaboradorVisualiza(F.COD_OPERADORA, F.COD_PLAN, F.cedula, S.ESTADOACTUAL, @Usuario) = 1
                      AND (@Cod_Operadora IS NULL OR F.Cod_operadora = @Cod_Operadora)
                      AND (@Cod_Plan IS NULL OR F.Cod_Plan LIKE @Cod_Plan)
                      AND (@Cod_Contrato IS NULL OR F.Cod_Contrato = @Cod_Contrato)
                      AND (@Cedula IS NULL OR F.Cedula LIKE @Cedula)
                      AND (@Nombre IS NULL OR S.Nombre LIKE @Nombre);";

        private const string SqlContratosBusqueda = @"
                    SELECT
                        F.cod_Contrato AS Cod_Contrato,
                        F.cedula AS Cedula
                    FROM dbo.fnd_Contratos F
                    WHERE F.cod_operadora = @CodOperadora
                      AND F.cod_plan = @CodPlan
                      AND F.Estado <> 'L'
                    ORDER BY F.cod_Contrato;";

        private const string SqlContratoObtener = @"
                    SELECT
                        F.Cedula,
                        S.Nombre,
                        F.Estado,
                        F.Plazo,
                        F.Monto,
                        F.Aportes,
                        F.Rendimiento,
                        F.Inc_Tipo,
                        F.Inc_Anual,
                        F.Operacion
                    FROM dbo.Fnd_Contratos F
                    INNER JOIN dbo.Socios S
                        ON F.Cedula = S.Cedula
                    WHERE F.cod_contrato = @CodContrato
                      AND F.Cod_operadora = @CodOperadora
                      AND F.Cod_plan = @CodPlan
                      AND F.Estado <> 'L';";

        private const string SqlRenovacionGeneral = @"
                    SELECT
                        Cod_plan,
                        Cod_Contrato,
                        Inc_Tipo,
                        Inc_Anual,
                        Monto,
                        Fecha_Inicio,
                        Ult_Renovacion,
                        Plazo
                    FROM dbo.Fnd_Contratos
                    WHERE Cod_operadora = @CodOperadora
                      AND Estado <> 'L'
                      AND Renueva = 'S';";

        private const string SqlRenovacionPlan = @"
                    SELECT
                        Cod_plan,
                        Cod_Contrato,
                        Inc_Tipo,
                        Inc_Anual,
                        Monto,
                        Fecha_Inicio,
                        Ult_Renovacion,
                        Plazo
                    FROM dbo.Fnd_Contratos
                    WHERE Cod_operadora = @CodOperadora
                      AND Cod_plan = @CodPlan
                      AND Estado <> 'L'
                      AND Renueva = 'S';";

        private const string SqlRenovacionContrato = @"
                    SELECT
                        Cod_plan,
                        Cod_Contrato,
                        Inc_Tipo,
                        Inc_Anual,
                        Monto,
                        Fecha_Inicio,
                        Ult_Renovacion,
                        Plazo
                    FROM dbo.Fnd_Contratos
                    WHERE Cod_operadora = @CodOperadora
                      AND Cod_plan = @CodPlan
                      AND cod_contrato = @CodContrato;";

        private const string SqlActualizarContrato = @"
                    UPDATE dbo.Fnd_Contratos
                    SET Monto = @Monto,
                        Ult_renovacion = dbo.mygetdate(),
                        ind_deduccion = 0
                    WHERE Cod_operadora = @CodOperadora
                      AND cod_plan = @CodPlan
                      AND cod_contrato = @CodContrato;";

        public FrmFndGestionesDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Busca contratos según los filtros proporcionados.
        /// </summary>
        /// <param name="param">Parámetros de búsqueda.</param>
        /// <returns>ErrorDto con la lista de contratos encontrados.</returns>
        public ErrorDto<List<FndGestionesBuscarContratosResult>> Gestiones_BuscarContratos(FndGestionesBuscarContratosParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de búsqueda son requeridos.",
                    -2,
                    new List<FndGestionesBuscarContratosResult>());
            }

            return DbHelper.ExecuteListQuery<FndGestionesBuscarContratosResult>(
                new PortalDB(_config),
                param.CodEmpresa,
                SqlBuscarContratos,
                CrearParametrosBusqueda(param));
        }

        /// <summary>
        /// Obtiene la lista corta de contratos (código y cédula) para el diálogo de búsqueda F4.
        /// </summary>
        /// <param name="param">Parámetros de búsqueda (operadora y plan).</param>
        /// <returns>ErrorDto con la lista de contratos.</returns>
        public ErrorDto<List<FndGestionesContratosBusquedaResult>> Gestiones_Contratos_Busqueda_Obtener(
            FndGestionesContratosBusquedaParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de búsqueda son requeridos.",
                    -2,
                    new List<FndGestionesContratosBusquedaResult>());
            }

            if (string.IsNullOrWhiteSpace(param.CodPlan))
            {
                return DbHelper.CreateErrorResponse(
                    "El plan es requerido para buscar contratos.",
                    -2,
                    new List<FndGestionesContratosBusquedaResult>());
            }

            return DbHelper.ExecuteListQuery<FndGestionesContratosBusquedaResult>(
                new PortalDB(_config),
                param.CodEmpresa,
                SqlContratosBusqueda,
                new
                {
                    param.CodOperadora,
                    CodPlan = NormalizarTexto(param.CodPlan)
                });
        }

        /// <summary>
        /// Consulta un contrato específico según los parámetros enviados.
        /// </summary>
        /// <param name="param">Parámetros de consulta.</param>
        /// <returns>ErrorDto con el resultado del contrato.</returns>
        public ErrorDto<FndGestionesContratoResult> Gestiones_Contrato_Obtener(FndGestionesContratoParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de consulta son requeridos.",
                    -2,
                    new FndGestionesContratoResult());
            }

            var queryResult = DbHelper.ExecuteSingleQuery<FndGestionesContratoResult>(
                new PortalDB(_config),
                param.CodEmpresa,
                SqlContratoObtener,
                default,
                CrearParametrosContrato(param));

            return new ErrorDto<FndGestionesContratoResult>
            {
                Code = queryResult.Code,
                Description = queryResult.Description,
                Result = queryResult.Result ?? new FndGestionesContratoResult()
            };
        }

        /// <summary>
        /// Consulta contratos para renovación según el tipo de gestión.
        /// </summary>
        /// <param name="param">Parámetros de consulta.</param>
        /// <returns>ErrorDto con la lista de contratos.</returns>
        public ErrorDto<List<FndGestionesContratosRenovacionResult>> Gestiones_ContratosRenovacion(FndGestionesContratosRenovacionParams param)
        {
            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de renovación son requeridos.",
                    -2,
                    new List<FndGestionesContratosRenovacionResult>());
            }

            var consulta = CrearConsultaRenovacion(param);
            if (consulta is null)
            {
                return DbHelper.CreateOkResponse(new List<FndGestionesContratosRenovacionResult>());
            }

            return DbHelper.ExecuteListQuery<FndGestionesContratosRenovacionResult>(
                new PortalDB(_config),
                param.CodEmpresa,
                consulta.Sql,
                consulta.Parametros);
        }

        /// <summary>
        /// Actualiza el monto, la fecha de última renovación y la deducción de múltiples contratos.
        /// </summary>
        /// <param name="param">Parámetros de actualización masiva.</param>
        /// <returns>ErrorDto indicando si la operación fue exitosa y cuántos contratos se actualizaron.</returns>
        public ErrorDto<FndGestionesContratoActualizarResult> Gestiones_Contrato_Actualizar(FndGestionesContratoActualizarParams param)
        {
            var result = DbHelper.CreateOkResponse(new FndGestionesContratoActualizarResult());

            if (param is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los parámetros de actualización son requeridos.",
                    -2,
                    new FndGestionesContratoActualizarResult { Success = false });
            }

            var contratos = param.Contratos?.Where(c => c != null).ToList() ?? new List<FndGestionesContratoActualizarItem>();
            if (contratos.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "Debe indicar al menos un contrato para actualizar.",
                    -2,
                    new FndGestionesContratoActualizarResult { Success = false });
            }

            var updateResult = DbHelper.WithConn(new PortalDB(_config), param.CodEmpresa, connection =>
                ActualizarContratos(connection, param.CodOperadora, contratos));

            if (updateResult.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    updateResult.Description ?? "Error al actualizar contratos.",
                    updateResult.Code ?? -1,
                    new FndGestionesContratoActualizarResult { Success = false });
            }

            result.Result = updateResult.Result ?? new FndGestionesContratoActualizarResult { Success = false };
            if (!result.Result.Success)
            {
                result.Code = -1;
                result.Description = "No se actualizaron todos los registros.";
            }

            return result;
        }

        private static DynamicParameters CrearParametrosBusqueda(FndGestionesBuscarContratosParams param)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Top", NormalizarTop(param.Top), DbType.Int32);
            parameters.Add("@Usuario", NormalizarTexto(param.Usuario), DbType.String);
            parameters.Add("@Cod_Operadora", param.Cod_Operadora, DbType.Int32);
            parameters.Add("@Cod_Plan", CrearFiltroLike(param.Cod_Plan), DbType.String);
            parameters.Add("@Cod_Contrato", param.Cod_Contrato, DbType.Int64);
            parameters.Add("@Cedula", CrearFiltroLike(param.Cedula), DbType.String);
            parameters.Add("@Nombre", string.IsNullOrWhiteSpace(param.Cedula) ? CrearFiltroLike(param.Nombre) : null, DbType.String);
            return parameters;
        }

        private static object CrearParametrosContrato(FndGestionesContratoParams param)
        {
            return new
            {
                param.CodContrato,
                param.CodOperadora,
                CodPlan = NormalizarTexto(param.CodPlan)
            };
        }

        private static RenovacionConsulta? CrearConsultaRenovacion(FndGestionesContratosRenovacionParams param)
        {
            var gestion = NormalizarTexto(param.Gestion).ToUpperInvariant();
            return gestion switch
            {
                "O" => new RenovacionConsulta(SqlRenovacionGeneral, new { param.CodOperadora }),
                "P" when !string.IsNullOrWhiteSpace(param.CodPlan) => new RenovacionConsulta(
                    SqlRenovacionPlan,
                    new { param.CodOperadora, CodPlan = NormalizarTexto(param.CodPlan) }),
                "C" when !string.IsNullOrWhiteSpace(param.CodPlan) && param.CodContrato.HasValue => new RenovacionConsulta(
                    SqlRenovacionContrato,
                    new { param.CodOperadora, CodPlan = NormalizarTexto(param.CodPlan), CodContrato = param.CodContrato.Value }),
                _ => null
            };
        }

        private static FndGestionesContratoActualizarResult ActualizarContratos(
            System.Data.IDbConnection connection,
            int codOperadora,
            List<FndGestionesContratoActualizarItem> contratos)
        {
            var updated = 0;
            var noActualizados = new List<int>();

            foreach (var contrato in contratos)
            {
                var rows = connection.Execute(SqlActualizarContrato, new
                {
                    contrato.Monto,
                    CodOperadora = codOperadora,
                    CodPlan = NormalizarTexto(contrato.CodPlan),
                    contrato.CodContrato
                });

                if (rows > 0)
                {
                    updated++;
                    continue;
                }

                noActualizados.Add(contrato.CodContrato);
            }

            return new FndGestionesContratoActualizarResult
            {
                Success = updated == contratos.Count,
                Updated = updated,
                NoActualizados = noActualizados
            };
        }

        private static int NormalizarTop(int top) => Math.Clamp(top, 1, 1000);

        private static string? CrearFiltroLike(string? valor)
        {
            var texto = NormalizarTexto(valor);
            return string.IsNullOrWhiteSpace(texto) ? null : $"%{texto}%";
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private sealed record RenovacionConsulta(string Sql, object Parametros);
    }
}
