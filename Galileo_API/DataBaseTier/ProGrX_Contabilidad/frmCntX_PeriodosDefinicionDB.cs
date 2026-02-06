using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;
using System.Text;

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
        /// Carga inicial de la pantalla de definición de periodos
        /// (equivalente a sbInicial VB6)
        /// </summary>
        public ErrorDto<PeriodosDefinicionDto> Inicial(int codEmpresa)
        {
            var response = new ErrorDto<PeriodosDefinicionDto>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                const string sql = @"
                    SELECT 
                        YEAR(FECHA)  AS desdeAnio,
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
                        SELECT DATEADD(MONTH,1,ISNULL(MAX(PERIODO_CORTE),GETDATE())) FECHA
                        FROM CNTX_PERIODOS
                        WHERE COD_CONTABILIDAD = @codEmpresa
                    ) X";

                response.Result = cn.QueryFirst<PeriodosDefinicionDto>(
                    sql,
                    new { codEmpresa }
                );
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Crea los periodos contables
        /// (cmdAplicar VB6)
        /// </summary>
        public ErrorDto Crear(int codEmpresa, PeriodosDefinicionDto dto)
        {
            var response = new ErrorDto();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = new StringBuilder();

                if (!dto.desdeAnio.HasValue ||
                    !dto.desdeMes.HasValue ||
                    !dto.hastaAnio.HasValue ||
                    !dto.hastaMes.HasValue)
                {
                    response.Code = -1;
                    response.Description = "Debe indicar año y mes de inicio y corte.";
                    return response;
                }


                var desdeAnio = dto.desdeAnio.Value;
                var desdeMes = dto.desdeMes.Value;
                var hastaAnio = dto.hastaAnio.Value;
                var hastaMes = dto.hastaMes.Value;

                var inicio = new DateTime(
                    desdeAnio,
                    desdeMes,
                    1,
                    0, 0, 0,
                    DateTimeKind.Local
                );

                var fin = new DateTime(
                    hastaAnio,
                    hastaMes,
                    1,
                    0, 0, 0,
                    DateTimeKind.Local
                );

                var totalMeses =
                    ((fin.Year - inicio.Year) * 12) +
                    fin.Month - inicio.Month;

                if (totalMeses < 0 || totalMeses > 120)
                {
                    response.Code = -1;
                    response.Description =
                        "Rango de periodos inválido o excede el máximo permitido.";
                    return response;
                }

                var fecha = inicio;

                for (int i = 0; i <= totalMeses; i++)
                {
                    sql.AppendLine($@"
                        INSERT INTO CNTX_PERIODOS
                        (COD_CONTABILIDAD, anio, mes, estado, PERIODO_CORTE)
                        VALUES
                        ({codEmpresa}, {fecha.Year}, {fecha.Month}, 'P',
                         dbo.fxSys_FechaAnioMesToDatetime({fecha.Year}, {fecha.Month}))");

                    fecha = fecha.AddMonths(1);
                }

                sql.AppendLine(@"
                    UPDATE CNTX_PERIODOS
                    SET PERIODO_CORTE = dbo.fxSys_FechaAnioMesToDatetime(anio, mes)
                    WHERE PERIODO_CORTE IS NULL");

                cn.Execute(sql.ToString());
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}
