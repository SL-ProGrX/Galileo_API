using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesMensualesDB
    {
        /// <summary>
        /// Obtiene el monto sugerido para la distribución mensual del periodo y corte indicados.
        /// </summary>
        public ErrorDto<decimal?> AH_ExcedentesMensuales_Mensual_Monto_Obtener(
            int codEmpresa,
            int periodoId,
            DateTime corte,
            string tipoAplicacion)
        {
            const string sql = @"
exec spExc_Mnt_Distribuir_Dato @PeriodoId, @Corte, @Tipo;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = conn.QueryFirstOrDefault(
                    sql,
                    new
                    {
                        PeriodoId = periodoId,
                        Corte = corte,
                        Tipo = tipoAplicacion
                    });

                decimal? monto = result == null ? null : (decimal?)result.Monto;
                return DbHelper.CreateOkResponse<decimal?>(monto);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<decimal?>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la utilidad contable del mes para base de aplicación real contable.
        /// </summary>
        public ErrorDto<decimal?> AH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener(
            int codEmpresa,
            int anio,
            int mes,
            int enlace)
        {
            const string sql = @"
select dbo.fxCntX_Utilidad_Mes_SinCF(@Anio, @Mes, @Enlace, '', '') as Excedente;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = conn.QueryFirstOrDefault(
                    sql,
                    new
                    {
                        Anio = anio,
                        Mes = mes,
                        Enlace = enlace
                    });

                decimal? excedente = result == null ? null : (decimal?)result.Excedente;
                return DbHelper.CreateOkResponse<decimal?>(excedente);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<decimal?>(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza el modo base de aplicación del periodo.
        /// </summary>
        public ErrorDto AH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar(
            int codEmpresa,
            int periodoId,
            string tipoAplicacion,
            string usuario)
        {
            const string sql = @"
exec spExc_Periodo_Modo_Aplicacion @PeriodoId, @TipoAplicacion, @Usuario;";

            var parameters = new
            {
                PeriodoId = periodoId,
                TipoAplicacion = tipoAplicacion,
                Usuario = usuario
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, parameters);
        }

        /// <summary>
        /// Valida si el corte ya fue aplicado para el periodo indicado.
        /// </summary>
        public ErrorDto<string?> AH_ExcedentesMensuales_Mensual_Valida(
            int codEmpresa,
            int periodoId,
            DateTime corte)
        {
            const string sql = @"
select dbo.fxEXC_Periodo_Corte_Valida(@PeriodoId, @Corte) as Resultado;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = conn.QueryFirstOrDefault(
                    sql,
                    new
                    {
                        PeriodoId = periodoId,
                        Corte = corte
                    });

                string? mensaje = result == null ? null : (string?)result.Resultado;
                return DbHelper.CreateOkResponse<string?>(mensaje);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<string?>(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta la aplicación mensual de excedentes para el periodo y corte indicados.
        /// </summary>
        public ErrorDto<FrmAhExcedentesMensualesMensualResultadoDto?> AH_ExcedentesMensuales_Mensual_Aplicar(
            int codEmpresa,
            int periodoId,
            DateTime corte,
            decimal monto,
            string tipoAplicacion,
            string usuario)
        {
            const string sql = @"
exec spExc_Cierre_Aplicacion_Mensual @PeriodoId, @Usuario, @Corte, @Monto, @TipoApl;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = conn.QueryFirstOrDefault(
                    sql,
                    new
                    {
                        PeriodoId = periodoId,
                        Usuario = usuario,
                        Corte = corte,
                        Monto = monto,
                        TipoApl = tipoAplicacion
                    });

                if (result == null)
                {
                    return DbHelper.CreateOkResponse<FrmAhExcedentesMensualesMensualResultadoDto?>(null);
                }

                decimal ahorro = result.ahorro == null ? 0 : (decimal)result.ahorro;
                decimal aholiq = result.aholiq == null ? 0 : (decimal)result.aholiq;
                decimal aporte = result.aporte == null ? 0 : (decimal)result.aporte;
                decimal apoliq = result.apoliq == null ? 0 : (decimal)result.apoliq;
                decimal capitaliza = result.capitaliza == null ? 0 : (decimal)result.capitaliza;
                decimal capliq = result.capliq == null ? 0 : (decimal)result.capliq;

                var dto = new FrmAhExcedentesMensualesMensualResultadoDto
                {
                    casos_general = result.total == null ? 0 : (int)result.total,
                    total_ahorros = ahorro + aholiq,
                    total_aportes = aporte + apoliq + capitaliza + capliq,
                    factor = result.factor == null ? 0 : (decimal)result.factor,
                    total_distribuido = result.excedente == null ? 0 : (decimal)result.excedente,
                    casos_proceso = result.total == null ? 0 : (int)result.total
                };

                return DbHelper.CreateOkResponse<FrmAhExcedentesMensualesMensualResultadoDto?>(dto);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmAhExcedentesMensualesMensualResultadoDto?>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la configuración mensual del periodo seleccionado.
        /// </summary>
        public ErrorDto<FrmAhExcedentesMensualesMensualPeriodoDto?> AH_ExcedentesMensuales_Mensual_Periodo_Obtener(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
SELECT
    RTRIM(ISNULL(ESTADO, '')) AS estado,
    RTRIM(ISNULL(TIPO_APL_MENSUAL, '')) AS tipo_apl_mensual,
    RTRIM(ISNULL(TIPO_APL_MENSUAL_DESC, '')) AS tipo_apl_mensual_desc
FROM vExc_Periodos_Consulta
WHERE ID_PERIODO = @PeriodoId;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = conn.QueryFirstOrDefault<FrmAhExcedentesMensualesMensualPeriodoDto>(
                    sql,
                    new { PeriodoId = periodoId });

                return DbHelper.CreateOkResponse<FrmAhExcedentesMensualesMensualPeriodoDto?>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<FrmAhExcedentesMensualesMensualPeriodoDto?>(ex.Message);
            }
        }
    }
}
