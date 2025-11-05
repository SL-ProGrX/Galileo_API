using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_ExcedentesPeriodosDB
    {
        private readonly IConfiguration _config;

        public frmAH_ExcedentesPeriodosDB(IConfiguration config)
        {
            _config = config;
        }

        public List<ExcedentePeriodoDTO> Periodo_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcedentePeriodoDTO> info = new List<ExcedentePeriodoDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select * from EXC_PERIODOS order by id_Periodo desc";

                    info = connection.Query<ExcedentePeriodoDTO>(query).ToList();


                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public int ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            int result = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "";

                    if (tipo == "desc")
                    {
                        if (consecutivo == 0)
                        {
                            query = $@"select Top 1 ID_PERIODO from EXC_PERIODOS
                                    order by ID_PERIODO desc";
                        }
                        else
                        {
                            query = $@"select Top 1 ID_PERIODO from EXC_PERIODOS
                                    where ID_PERIODO < {consecutivo} order by ID_PERIODO desc";
                        }

                    }
                    else
                    {
                        query = $@"select Top 1 ID_PERIODO from EXC_PERIODOS
                                    where ID_PERIODO > {consecutivo} order by ID_PERIODO asc";
                    }


                    result = connection.Query<int>(query).FirstOrDefault();

                    result = result == 0 || result == consecutivo ? consecutivo : result;



                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return result;
        }


        public ExcedentePeriodoDTO periodo_Obtenerselect(int CodEmpresa, int consecutivo)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ExcedentePeriodoDTO info = new ExcedentePeriodoDTO();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM EXC_PERIODOS
                                WHERE ID_PERIODO = {consecutivo}";

                    info = connection.Query<ExcedentePeriodoDTO>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }



        public List<ExcedentePeriodoDTO> PeriodosExcedente_Obtener(int CodEmpresa, int consecutivo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcedentePeriodoDTO> info = new List<ExcedentePeriodoDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT P.* FROM vExc_Periodos_Consulta P WHERE P.ID_PERIODO = {consecutivo}";

                    info = connection.Query<ExcedentePeriodoDTO>(query).ToList();


                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }



        public List<BitacoraExcedenteDTO> BitacoraExcedente_Obtener(int CodEmpresa, int consecutivo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<BitacoraExcedenteDTO> info = new List<BitacoraExcedenteDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"	SELECT * FROM vExc_Periodos_Bitacora WHERE id_periodo = {consecutivo} ORDER BY registro_fecha DESC";

                    info = connection.Query<BitacoraExcedenteDTO>(query).ToList();


                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<ExcedentePeriodoDTO> Periodo_Insertar(int CodEmpresa, int consecutivo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcedentePeriodoDTO> info = new List<ExcedentePeriodoDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"	SELECT * FROM vExc_Periodos_Bitacora WHERE id_periodo = {consecutivo} ORDER BY registro_fecha DESC";

                    info = connection.Query<ExcedentePeriodoDTO>(query).ToList();


                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<ExcedentePeriodoDTO> Periodo_Actualizar(int CodEmpresa, int consecutivo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcedentePeriodoDTO> info = new List<ExcedentePeriodoDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"	SELECT * FROM vExc_Periodos_Bitacora WHERE id_periodo = {consecutivo} ORDER BY registro_fecha DESC";

                    info = connection.Query<ExcedentePeriodoDTO>(query).ToList();


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