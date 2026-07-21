using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXPeriodosDefinicionDb
    {
        private readonly PortalDB _portalDB;

        public FrmCntXPeriodosDefinicionDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Carga los valores iniciales de la definición de periodos (equivalente a sbInicial de VB6).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa utilizado para seleccionar la conexión.</param>
        /// <param name="codContabilidad">Código de la contabilidad cuyos periodos se consultan.</param>
        /// <returns>Periodo inicial y corte sugeridos para la contabilidad.</returns>
        public ErrorDto<PeriodosDefinicionDto> Inicial(int codEmpresa, int codContabilidad)
        {
            const string sql = @"
                SELECT
                    YEAR(FECHA) AS desdeAnio,
                    MONTH(FECHA) AS desdeMes,
                    CASE
                        WHEN MONTH(FECHA) > 1 THEN YEAR(FECHA) + 1
                        ELSE YEAR(FECHA)
                    END AS hastaAnio,
                    CASE
                        WHEN MONTH(FECHA) > 1 THEN MONTH(FECHA) - 1
                        ELSE 12
                    END AS hastaMes
                FROM (
                    SELECT DATEADD(MONTH, 1, ISNULL(MAX(PERIODO_CORTE), GETDATE())) FECHA
                    FROM CNTX_PERIODOS
                    WHERE COD_CONTABILIDAD = @codContabilidad
                ) X";

            return DbHelper.ExecuteSingleQuery(
                _portalDB,
                codEmpresa,
                sql,
                new PeriodosDefinicionDto(),
                new { codContabilidad });
        }

        /// <summary>
        /// Crea los periodos contables solicitados (equivalente a cmdAplicar de VB6).
        /// </summary>
        /// <param name="codEmpresa">Código de empresa utilizado para seleccionar la conexión.</param>
        /// <param name="codContabilidad">Código de la contabilidad que recibirá los periodos.</param>
        /// <param name="dto">Rango de años y meses que se debe crear.</param>
        /// <returns>Resultado de la creación del rango de periodos.</returns>
        public ErrorDto Crear(int codEmpresa, int codContabilidad, PeriodosDefinicionDto dto)
        {
            if (!dto.desdeAnio.HasValue ||
                !dto.desdeMes.HasValue ||
                !dto.hastaAnio.HasValue ||
                !dto.hastaMes.HasValue)
            {
                return DbHelper.ErrorResponse(
                    "Debe indicar año y mes de inicio y corte.");
            }

            DateTime inicio;
            DateTime fin;
            try
            {
                inicio = new DateTime(
                    dto.desdeAnio.Value,
                    dto.desdeMes.Value,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Unspecified);
                fin = new DateTime(
                    dto.hastaAnio.Value,
                    dto.hastaMes.Value,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Unspecified);
            }
            catch (ArgumentOutOfRangeException)
            {
                return DbHelper.ErrorResponse("El año o mes indicado no es válido.");
            }

            var totalMeses = ((fin.Year - inicio.Year) * 12) + fin.Month - inicio.Month;
            if (totalMeses < 0 || totalMeses > 120)
            {
                return DbHelper.ErrorResponse(
                    "Rango de periodos inválido o excede el máximo permitido.");
            }

            const string sql = @"
                ;WITH Periodos AS
                (
                    SELECT @inicio AS fecha
                    UNION ALL
                    SELECT DATEADD(MONTH, 1, fecha)
                    FROM Periodos
                    WHERE fecha < @fin
                )
                INSERT INTO CNTX_PERIODOS
                    (COD_CONTABILIDAD, anio, mes, estado, PERIODO_CORTE)
                SELECT
                    @codContabilidad,
                    YEAR(fecha),
                    MONTH(fecha),
                    'P',
                    dbo.fxSys_FechaAnioMesToDatetime(YEAR(fecha), MONTH(fecha))
                FROM Periodos
                OPTION (MAXRECURSION 120);

                UPDATE CNTX_PERIODOS
                SET PERIODO_CORTE = dbo.fxSys_FechaAnioMesToDatetime(anio, mes)
                WHERE PERIODO_CORTE IS NULL;";

            return DbHelper.ExecuteNonQuery(
                _portalDB,
                codEmpresa,
                sql,
                new { codContabilidad, inicio, fin });
        }
    }
}
