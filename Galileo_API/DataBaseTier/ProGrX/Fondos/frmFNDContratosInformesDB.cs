using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndContratosInformesDb
    {
        private const string SpContratoConsulta = "spFnd_Contrato_Consulta";
        private const string SpContratoNotificaEmail = "spFnd_Contrato_Notifica_Email";

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
                    SELECT consec
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
                        consec ASC
                    OFFSET @offset ROWS FETCH NEXT @fetch ROWS ONLY;";

        private static readonly IReadOnlyDictionary<string, int> RetirosSortMap = new Dictionary<string, int>
        {
            ["consec"] = 1,
            ["Consec"] = 1,
        };

        private readonly PortalDB _portalDB;

        public FrmFndContratosInformesDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        public ErrorDto<FndContratosInformesContrato> Fnd_ContratosInformes_Contrato_Obtener(
            int CodEmpresa,
            int operadora,
            string plan,
            int contrato,
            string usuario)
        {
            var result = DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
                connection.QueryFirstOrDefault<FndContratosInformesContrato>(
                    SpContratoConsulta,
                    new
                    {
                        Operadora = operadora,
                        Plan = NormalizarTexto(plan),
                        Contrato = contrato,
                        Usuario = NormalizarTexto(usuario)
                    },
                    commandType: System.Data.CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al consultar contrato.",
                    result.Code.GetValueOrDefault(-1),
                    new FndContratosInformesContrato());
            }

            return DbHelper.CreateOkResponse(result.Result ?? new FndContratosInformesContrato());
        }

        public ErrorDto<string> Fnd_ContratosInformes_Email_Enviar(
            int CodEmpresa,
            int operadora,
            string plan,
            int contrato,
            string usuario)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                var data = connection.QueryFirstOrDefault<dynamic>(
                    SpContratoNotificaEmail,
                    new
                    {
                        Operadora = operadora,
                        Plan = NormalizarTexto(plan),
                        Contrato = contrato,
                        Usuario = NormalizarTexto(usuario)
                    },
                    commandType: System.Data.CommandType.StoredProcedure);

                if (data == null)
                    return "No se obtuvo respuesta del proceso de envio.";

                int pass = Convert.ToInt32(data.Pass);
                string mensaje = Convert.ToString(data.Mensaje) ?? string.Empty;
                return pass == 1 ? "Correo de Solicitud de Contrato enviado a la persona!" : mensaje;
            });
        }

        public ErrorDto<FndContratosInformesLiquidacionesLista> Fnd_ContratosInformes_Retiros_Obtener(
            int CodEmpresa,
            int operadora,
            string plan,
            int contrato,
            FiltrosLazyLoadData filtros)
        {
            var spec = LazyLoadHelper.Build(filtros, RetirosSortMap, "consec");
            var parametros = new
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

            var result = DbHelper.WithConn(_portalDB, CodEmpresa, connection => new FndContratosInformesLiquidacionesLista
            {
                total = connection.QueryFirstOrDefault<int>(SqlRetirosTotal, parametros),
                lineas = connection.Query<FndContratosInformesLiquidacion>(SqlRetirosBuscar, parametros).ToList()
            });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener liquidaciones del contrato.",
                    result.Code.GetValueOrDefault(-1),
                    new FndContratosInformesLiquidacionesLista());
            }

            return DbHelper.CreateOkResponse(result.Result ?? new FndContratosInformesLiquidacionesLista());
        }

        private static string NormalizarTexto(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }
    }
}
