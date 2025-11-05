using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.AH;

namespace PgxAPI.DataBaseTier.ProGrX_Procesos
{
    public class frmAH_ConciliadorPatronalDB
    {
        private readonly IConfiguration _config;

        public frmAH_ConciliadorPatronalDB(IConfiguration config)
        {
            _config = config;
        }

        public List<InstitucionesConciliarDTO> obtener_InstitucionesConciliar(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<InstitucionesConciliarDTO> info = new List<InstitucionesConciliarDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT 
                                    COD_INSTITUCION AS Idx, 
                                    '[' + COD_DIVISA + ']  ' + DESCRIPCION AS ItmX
                                FROM 
                                    INSTITUCIONES 
                                WHERE 
                                    ACTIVA = 1
                                ORDER BY 
                                    COD_INSTITUCION;";

                    info = connection.Query<InstitucionesConciliarDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }



        public List<InstitucionesConciliarDTO> Conciliacion_Aplicar(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            List<InstitucionesConciliarDTO> info = new List<InstitucionesConciliarDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"SELECT 
                                    COD_INSTITUCION AS Idx, 
                                    '[' + COD_DIVISA + ']  ' + DESCRIPCION AS ItmX
                                FROM 
                                    INSTITUCIONES 
                                WHERE 
                                    ACTIVA = 1
                                ORDER BY 
                                    COD_INSTITUCION;";

                    info = connection.Query<InstitucionesConciliarDTO>(query).ToList();

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