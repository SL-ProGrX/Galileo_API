using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesMensualesDB
    {
        /// <summary>
        /// Obtiene la información base del periodo para el tab Cierre.
        /// </summary>
        public ErrorDto<ExcedentePeriodoDto?> AH_ExcedentesMensuales_Cierre_Periodo_Obtener(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
SELECT TOP 1
    id_periodo,
    Inicio,
    Corte,
    Estado,
    ISNULL(Capitaliza_Porc, 0) AS Capitaliza_Porc,
    ISNULL(Capitaliza_Renta_Aplica, 0) AS Capitaliza_Renta_Aplica,
    ISNULL(Nc_Saldos, 0) AS Nc_Saldos,
    ISNULL(Nc_Mora, 0) AS Nc_Mora,
    RTRIM(ISNULL(Nc_Opcf, '')) AS Nc_Opcf,
    ISNULL(Visible_Sys, 0) AS Visible_Sys,
    ISNULL(Visible_Webapp, 0) AS Visible_Webapp
FROM EXC_PERIODOS
WHERE id_periodo = @PeriodoId;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var result = conn.QueryFirstOrDefault<ExcedentePeriodoDto>(sql, new { PeriodoId = periodoId });
                return DbHelper.CreateOkResponse<ExcedentePeriodoDto?>(result);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<ExcedentePeriodoDto?>(ex.Message);
            }
        }

        /// <summary>
        /// Obtiene la tabla de renta utilizada por el cierre.
        /// </summary>
        public ErrorDto<List<RentaExcedenteDto>> AH_ExcedentesMensuales_Cierre_Renta_Lista(int codEmpresa)
        {
            const string sql = @"
SELECT
    ISNULL(Id_Renta, 0) AS Id_Renta,
    ISNULL(Desde, 0) AS Desde,
    ISNULL(Hasta, 0) AS Hasta,
    ISNULL(Porcentaje, 0) AS Porcentaje
FROM EXC_RENTA_TABLA
ORDER BY Desde;";

            return DbHelper.ExecuteListQuery<RentaExcedenteDto>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Valida si el periodo puede cerrarse.
        /// </summary>
        public ErrorDto<string?> AH_ExcedentesMensuales_Cierre_Valida(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
select dbo.fxExc_Cierre_Valida(@PeriodoId) as Mensaje;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                var result = conn.QueryFirstOrDefault(
                    sql,
                    new { PeriodoId = periodoId });

                string? mensaje = result == null ? null : (string?)result.Mensaje;
                return DbHelper.CreateOkResponse<string?>(mensaje);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<string?>(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta el cierre de excedentes del periodo.
        /// </summary>
        public ErrorDto AH_ExcedentesMensuales_Cierre_Aplicar(
            int codEmpresa,
            int periodoId,
            string usuario)
        {
            const string sql = @"
exec spExc_Cierre @PeriodoId, @Usuario;";

            var parameters = new
            {
                PeriodoId = periodoId,
                Usuario = usuario
            };

            return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, parameters);
        }
    }
}
