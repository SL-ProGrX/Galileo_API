using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneficioRolesDB
    {
        private readonly IConfiguration _config;

        public frmAF_BeneficioRolesDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene la lista de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<BeneficioGrupoDataLista> BeneficioGrupoLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<BeneficioGrupoDataLista>();
            response.Result = new BeneficioGrupoDataLista();
            response.Code = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "SELECT COUNT(*) from AFI_BENEFICIO_GRUPOS ";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " where cod_grupo LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"select cod_grupo,descripcion from AFI_BENEFICIO_GRUPOS 
                                         {filtro} 
                                        order by cod_grupo
                                        {paginaActual}
                                        {paginacionActual} ";

                    response.Result.beneficios = connection.Query<BeneficioGrupoData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Total = 0;
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener la lista de usuarios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <param name="cod_grupo"></param>
        /// <returns></returns>
        public ErrorDto<BeneficioUsuariosDataLista> BeneficioUsuariosLista_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, string cod_grupo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<BeneficioUsuariosDataLista>();
            response.Result = new BeneficioUsuariosDataLista();
            response.Code = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = $"select COUNT(*) from Usuarios U left join AFI_BENE_USERG A on U.nombre = A.usuario and A.cod_grupo = '{cod_grupo}' Where U.estado = 'A' ";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND nombre LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"select U.nombre,U.descripcion,A.usuario, case when A.usuario is null then 0 else 1 end as activo
                                 from Usuarios U left join AFI_BENE_USERG A on U.nombre = A.usuario
                                         and A.cod_grupo = '{cod_grupo}' 
                                 Where U.estado = 'A' 
                                         {filtro} 
                                        order by A.usuario desc,U.nombre asc
                                        {paginaActual}
                                        {paginacionActual} ";

                    response.Result.usuarios = connection.Query<BeneficioUsuariosData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Total = 0;
            }
            return response;

        }

        /// <summary>
        /// Metodo para insertar un grupo de usuarios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_grupo"></param>
        /// <returns></returns>
        public ErrorDto GrupoUsuario_Insertar(int CodCliente, string usuario, string cod_grupo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"insert AFI_BENE_USERG(usuario,cod_grupo) values(
                                     '{usuario.Trim()}', '{cod_grupo.Trim()}') ";
                    var result = connection.Execute(query);
                }

                info.Description = "Grupo Insertado!";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;

        }

        /// <summary>
        /// Metodo para eliminar un grupo de usuarios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="cod_grupo"></param>
        /// <returns></returns>
        public ErrorDto GrupoUsuario_Eliminar(int CodCliente, string usuario, string cod_grupo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete from AFI_BENE_USERG where usuario = '{usuario}' and cod_grupo = '{cod_grupo}' ";
                    var result = connection.Execute(query);
                }

                info.Description = "Grupo Eliminado!";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;

        }

        /// <summary>
        /// Metodo para insertar un grupo de usuarios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_grupo"></param>
        /// <param name="descripcion"></param>
        /// <returns></returns>
        private ErrorDto BeneficioGrupo_Insertar(int CodCliente, string cod_grupo, string descripcion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {

                if (!BeneficioGrupo_Existe(CodCliente, cod_grupo))
                {
                    using var connection = new SqlConnection(clienteConnString);
                    {

                        var query = $@"insert AFI_BENEFICIO_GRUPOS(cod_grupo,descripcion) values(
                                     '{cod_grupo}', '{descripcion}') ";
                        var result = connection.Execute(query);
                    }

                    info.Description = "Grupo Insertado!";
                }
                else
                {
                    info.Code = -1;
                    info.Description = "Grupo ya Existe!";
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;

        }

        /// <summary>
        /// Metodo para actualizar un grupo de usuarios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_grupo"></param>
        /// <param name="descripcion"></param>
        /// <returns></returns>
        private ErrorDto BeneficioGrupo_Actualizar(int CodCliente, string cod_grupo, string descripcion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            ErrorDto info = new ErrorDto();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"update AFI_BENEFICIO_GRUPOS set descripcion = '{descripcion}' where cod_grupo = '{cod_grupo}' ";
                    var result = connection.Execute(query);
                }

                info.Description = "Grupo Actualizado!";
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }
            return info;
        }

        /// <summary>
        /// Metodo para verificar si existe un grupo de usuarios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_grupo"></param>
        /// <returns></returns>
        private bool BeneficioGrupo_Existe(int CodCliente, string cod_grupo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            bool existe = false;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT COUNT(*) from AFI_BENEFICIO_GRUPOS where cod_grupo = '{cod_grupo}' ";
                    existe = connection.Query<int>(query).FirstOrDefault() > 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                existe = false;
            }
            return existe;
        }

        /// <summary>
        /// Metodo para obtener la lista de grupos de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<BeneficioGrupoData>> BeneficioGrupoData_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<BeneficioGrupoData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cod_grupo,descripcion from AFI_BENEFICIO_GRUPOS order by cod_grupo ";
                    response.Result = connection.Query<BeneficioGrupoData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Metodo para guardar un grupo de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="grupo"></param>
        /// <returns></returns>
        public ErrorDto BeneficioGrupo_Guardar(int CodCliente, BeneficioGrupoData grupo)
        {
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            if (!BeneficioGrupo_Existe(CodCliente, grupo.cod_grupo))
            {
                info = BeneficioGrupo_Insertar(CodCliente, grupo.cod_grupo, grupo.descripcion);
            }
            else
            {
                info = BeneficioGrupo_Actualizar(CodCliente, grupo.cod_grupo, grupo.descripcion);
            }
            return info;
        }
    }
}