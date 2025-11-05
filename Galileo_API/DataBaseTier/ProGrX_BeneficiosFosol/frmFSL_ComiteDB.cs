using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;

namespace PgxAPI.DataBaseTier
{
    public class frmFSL_ComiteDB
    {
        private readonly IConfiguration _config;

        public frmFSL_ComiteDB(IConfiguration config)
        {
            _config = config;
        }
        //Comites
        public ErrorDTO<FslComitesDataLista> FslComites_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<FslComitesDataLista>();

            response.Result = new FslComitesDataLista();

            response.Result.Total = 0;
            try
            {

                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                string vFiltro = "";
                FslComitefiltros filtro = JsonConvert.DeserializeObject<FslComitefiltros>(filtros) ?? new FslComitefiltros();


                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "select count(*) " +
                        " from FSL_COMITES";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros != null)
                    {
                        vFiltro = " where COD_COMITE LIKE '%" + filtro.filtro + "%' OR descripcion LIKE '%" + filtro.filtro + "%' ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select COD_COMITE,DESCRIPCION,NUMERO_RESOLUTORES,ACTIVO
                                         from FSL_COMITES 
                                         {vFiltro} 
                                        order by COD_COMITE
                                        {paginaActual}
                                        {paginacionActual}; ";


                    response.Result.Comites = connection.Query<FslComitesDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslComites_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;


        }

        public ErrorDTO<List<FslComitesActivosData>> FslComitesActivos_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<FslComitesActivosData>>();

            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = "select COD_COMITE as item,  RTRIM(COD_COMITE) + ' - ' + DESCRIPCION as descripcion FROM FSL_COMITES WHERE ACTIVO = 1";
                    response.Result = connection.Query<FslComitesActivosData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslComitesActivos_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }

        public ErrorDTO Comite_Guardar(int CodCliente, FslComitesDTO comite)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                if (!Comite_Existe(CodCliente, comite.cod_comite))
                {
                    info = FslComites_Insertar(CodCliente, comite);
                }
                else
                {
                    info = FslComites_Actualizar(CodCliente, comite);

                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info; ;
        }

        public ErrorDTO FslComites_Insertar(int CodCliente, FslComitesDTO comite)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {

                if (Comite_Existe(CodCliente, comite.cod_comite))
                {
                    info.Code = -1;
                    info.Description = "Comite ya existe";
                }
                else
                {
                    using var connection = new SqlConnection(clienteConnString);
                    {
                        int activo = comite.activo ? 1 : 0;
                        var query = $@"insert FSL_COMITES (COD_COMITE ,Descripcion, Numero_Resolutores, Activo,registro_fecha,registro_usuario ) 
                                    values ('{comite.cod_comite}', '{comite.descripcion}', '{comite.numero_resolutores}', {activo}, getdate() , '{comite.registro_usuario}' ) ";

                        info.Code = connection.Execute(query);
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

        private bool Comite_Existe(int CodCliente, string cod_comite)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            bool existe = false;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"select count(*) from FSL_COMITES where COD_COMITE = '{cod_comite}'";
                    existe = connection.Query<int>(query).FirstOrDefault() > 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return existe;
        }

        public ErrorDTO FslComites_Actualizar(int CodCliente, FslComitesDTO comite)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int activo = comite.activo ? 1 : 0;
                    var query = $@"update FSL_COMITES set Descripcion = '{comite.descripcion}', Numero_Resolutores = '{comite.numero_resolutores}', ACTIVO = {activo} 
                                        where COD_COMITE = '{comite.cod_comite}' ";
                    info.Code = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        public ErrorDTO FslComites_Eliminar(int CodCliente, string comite)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete from FSL_COMITES where COD_COMITE = '{comite}' ";
                    var result = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }




        //Miembros Comite

        public ErrorDTO<FslMiembrosComitesDataLista> FslMiembrosComite_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<FslMiembrosComitesDataLista>();

            response.Result = new FslMiembrosComitesDataLista();

            response.Result.Total = 0;
            try
            {

                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                string vFiltro = "";
                FslComitefiltros filtro = JsonConvert.DeserializeObject<FslComitefiltros>(filtros);


                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "select count(*) " +
                        " from FSL_COMITES_MIEMBROS";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros != null)
                    {
                        vFiltro = "WHERE COD_COMITE = '" + filtro.comiteSeleccionado + "' ";
                        vFiltro += "AND (COD_COMITE LIKE '%" + filtro.filtro + "%' OR NOMBRE LIKE '%" + filtro.filtro + "%')";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select CEDULA, COD_COMITE,NOMBRE,USUARIO_VINCULADO,REGISTRO_FECHA,REGISTRO_USUARIO,SALIDA_FECHA,ACTIVO
                                         from FSL_COMITES_MIEMBROS 
                                         {vFiltro} 
                                        order by COD_COMITE
                                        {paginaActual}
                                        {paginacionActual}; ";


                    response.Result.Miembros = connection.Query<FslMiembrosComitesDTO>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslMiembrosComite_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;

        }
        public ErrorDTO ComiteMiembro_Guardar(int CodCliente, FslMiembrosComitesDTO miembro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                if (!MiembroComite_Existe(CodCliente, miembro.cod_comite, miembro.cedula))
                {
                    info = FslMiembrosComite_Insertar(CodCliente, miembro);
                }
                else
                {
                    info = FslMiembrosComite_Actualizar(CodCliente, miembro);
                }
            }

            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;


        }

