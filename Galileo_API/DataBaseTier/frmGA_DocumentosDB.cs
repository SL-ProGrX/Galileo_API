using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.GA;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmGaDocumentosDb
    {
        private readonly IConfiguration _config;
        private const string connectionStringName = "GAConnString";

        public FrmGaDocumentosDb(IConfiguration config)
        {
            _config = config;
        }
        
        /// <summary>
        /// Obtiene los tipos de documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Usuario"></param>
        /// <param name="Modulo"></param>
        /// <returns></returns>
        public ErrorDto<List<TiposDocumentosArchivosDto>> TiposDocumentos_Obtener(int CodEmpresa, string Usuario, string Modulo)
        {
            var resp = new ErrorDto<List<TiposDocumentosArchivosDto>>();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    var procedure = "[spGA_Interface_Tipos_Documentos]";
                    var values = new
                    {
                        Empresa = CodEmpresa,
                        Usuario = Usuario,
                        Modulo = Modulo
                    };
                    resp.Result = connection.Query<TiposDocumentosArchivosDto>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                resp.Result = null!;
            }
            return resp;
        }

        /// <summary>
        /// Insentar los documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto Documentos_Insertar(int CodEmpresa, DocumentosArchivoDto data)
        {
            ErrorDto resp = new ErrorDto();
            try
            {

                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    string queryExist = $@"SELECT COUNT(*) FROM GA_Files WHERE Llave_01 = '{data.llave_01}' AND Llave_02 = '{data.llave_02}' AND Llave_03 = '{data.llave_03}'";
                    int count = connection.QuerySingle<int>(queryExist);
                    if (count > 0)
                    {
                        resp.Code = -1;
                        resp.Description = "Ya existe un documento con la misma llave.";
                        return resp;
                    }


                    string query = @"
                INSERT INTO GA_Files (
                    EmpresaId, ModuloId, TypeId, Llave_01, Llave_02, Llave_03, FileType, FileName, FileContent, Vencimiento, 
                    FechaEmision, RegistroFecha, RegistroUsuario
                ) VALUES (
                    @EmpresaId, @ModuloId, @TypeId, @Llave_01, @Llave_02, @Llave_03, @FileType, @FileName, @FileContent, @Vencimiento, 
                    @FechaEmision, @RegistroFecha, @RegistroUsuario
                )";

                    using (var command = new SqlCommand(query, connection))
                    {

                        command.Parameters.AddWithValue("@EmpresaId", data.empresaid);
                        command.Parameters.AddWithValue("@ModuloId", data.moduloid);
                        command.Parameters.AddWithValue("@TypeId", data.typeid);
                        command.Parameters.AddWithValue("@Llave_01", data.llave_01);
                        command.Parameters.AddWithValue("@Llave_02", data.llave_02);
                        command.Parameters.AddWithValue("@Llave_03", data.llave_03);
                        command.Parameters.AddWithValue("@FileType", data.filetype);
                        command.Parameters.AddWithValue("@FileName", data.filename);
                        command.Parameters.AddWithValue("@FileContent", data.filecontent != null ? (object)data.filecontent : DBNull.Value); // Maneja datos binarios correctamente
                        command.Parameters.AddWithValue("@Vencimiento", DateTime.Parse(data.vencimiento, System.Globalization.CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("@FechaEmision", DateTime.Parse(data.fechaemision, System.Globalization.CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("@RegistroFecha", DateTime.Parse(data.registrofecha, System.Globalization.CultureInfo.InvariantCulture));
                        command.Parameters.AddWithValue("@RegistroUsuario", data.registrousuario);

                        connection.Open();
                        command.ExecuteNonQuery(); // Usa ExecuteNonQuery para comandos INSERT
                    }
                }

                resp.Code = 0; // �xito
                resp.Description = "Datos insertados correctamente.";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        /// <summary>
        /// Obtiene los documentos
        /// </summary>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public List<DocumentosArchivoDto> Documentos_Obtener(GaDocumento filtros)
        {
            List<DocumentosArchivoDto> resp = new List<DocumentosArchivoDto>();
            try
            {

                List<DocumentosArchivoDto> respGen = null!;

                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    // Build parameterized query
                    string query = "SELECT * FROM GA_Files WHERE Llave_01 = @llave1";
                    var parameters = new DynamicParameters();
                    parameters.Add("@llave1", filtros.llave1, DbType.String);

                    if (filtros.llave2 != null)
                    {
                        query += " AND Llave_02 = @llave2";
                        parameters.Add("@llave2", filtros.llave2, DbType.String);
                    }

                    if (filtros.llave3 != null)
                    {
                        query += " AND Llave_03 = @llave3";
                        parameters.Add("@llave3", filtros.llave3, DbType.String);
                    }

                    respGen = connection.Query<DocumentosArchivoDto>(query, parameters).ToList();
                    resp = respGen;

                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Elimina los documentos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="llave01"></param>
        /// <param name="llave02"></param>
        /// <param name="llave03"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Documentos_Eliminar(int CodEmpresa, string llave01, string llave02, string llave03, string usuario)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                var procedure = "[spGA_EliminarDocumentos]";

                var parameters = new DynamicParameters();
                parameters.Add("llave01", llave01, DbType.String);
                parameters.Add("llave02", llave02.Trim(), DbType.String);
                parameters.Add("llave03", llave03, DbType.String);
                parameters.Add("usuario", usuario, DbType.String);


                using (var connection = new SqlConnection(_config.GetConnectionString(connectionStringName)))
                {
                    resp.Code = connection.Query<int>(procedure, parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

    }

}