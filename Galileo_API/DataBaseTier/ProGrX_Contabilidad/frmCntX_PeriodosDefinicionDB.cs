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
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
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
        /// Crea el nuevo periodo
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        public ErrorDto Crear(int codEmpresa, PeriodosDefinicionDto dto)
        {
            var response = new ErrorDto();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = new StringBuilder();

                var anio = dto.desdeAnio;
                var mes = dto.desdeMes;

                while (!(anio == dto.hastaAnio && mes == dto.hastaMes))
                {
                    sql.AppendLine($@"
                        INSERT INTO CNTX_PERIODOS
                        (COD_CONTABILIDAD,anio,mes,estado,PERIODO_CORTE)
                        VALUES
                        ({codEmpresa},{anio},{mes},'P',
                         dbo.fxSys_FechaAnioMesToDatetime({anio},{mes}))");

                    if (mes == 12)
                    {
                        mes = 1;
                        anio++;
                    }
                    else
                    {
                        mes++;
                    }
                }

                sql.AppendLine($@"
                    INSERT INTO CNTX_PERIODOS
                    (COD_CONTABILIDAD,anio,mes,estado,PERIODO_CORTE)
                    VALUES
                    ({codEmpresa},{anio},{mes},'P',
                     dbo.fxSys_FechaAnioMesToDatetime({anio},{mes}))");

                sql.AppendLine(@"
                    UPDATE CNTX_PERIODOS
                    SET PERIODO_CORTE = dbo.fxSys_FechaAnioMesToDatetime(anio,mes)
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
