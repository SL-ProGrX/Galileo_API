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
        /// Helpers genéricos para reducir duplicación
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="initialResult"></param>
        /// <returns></returns>
        private static ErrorDto<T> CreateOkResponse<T>(T initialResult)
        {
            return new ErrorDto<T>
            {
                Code        = 0,
                Description = "Ok",
                Result      = initialResult
            };
        }


        /// <summary>
        /// Métodos genéricos para reducir duplicación
        /// </summary>
        /// <returns></returns>
        private static ErrorDto CreateOkResponse()
        {
            return new ErrorDto
            {
                Code        = 0,
                Description = "Ok"
            };
        }


        /// <summary>
        /// Método genérico para ejecutar consultas que retornan un solo valor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="codEmpresa"></param>
        /// <param name="sql"></param>
        /// <param name="defaultValue"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private ErrorDto<T> ExecuteSingleQuery<T>(
            int codEmpresa,
            string sql,
            T defaultValue,
            object? parameters = null)
        {
            var result = CreateOkResponse(defaultValue);

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);
                result.Result = connection.Query<T>(sql, parameters).FirstOrDefault()!;
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
                result.Result      = defaultValue;
            }

            return result;
        }


        /// <summary>
        /// Método genérico para ejecutar consultas que no retornan valor
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private ErrorDto ExecuteNonQuery(
            int codEmpresa,
            string sql,
            object? parameters = null)
        {
            var result = CreateOkResponse();

            try
            {
                using var connection = _portalDB.CreateConnection(codEmpresa);
                connection.Execute(sql, parameters);
            }
            catch (Exception ex)
            {
                result.Code        = -1;
                result.Description = ex.Message;
            }

            return result;
        }

        // -----------------------------------------------------------------
        // Métodos públicos
        // -----------------------------------------------------------------

        /// <summary>
        /// Método para consultar el estado de un periodo.
        /// Retorna: "P" si no existe registro.
        /// </summary>
        public ErrorDto<string> Activos_PeriodoEstado_Obtener(int CodEmpresa, DateTime periodo)
        {
            const string sql = @"
                SELECT estado
                FROM   Activos_periodos
                WHERE  anio = @anno
                AND    mes  = @mes";

            var result = ExecuteSingleQuery(
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
        /// Método que ejecuta cierre del periodo.
        /// </summary>
        public ErrorDto Activos_Periodo_Cerrar(int CodEmpresa, string usuario, DateTime periodo)
        {
            var result = CreateOkResponse();

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
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            var result = CreateOkResponse(DateTime.Now);

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