        public ErrorDTO FslMiembrosComite_Insertar(int CodCliente, FslMiembrosComitesDTO miembro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0; ;
            try
            {

                if (MiembroComite_Existe(CodCliente, miembro.cedula, miembro.cod_comite))
                {
                    info.Description = "Miebro ya existe en este comite";
                }
                else
                {
                    using var connection = new SqlConnection(clienteConnString);
                    {
                        int activo = miembro.activo ? 1 : 0;
                        var query = $@"insert FSL_COMITES_MIEMBROS (CEDULA,COD_COMITE, Nombre, USUARIO_VINCULADO,Activo,registro_fecha,registro_usuario) 
                                    values ('{miembro.cedula}', '{miembro.cod_comite}', '{miembro.nombre}', '{miembro.usuario_Vinculado}', '{activo}', getdate() , '{miembro.registro_Usuario}' ) ";
                        var result = connection.Execute(query);
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

        public ErrorDTO FslMiembrosComite_Actualizar(int CodCliente, FslMiembrosComitesDTO miembro)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int activo = miembro.activo ? 1 : 0;
                    string salidaWhere = "";
                    if (activo == 0)
                    {
                        salidaWhere = $", Salida_Fecha = getdate(), Salida_Usuario = '{miembro.salida_usuario}' ";
                    }
                    else
                    {
                        salidaWhere = $", Salida_Fecha = null, Salida_Usuario = null ";
                    }

                    var query = $@"update FSL_COMITES_MIEMBROS set Nombre = '{miembro.nombre}', USUARIO_VINCULADO = '{miembro.usuario_Vinculado}', Activo = {activo} {salidaWhere} 
                                        where COD_COMITE = '{miembro.cod_comite}' and CEDULA = '{miembro.cedula}' ";
                    var result = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;

        }

        private bool MiembroComite_Existe(int CodCliente, string cod_comite, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            bool existe = false;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"select count(*) from FSL_COMITES_MIEMBROS where COD_COMITE = '{cod_comite}' and CEDULA = '{cedula}' ";
                    existe = connection.Query<int>(query).FirstOrDefault() > 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return existe;
        }

        public ErrorDTO FslMiembrosComite_Eliminar(int CodCliente, string cedula, string comite)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete from FSL_COMITES_MIEMBROS where COD_COMITE = '{comite}' and CEDULA = '{cedula}' ";
                    var result = connection.Execute(query);
                }
            }

            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;

            ;
        }
    }
}