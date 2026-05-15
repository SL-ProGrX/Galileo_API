using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public class FrmFndRastreoVerificaSaldosDB
    {
        private readonly IConfiguration _config;

        private const string SqlPlanes = @"
                    SELECT
                        cod_plan AS Item,
                        descripcion AS Descripcion
                    FROM dbo.fnd_planes
                    ORDER BY descripcion;";

        private const string SqlPeriodos = @"
                    SELECT
                        id_per_historico AS Item,
                        CONVERT(VARCHAR(10), dbo.fxSys_FechaAnioMesToDatetime(anio, mes), 103) + ' 23:59:00' AS Descripcion
                    FROM dbo.fnd_per_historico
                    ORDER BY anio DESC, mes DESC;";

        private const string SqlPeriodo = @"
                    SELECT
                        anio,
                        mes
                    FROM dbo.fnd_per_historico
                    WHERE id_per_historico = @PeriodoId;";

        private const string SqlVerificacionSaldos = @"
                    SELECT TOP (@Lineas)
                        C.cod_operadora,
                        C.cod_plan,
                        C.cod_contrato,
                        S.cedula AS identificacion,
                        S.nombre,
                        ISNULL(A.aportes + A.rendimientos, 0) AS saldo_inicial,
                        ISNULL(M.Debito, 0) AS debitos,
                        ISNULL(M.Credito, 0) AS creditos,
                        (ISNULL(A.aportes + A.rendimientos, 0) + ISNULL(M.Credito, 0) - ISNULL(M.Debito, 0)) AS sf_calculado,
                        (D.aportes + D.rendimientos) AS saldo_final
                    FROM dbo.fnd_contratos C
                    INNER JOIN dbo.fnd_per_cerrados D
                        ON C.cod_operadora = D.cod_operadora
                       AND C.cod_plan = D.cod_plan
                       AND C.cod_contrato = D.cod_contrato
                       AND D.anio = @anio
                       AND D.mes = @mes
                    LEFT JOIN dbo.socios S
                        ON C.cedula = S.cedula
                    LEFT JOIN dbo.fnd_per_cerrados A
                        ON D.cod_operadora = A.cod_operadora
                       AND D.cod_plan = A.cod_plan
                       AND D.cod_contrato = A.cod_contrato
                       AND A.anio = @anioPrev
                       AND A.mes = @mesPrev
                    LEFT JOIN dbo.vFnd_Contratos_Mov_Periodo_Rsm M
                        ON D.cod_operadora = M.cod_operadora
                       AND D.cod_plan = M.cod_plan
                       AND D.cod_contrato = M.cod_contrato
                       AND D.anio = M.anio
                       AND D.mes = M.mes
                    WHERE (@TodosLosPlanes = 1 OR D.cod_plan = @Plan)
                    ORDER BY C.cod_operadora, C.cod_plan, C.cod_contrato;";

        public FrmFndRastreoVerificaSaldosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "La configuración no puede ser nula.");
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Planes_Lista(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlPlanes);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> Periodos_Lista(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                new PortalDB(_config),
                CodEmpresa,
                SqlPeriodos);
        }

        public ErrorDto<List<FndVerificacionSaldoDto>> VerificacionSaldos_Buscar( int CodEmpresa, string Plan, string PeriodoId, int Lineas, bool SoloDiferencias)
        {
            var periodo = ObtenerPeriodo(CodEmpresa, PeriodoId);
            if (periodo.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    periodo.Description ?? "Error al obtener el periodo especificado.",
                    periodo.Code.GetValueOrDefault(-1),
                    new List<FndVerificacionSaldoDto>());
            }

            if (periodo.Result is null)
            {
                return DbHelper.CreateErrorResponse(
                    "No se encontró el periodo especificado.",
                    -1,
                    new List<FndVerificacionSaldoDto>());
            }

            var fechas = CalcularPeriodoAnterior(periodo.Result);
            var dataResult = DbHelper.ExecuteListQuery<FndVerificacionSaldoDto>(
                new PortalDB(_config),
                CodEmpresa,
                SqlVerificacionSaldos,
                CrearParametrosBusqueda(Plan, Lineas, periodo.Result, fechas));

            if (dataResult.Code != 0)
            {
                return dataResult;
            }

            var data = dataResult.Result ?? new List<FndVerificacionSaldoDto>();
            if (SoloDiferencias)
            {
                data = data.Where(x => x.sf_calculado != x.saldo_final).ToList();
            }

            CalcularDiferencias(data);

            return DbHelper.CreateOkResponse(data);
        }

        private ErrorDto<PeriodoSaldo?> ObtenerPeriodo(int codEmpresa, string periodoId)
        {
            return DbHelper.ExecuteSingleQuery<PeriodoSaldo>(
                new PortalDB(_config),
                codEmpresa,
                SqlPeriodo,
                default,
                new { PeriodoId = NormalizarTexto(periodoId) });
        }

        private static PeriodoSaldo CalcularPeriodoAnterior(PeriodoSaldo periodo)
        {
            var mesPrev = periodo.mes == 1 ? 12 : periodo.mes - 1;
            var anioPrev = periodo.mes == 1 ? periodo.anio - 1 : periodo.anio;
            return new PeriodoSaldo(anioPrev, mesPrev);
        }

        private static object CrearParametrosBusqueda(string plan, int lineas, PeriodoSaldo periodo, PeriodoSaldo periodoAnterior)
        {
            var planNormalizado = NormalizarTexto(plan);
            var todosLosPlanes = string.Equals(planNormalizado, "TODOS", StringComparison.OrdinalIgnoreCase);

            return new
            {
                Lineas = Math.Clamp(lineas > 0 ? lineas : 100, 1, 10000),
                anio = periodo.anio,
                mes = periodo.mes,
                anioPrev = periodoAnterior.anio,
                mesPrev = periodoAnterior.mes,
                Plan = planNormalizado,
                TodosLosPlanes = todosLosPlanes ? 1 : 0
            };
        }

        private static void CalcularDiferencias(List<FndVerificacionSaldoDto> data)
        {
            foreach (var item in data)
            {
                item.diferencia = item.saldo_final - item.sf_calculado;
            }
        }

        private static string NormalizarTexto(string? valor) => (valor ?? string.Empty).Trim();

        private sealed record PeriodoSaldo(int anio, int mes);
    }
}