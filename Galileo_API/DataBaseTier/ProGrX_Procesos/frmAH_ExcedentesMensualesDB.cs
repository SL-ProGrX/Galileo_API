using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;

namespace PgxAPI.DataBaseTier.ProGrX_Procesos
{
    public class frmAH_ExcedentesMensualesDB
    {
        private readonly IConfiguration _config;

        public frmAH_ExcedentesMensualesDB(IConfiguration config)
        {
            _config = config;
        }

        public List<excParametrosDTO> obtener_ParametrosExcedentes(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<excParametrosDTO> info = new List<excParametrosDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select * from EXC_PARAMETROS order by cod_parametro asc";

                    info = connection.Query<excParametrosDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<excPeriodosDTO> obtener_PeriodosExcedentes(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<excPeriodosDTO> info = new List<excPeriodosDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT * FROM vExc_Periodos order by IDX desc";

                    info = connection.Query<excPeriodosDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }

        public List<excPeriodosCorte> obtener_PeriodosCorte(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<excPeriodosCorte> info = new List<excPeriodosCorte>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select CORTE_DATETIME_STR as 'IdX', CORTE_DATE_STR as 'ItmX'  from vExc_Periodos_Cortes order by idx desc";

                    info = connection.Query<excPeriodosCorte>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<excPeriodosCorte> aplicar_PeriodoExcedente(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<excPeriodosCorte> info = new List<excPeriodosCorte>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select CORTE_DATETIME_STR as 'IdX', CORTE_DATE_STR as 'ItmX'  from vExc_Periodos_Cortes order by idx desc";

                    info = connection.Query<excPeriodosCorte>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<ResumenExcedenteMDTO> obtener_ResumenMensual(int CodEmpresa, string CodPeriodo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ResumenExcedenteMDTO> info = new List<ResumenExcedenteMDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT *,
                                       base AS total
                                FROM vExc_Periodos_Cortes_Resumen
                                        WHERE id_periodo = '{CodPeriodo}'
                                                           ORDER BY corte DESC;";

                    info = connection.Query<ResumenExcedenteMDTO>(query).ToList();

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