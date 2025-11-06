using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AH;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_ExcedentesPeriodosDB
    {
        private readonly IConfiguration _config;

        public frmAH_ExcedentesPeriodosDB(IConfiguration config)
        {
            _config = config;
        }

        public List<ExcedentePeriodoDto> Periodo_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcedentePeriodoDto> info = new List<ExcedentePeriodoDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select * from EXC_PERIODOS order by id_Periodo desc";

                    info = connection.Query<ExcedentePeriodoDto>(query).ToList();


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


        public ExcedentePeriodoDto periodo_Obtenerselect(int CodEmpresa, int consecutivo)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ExcedentePeriodoDto info = new ExcedentePeriodoDto();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM EXC_PERIODOS
                                WHERE ID_PERIODO = {consecutivo}";

                    info = connection.Query<ExcedentePeriodoDto>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }



        public List<ExcedentePeriodoDto> PeriodosExcedente_Obtener(int CodEmpresa, int consecutivo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcedentePeriodoDto> info = new List<ExcedentePeriodoDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT P.* FROM vExc_Periodos_Consulta P WHERE P.ID_PERIODO = {consecutivo}";

                    info = connection.Query<ExcedentePeriodoDto>(query).ToList();


                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }



        public List<BitacoraExcedenteDto> BitacoraExcedente_Obtener(int CodEmpresa, int consecutivo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<BitacoraExcedenteDto> info = new List<BitacoraExcedenteDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"	SELECT * FROM vExc_Periodos_Bitacora WHERE id_periodo = {consecutivo} ORDER BY registro_fecha DESC";

                    info = connection.Query<BitacoraExcedenteDto>(query).ToList();


                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<ExcedentePeriodoDto> Periodo_Insertar(int CodEmpresa, int consecutivo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcedentePeriodoDto> info = new List<ExcedentePeriodoDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"	SELECT * FROM vExc_Periodos_Bitacora WHERE id_periodo = {consecutivo} ORDER BY registro_fecha DESC";

                    info = connection.Query<ExcedentePeriodoDto>(query).ToList();


                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<ExcedentePeriodoDto> Periodo_Actualizar(int CodEmpresa, int consecutivo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<ExcedentePeriodoDto> info = new List<ExcedentePeriodoDto>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"	SELECT * FROM vExc_Periodos_Bitacora WHERE id_periodo = {consecutivo} ORDER BY registro_fecha DESC";

                    info = connection.Query<ExcedentePeriodoDto>(query).ToList();


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