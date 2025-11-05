using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.AH;

namespace PgxAPI.DataBaseTier
{
    public class frmAH_ReportesRangosFechaDB
    {
        private readonly IConfiguration _config;

        public frmAH_ReportesRangosFechaDB(IConfiguration config)
        {
            _config = config;
        }

        public List<EstadosPersonaPatrimonioDTO> EstadosPersonaPatrimonio_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            List<EstadosPersonaPatrimonioDTO> info = new List<EstadosPersonaPatrimonioDTO>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT RTRIM(cod_Estado) AS IdX, 
                                               RTRIM(Descripcion) AS ItmX
                                        FROM AFI_ESTADOS_PERSONA
                                        WHERE Activo = 1;";

                    info = connection.Query<EstadosPersonaPatrimonioDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return info;
        }


        public List<InstitucionesPatrimonioDTO> InstitucionesPatriomonio_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            List<InstitucionesPatrimonioDTO> info = new List<InstitucionesPatrimonioDTO>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT cod_institucion AS Idx, 
                                           Descripcion AS ItmX
                                    FROM INSTITUCIONES
                                    WHERE Activa = 1;";

                    info = connection.Query<InstitucionesPatrimonioDTO>(query).ToList();

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