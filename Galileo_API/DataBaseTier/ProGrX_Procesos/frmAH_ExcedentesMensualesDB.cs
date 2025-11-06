using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AH;

namespace PgxAPI.DataBaseTier.ProGrX_Procesos
{
    public class frmAH_ExcedentesMensualesDB
    {
        private readonly IConfiguration _config;

        public frmAH_ExcedentesMensualesDB(IConfiguration config)
        {
            _config = config;
        }

        public List<ExcParametrosDto> obtener_ParametrosExcedentes(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcParametrosDto> info = new List<ExcParametrosDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select * from EXC_PARAMETROS order by cod_parametro asc";

                    info = connection.Query<ExcParametrosDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<ExcPeriodosDto> obtener_PeriodosExcedentes(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcPeriodosDto> info = new List<ExcPeriodosDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT * FROM vExc_Periodos order by IDX desc";

                    info = connection.Query<ExcPeriodosDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public List<ExcPeriodosCorte> obtener_PeriodosCorte(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcPeriodosCorte> info = new List<ExcPeriodosCorte>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select CORTE_DATETIME_STR as 'IdX', CORTE_DATE_STR as 'ItmX'  from vExc_Periodos_Cortes order by idx desc";

                    info = connection.Query<ExcPeriodosCorte>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<ExcPeriodosCorte> aplicar_PeriodoExcedente(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcPeriodosCorte> info = new List<ExcPeriodosCorte>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select CORTE_DATETIME_STR as 'IdX', CORTE_DATE_STR as 'ItmX'  from vExc_Periodos_Cortes order by idx desc";

                    info = connection.Query<ExcPeriodosCorte>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<ResumenExcedenteMDto> obtener_ResumenMensual(int CodEmpresa, string CodPeriodo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ResumenExcedenteMDto> info = new List<ResumenExcedenteMDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT *,
                                       base AS total
                                FROM vExc_Periodos_Cortes_Resumen
                                        WHERE id_periodo = '{CodPeriodo}'
                                                           ORDER BY corte DESC;";

                    info = connection.Query<ResumenExcedenteMDto>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

    }
}