using PgxAPI.Models.ERROR; 
using Microsoft.Data.SqlClient;
using Dapper; 

namespace PgxAPI.DataBaseTier.ProGrX_Activos_Fijos
{
    public class frmActivos_CierrePeriodoDB
    {

        private readonly IConfiguration? _config;
        private readonly int vModulo = 36;
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly mActivosFijos _mActivos;

        public frmActivos_CierrePeriodoDB(IConfiguration? config)
        {
            _config = config;
            _mActivos = new mActivosFijos(_config);
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Método para consultar el estado de un periodo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="periodo"></param>
        /// <returns>Desrcripcion del estado del periodo</returns>
        public ErrorDto<string> Activos_PeriodoEstado_Obtener(int CodEmpresa, DateTime periodo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select estado from Activos_periodos where anio =@anno and mes =  @mes ";

                    result.Result = connection.Query<string>(query, new { anno = periodo.Year, mes = periodo.Month }).FirstOrDefault();

                    if (result.Result == null && result.Result == "")
                    {
                        result.Result = "P";
                    }
                   
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Método que ejecuta cierre del periodo
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="periodo"></param>
        /// <returns></returns>
        public ErrorDto Activos_Periodo_Cerrar(int CodEmpresa, string usuario, DateTime periodo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var result = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spActivos_CierreAuxiliar @anno, @mes, @user ";
                    int res = 0;
                   res= connection.Query<int>(query, new { anno = periodo.Year, mes = periodo.Month, user = usuario }).FirstOrDefault();
                }

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;

            }
            return result;
        }

        /// <summary>
        /// Consulta el periodo pendiente
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contabilidad"></param>
        /// <returns></returns>
        public ErrorDto<DateTime> Activos_Periodo_Consultar(int CodEmpresa, int contabilidad)
        {
            

              var result = new ErrorDto<DateTime>
            {
                Code = 0,
                Description = "Ok",
                Result= DateTime.Now,
            };
            try
            {
                result.Result = _mActivos.fxCntX_PeriodoActual(CodEmpresa, contabilidad);

            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;

            }
            return result;
        }

    }
}
