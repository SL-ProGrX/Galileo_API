using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using System.Data;
using System.Data.SqlTypes;

namespace PgxAPI.DataBaseTier
{
    public class mImagenes
    {
        private readonly IConfiguration _config;
        public mImagenes(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Guarda una imagen en la base de datos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="sql"></param>
        /// <param name="Campo_Imagen"></param>
        /// <param name="Path_Imagen"></param>
        /// <returns></returns>
        public bool fxImagen_Guardar(int codEmpresa, string sql,string Campo_Imagen, string Path_Imagen)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            bool resp = false;

            if (string.IsNullOrEmpty(Path_Imagen) || !File.Exists(Path_Imagen))
                return false;

            try
            {
                byte[] imagenBytes = File.ReadAllBytes(Path_Imagen);

                using var connection = new SqlConnection(stringConn);
                using var adapter = new SqlDataAdapter(sql, connection);
                var builder = new SqlCommandBuilder(adapter);

                var dataSet = new DataSet();
                adapter.Fill(dataSet);

                if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
                {
                    var row = dataSet.Tables[0].Rows[0];
                    row[Campo_Imagen] = imagenBytes;

                    adapter.UpdateCommand = builder.GetUpdateCommand();
                    adapter.Update(dataSet); // Aquí se guarda el cambio en la base de datos
                    resp = true;
                }
            }
            catch (Exception)
            {
                resp = false;
            }


            return resp;
        }

        /// <summary>
        /// Lee una imagen de la base de datos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="sqlSelect"></param>
        /// <param name="campoImagen"></param>
        /// <returns></returns>
        public byte[]? fxImagen_Leer(int codEmpresa, string sqlSelect, string campoImagen)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                using var command = new SqlCommand(sqlSelect, connection);
                connection.Open();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal(campoImagen)))
                    {
                        return (byte[])reader[campoImagen];
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer la imagen: {ex.Message}");
            }

            return null;
        }

        
    }
}
