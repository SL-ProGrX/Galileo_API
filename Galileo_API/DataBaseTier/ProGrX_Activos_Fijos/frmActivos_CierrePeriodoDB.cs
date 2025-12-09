using Dapper;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosCierrePeriodoDb
    {
        private readonly MActivosFijos _mActivos;
        private readonly PortalDB _portalDB;

        public FrmActivosCierrePeriodoDb(IConfiguration config)
        {
            _mActivos = new MActivosFijos(config);
            _portalDB = new PortalDB(config);
        }

       /// <summary>
       ///  Método para consultar el estado de un periodo.
       /// </summary>
       /// <param name="CodEmpresa"></param>
       /// <param name="periodo"></param>
       /// <returns></returns>
        public ErrorDto<string> Activos_PeriodoEstado_Obtener(int CodEmpresa, DateTime periodo)
        {
            const string sql = @"
                SELECT estado
                FROM   Activos_periodos
                WHERE  anio = @anno
                AND    mes  = @mes";

            var result = DbHelper.ExecuteSingleQuery(
                _portalDB,
                CodEmpresa,
                sql,
                defaultValue: string.Empty,
                parameters: new { anno = periodo.Year, mes = periodo.Month });

            // Si no hay registro o viene vacío, asumimos "P"
            if (result.Code == 0 && string.IsNullOrWhiteSpace(result.Result))
            {
                result.Result = "P";
            }

            return result;
        }

       /// <summary>
       ///  Método que ejecuta cierre del periodo.
       /// </summary>
       /// <param name="CodEmpresa"></param>
       /// <param name="usuario"></param>
       /// <param name="periodo"></param>
       /// <returns></returns>
        public ErrorDto Activos_Periodo_Cerrar(int CodEmpresa, string usuario, DateTime periodo)
        {
            var result = DbHelper.CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);

                const string sql = @"
                    EXEC spActivos_CierreAuxiliar @anno, @mes, @user";

                var spResult = connection.Query<int>(sql, new
                {
                    anno = periodo.Year,
                    mes  = periodo.Month,
                    user = usuario
                }).FirstOrDefault();

                if (spResult != 0)
                {
                    result.Code        = -1;
                    result.Description = "Error al cerrar el periodo.";
                }
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// Consulta el periodo pendiente (periodo actual de contabilidad).
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            var result = DbHelper.CreateOkResponse(DateTime.Now);

            try
            {
                result.Result = _mActivos.fxCntX_PeriodoActual(CodEmpresa, contabilidad);
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }
    }
}