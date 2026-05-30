using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public partial class FrmAhExcedentesMensualesDB
    {
        /// <summary>
        /// Obtiene el resumen de cortes del periodo para el tab Resumen.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="periodoId">Periodo seleccionado.</param>
        /// <returns>Lista resumen de cortes del periodo.</returns>
        public ErrorDto<List<ResumenExcedenteMDto>> Patrimonio_frmAH_ExcedentesMensuales_Resumen_Lista(
            int codEmpresa,
            int periodoId)
        {
            const string sql = @"
SELECT
    CAST(id_periodo AS varchar(20)) AS id_periodo,
    CONVERT(varchar(19), corte, 120) AS corte,
    RTRIM(ISNULL(corte_date_str, '')) AS corte_date_str,
    RTRIM(ISNULL(corte_datetime_str, '')) AS corte_datetime_str,
    CAST(ISNULL(casos, 0) AS varchar(20)) AS casos,
    CAST(ISNULL(base, 0) AS varchar(30)) AS total,
    CAST(ISNULL(bruto, 0) AS varchar(30)) AS bruto
FROM vExc_Periodos_Cortes_Resumen
WHERE id_periodo = @periodoId
ORDER BY corte DESC;";

            var parameters = new
            {
                periodoId
            };

            return DbHelper.ExecuteListQuery<ResumenExcedenteMDto>(_portalDb, codEmpresa, sql, parameters);
        }
    }
}
