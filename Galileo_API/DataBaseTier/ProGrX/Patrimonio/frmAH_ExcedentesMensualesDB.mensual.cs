using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesMensualesDB
    {
        /// <summary>
        /// Obtiene el monto sugerido para la distribución mensual del periodo y corte indicados.
        /// </summary>
        public ErrorDto<decimal?> Patrimonio_frmAH_ExcedentesMensuales_Mensual_Monto_Obtener(
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
        public ErrorDto<decimal?> Patrimonio_frmAH_ExcedentesMensuales_Mensual_UtilidadContable_Obtener(
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
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Mensual_BaseAplicacion_Actualizar(
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
        public ErrorDto<string?> Patrimonio_frmAH_ExcedentesMensuales_Mensual_Valida(
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
        public ErrorDto Patrimonio_frmAH_ExcedentesMensuales_Mensual_Aplicar(
            int codEmpresa,
            int periodoId,
            DateTime corte,
            decimal monto,
            string tipoAplicacion,
            string usuario)
        {
            const string sql = @"
exec spExc_Cierre_Aplicacion_Mensual @PeriodoId, @Usuario, @Corte, @Monto, @TipoApl;";

            var parameters = new
            {
                PeriodoId = periodoId,
                Usuario = usuario,
                Corte = corte,
                Monto = monto,
                TipoApl = tipoAplicacion
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, parameters);
        }
    }
}
