using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.FSL;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmFSL_TablasTiposDB
    {
        private readonly IConfiguration _config;

        public frmFSL_TablasTiposDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<fslTablaTipoLista> FslTablaTipos_Obtener(int CodCliente, string tipo, string? filtro, int? pagina, int? paginacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<fslTablaTipoLista>();
            response.Result = new fslTablaTipoLista();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    string query = "", total = "", paginaActual = " ", paginacionActual = " ", vfiltro = " ";

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    switch (tipo)
                    {
                        case "G":
                            if (filtro != null)
                            {
                                vfiltro = " WHERE COD_GESTION LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                            }
                            total = $"SELECT COUNT(*) FROM FSL_TIPOS_GESTIONES {vfiltro}";
                            query = $@"select COD_GESTION as codigo,descripcion,Activa from FSL_TIPOS_GESTIONES {vfiltro} order by COD_GESTION {paginaActual} {paginacionActual}";
                            break;
                        case "A":
                            if (filtro != null)
                            {
                                vfiltro = " WHERE COD_APELACION LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                            }
                            total = $"SELECT COUNT(*) FROM FSL_TIPOS_APELACIONES {vfiltro}";
                            query = $@"select COD_APELACION as codigo,descripcion,Activa from FSL_TIPOS_APELACIONES {vfiltro} order by COD_APELACION {paginaActual} {paginacionActual}";
                            break;
                        case "E":
                            if (filtro != null)
                            {
                                vfiltro = " WHERE COD_ENFERMEDAD LIKE '%" + filtro + "%' OR descripcion LIKE '%" + filtro + "%' ";
                            }
                            total = $"SELECT COUNT(*) FROM FSL_TIPOS_ENFERMEDADES {vfiltro}";
                            query = $@"select COD_ENFERMEDAD as codigo, descripcion,Activa from FSL_TIPOS_ENFERMEDADES {vfiltro} order by COD_ENFERMEDAD {paginaActual} {paginacionActual}";
                            break;
                        default:
                            break;
                    }
                    response.Result.Total = connection.Query<int>(total).FirstOrDefault();
                    response.Result.Lista = connection.Query<fslTablaTipoData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Lista = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDTO FslTablaTipos_Actualizar(int CodCliente, string tipo, fslTablaTipoData tipoData)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    string query = "";
                    int activo = tipoData.activa ? 1 : 0;
                    switch (tipo)
                    {
                        case "G":
                            query = $@"update FSL_TIPOS_GESTIONES set descripcion = '{tipoData.descripcion}', Activa = {activo} where COD_GESTION = '{tipoData.codigo}'";
                            break;
                        case "A":
                            query = $@"update FSL_TIPOS_APELACIONES set descripcion = '{tipoData.descripcion}', Activa = {activo} where COD_APELACION = '{tipoData.codigo}'";
                            break;
                        case "E":
                            query = $@"update FSL_TIPOS_ENFERMEDADES set descripcion = '{tipoData.descripcion}' , Activa = {activo} where COD_ENFERMEDAD = '{tipoData.codigo}'";
                            break;
                        default:
                            break;
                    }
                    var result = connection.Execute(query);
                    resp.Description = "Registro actualizado satisfactoriamente!";

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDTO FslTablaTipo_Insertar(int CodCliente, string tipo, string usuario, fslTablaTipoData tipoData)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    string query = "";
                    int activo = tipoData.activa ? 1 : 0;

                    if (FslTablaTipoExiste(CodCliente, tipo, tipoData.codigo))
                    {
                        resp = FslTablaTipos_Actualizar(CodCliente, tipo, tipoData);
                    }
                    else
                    {
                        switch (tipo)
                        {
                            case "G":
                                query = $@"insert into FSL_TIPOS_GESTIONES (COD_GESTION,descripcion,Activa,registro_fecha,registro_usuario) 
                                            values ('{tipoData.codigo}','{tipoData.descripcion}',{activo},  getdate(), '{usuario}' )";
                                break;
                            case "A":
                                query = $@"insert into FSL_TIPOS_APELACIONES (COD_APELACION,descripcion,Activa,registro_fecha,registro_usuario) 
                                            values ('{tipoData.codigo}','{tipoData.descripcion}',{activo} ,  getdate(), '{usuario}')";
                                break;
                            case "E":
                                query = $@"insert into FSL_TIPOS_ENFERMEDADES (COD_ENFERMEDAD,descripcion,Activa,registro_fecha,registro_usuario) 
                                            values ('{tipoData.codigo}','{tipoData.descripcion}',{activo} ,  getdate(), '{usuario}')";
                                break;
                            default:
                                break;
                        }

                        var result = connection.Execute(query);

                        resp.Description = "Registro agregado satisfactoriamente!";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private bool FslTablaTipoExiste(int CodCliente, string tipo, string codigo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            bool resp = false;
            try
            {
                string query = "";
                using var connection = new SqlConnection(clienteConnString);
                {

                    switch (tipo)
                    {
                        case "G":
                            query = $@"select isnull(count(*),0) as Existe from FSL_TIPOS_GESTIONES  where COD_GESTION = '{codigo}' ";
                            break;
                        case "A":
                            query = $@"select isnull(count(*),0) as Existe from FSL_TIPOS_APELACIONES  where COD_APELACION = '{codigo}' ";
                            break;
                        case "E":
                            query = $@"select isnull(count(*),0) as Existe from FSL_TIPOS_ENFERMEDADES  where COD_ENFERMEDAD = '{codigo}' ";
                            break;
                        default:
                            break;
                    }
                    var info = connection.Query<string>(query).ToList();
                    if (info.Count > 0)
                    {
                        resp = info[0] == "0" ? false : true;
                    }
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                resp = true;
            }
            return resp;
        }

        public ErrorDTO FslTablaTipo_Eliminar(int CodCliente, string tipo, string codigo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO error = new ErrorDTO();
            error.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    string query = "";
                    switch (tipo)
                    {
                        case "G":
                            query = $@"delete from FSL_TIPOS_GESTIONES where COD_GESTION = '{codigo}'";
                            break;
                        case "A":
                            query = $@"delete from FSL_TIPOS_APELACIONES where COD_APELACION = '{codigo}'";
                            break;
                        case "E":
                            query = $@"delete from FSL_TIPOS_ENFERMEDADES where COD_ENFERMEDAD = '{codigo}'";
                            break;
                        default:
                            break;
                    }
                    var result = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                error.Code = -1;
                error.Description = ex.Message;
            }
            return error;
        }
    }
}