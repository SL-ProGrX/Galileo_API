using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmFSL_ExpedienteGestionesDB
    {
        private readonly IConfiguration _config;

        public frmFSL_ExpedienteGestionesDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<FslGestionesListaDatos>> FslGestiones_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<FslGestionesListaDatos>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cod_gestion as item, rtrim(cod_gestion) + ' - ' + DESCRIPCION as descripcion from FSL_TIPOS_GESTIONES WHERE ACTIVA = 1";
                    response.Result = connection.Query<FslGestionesListaDatos>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslGestiones_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDto FslGestion_Agregar(int CodCliente, FslGestionAgregar gestion)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;


            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var procedure = "[spFSL_GestionRegistra]";
                    var values = new
                    {
                        Expediente = gestion.cod_expediente,
                        Tipo = gestion.cod_gestion,
                        Notas = gestion.notas,
                        Usuario = gestion.usuario
                    };

                    connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;


        }
    }
}