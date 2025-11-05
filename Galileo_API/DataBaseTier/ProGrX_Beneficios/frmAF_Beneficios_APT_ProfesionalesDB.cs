using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_APT_ProfesionalesDB
    {
        private readonly IConfiguration _config;

        public frmAF_Beneficios_APT_ProfesionalesDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Metodo para obtener la lista de los profesionales
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<BeneAptProfesionalesDataLista> AfBeneAptPro_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<BeneAptProfesionalesDataLista>();

            response.Result = new BeneAptProfesionalesDataLista();


            AfiAptProFiltros filtro = JsonConvert.DeserializeObject<AfiAptProFiltros>(filtros) ?? new AfiAptProFiltros();

            response.Code = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(ID_PROFESIONAL) from AFI_BENE_APT_PROFESIONALES ";
                    response.Code = connection.Query<int>(query).FirstOrDefault();

                    string vFiltro = "";
                    if (filtro.filtro != null)
                    {
                        vFiltro = " where ID_PROFESIONAL LIKE '%" + filtro.filtro + "%'" +
                            " OR IDENTIFICACION LIKE '%" + filtro.filtro + "%' " +
                             " OR USUARIO LIKE '%" + filtro.filtro + "%' " +
                            " OR NOMBRE LIKE '%" + filtro.filtro + "%' ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT  [ID_PROFESIONAL]
                                          ,[IDENTIFICACION]
                                          ,[NOMBRE]
                                          ,[USUARIO]
                                          ,[ACTIVO]
                                          ,[REGISTRO_FECHA]
                                          ,[REGISTRO_USUARIO]
                                          ,[MODIFICA_FECHA]
                                          ,[MODIFICA_USUARIO]
                                      FROM AFI_BENE_APT_PROFESIONALES 
                                         {vFiltro} 
                                        order by ID_PROFESIONAL
                                        {paginaActual}
                                        {paginacionActual} ";


                    response.Result.lista = connection.Query<BeneAptProfesionalesData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "AfBeneAptPro_Obtener - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Metodo para insertar o actualizar un profesional
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="profesional"></param>
        /// <returns></returns>
        public ErrorDto AfBeneAptPro_Insertar(int CodCliente, BeneAptProfesionalesData profesional)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Valido si existe
                    var query = $@"select isnull(count(*),0) as Existe from AFI_BENE_APT_PROFESIONALES 
                          where ID_PROFESIONAL = '{profesional.id_profesional}' ";
                    var existe = connection.Query<int>(query).FirstOrDefault();

                    if (existe == 0)
                    {
                        int activo = profesional.activo ? 1 : 0;
                        query = $@"INSERT INTO AFI_BENE_APT_PROFESIONALES
                                           (
                                            IDENTIFICACION
                                           ,NOMBRE
                                           ,USUARIO
                                           ,ACTIVO
                                           ,REGISTRO_FECHA
                                           ,REGISTRO_USUARIO
                                           )
                                     VALUES
                                           (
                                           '{profesional.identificacion}'
                                           ,'{profesional.nombre}'
                                           ,'{profesional.usuario}'
                                           ,{activo}
                                           ,getdate()
                                           ,'{profesional.registro_usuario}'
                                            )";
                        info.Code = connection.Execute(query, profesional);
                    }
                    else
                    {
                        info = AfBeneAptPro_Actualizar(CodCliente, profesional);
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        /// <summary>
        /// Metodo para actualizar un profesional
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="profesional"></param>
        /// <returns></returns>
        public ErrorDto AfBeneAptPro_Actualizar(int CodCliente, BeneAptProfesionalesData profesional)
        {


            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int activo = profesional.activo ? 1 : 0;
                    var query = $@"UPDATE AFI_BENE_APT_PROFESIONALES
                                   SET [IDENTIFICACION] = '{profesional.identificacion}'
                                      ,[NOMBRE] = '{profesional.nombre}'
                                      ,[USUARIO] = '{profesional.usuario}'
                                      ,[ACTIVO] = {activo}
                                      ,[MODIFICA_FECHA] = getdate()
                                      ,[MODIFICA_USUARIO] = '{profesional.modifica_usuario}'
                                 WHERE ID_PROFESIONAL = {profesional.id_profesional} ";

                    info.Code = connection.Execute(query, profesional);
                }
            }

            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        /// <summary>
        /// Metodo para eliminar un profesional
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="id_profesional"></param>
        /// <returns></returns>
        public ErrorDto AfBeneAptPro_Eliminar(int CodCliente, int id_profesional)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;



            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"DELETE FROM AFI_BENE_APT_PROFESIONALES WHERE ID_PROFESIONAL = {id_profesional} ";
                    info.Code = connection.Execute(query, new { id_profesional });
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