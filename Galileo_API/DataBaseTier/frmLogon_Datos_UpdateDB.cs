using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using System.Data;


namespace Galileo.DataBaseTier
{
    public class FrmLogon_DatosUpdateDb
    {

        private readonly IConfiguration _config;

        public FrmLogon_DatosUpdateDb(IConfiguration config)
        {
            _config = config;
        }

        public LogonUpdateData LogonObtenerDatosUsuario(string usuario)
        {
            LogonUpdateData? result = null;
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    const string query = @"
                                        SELECT [userid] AS id,
                                                USUARIO AS usuario,   
                                                EMAIL,
                                                TEL_CELL AS tell_cell
                                            FROM US_USUARIOS
                                            WHERE USUARIO = @usuario";

                    // Execute the query and map the result to LogonUpdateData
                    result = connection.QueryFirstOrDefault<LogonUpdateData>(query, new { Usuario = usuario });

                }
            }
            catch (Exception ex)
            {
                // Optionally log or handle the exception
                _ = ex.Message;
            }
            return result ?? new LogonUpdateData();
        }


        public ErrorDto LogonUpdateDatosUsuario(LogonUpdateData Info)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spSEG_Logon_Info_Update]";
                    var values = new
                    {
                        Usuario = Info.usuario,
                        Email = Info.email,
                        Movil = Info.tell_cell,
                        UserID = Info.id
                    };

                    // Execute the stored procedure and get the number of affected rows
                    int rowsAffected = connection.Execute(procedure, values, commandType: CommandType.StoredProcedure);

                    // Check if any rows were affected as an indication of success
                    if (rowsAffected > 0)
                    {
                        resp.Code = 0; // Success code
                        resp.Description = "Información actualziada correctamente";
                    }
                    else
                    {
                        resp.Code = -1; // Error code if no rows were updated
                        resp.Description = "Ningún registro actualizado. Verifique el id enviado";
                    }

                }
            }
            catch (SqlException ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }


    }

}//end namespace
