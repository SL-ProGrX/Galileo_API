using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;


namespace PgxAPI.DataBaseTier
{
    public class frmFSL_RequisitosDB
    {
        private readonly IConfiguration _config;

        public frmFSL_RequisitosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<fslRequisitosDataLista> FslRequisitos_Obtener(int CodCliente, string filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<fslRequisitosDataLista>();

            response.Result = new fslRequisitosDataLista();

            response.Result.Total = 0;
            try
            {

                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                string vFiltro = "";
                fslRequisitosFiltros filtro = JsonConvert.DeserializeObject<fslRequisitosFiltros>(filtros) ?? new fslRequisitosFiltros();


                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = "select count(*) " +
                        " from FSL_COMITES";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros != null)
                    {
                        vFiltro = " where COD_REQUISITO LIKE '%" + filtro.filtro + "%' OR DESCRIPCION LIKE '%" + filtro.filtro + "%' ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select COD_REQUISITO,DESCRIPCION,ACTIVO,REGISTRO_FECHA, REGISTRO_USUARIO
                             from FSL_REQUISITOS 
                             {vFiltro} 
                            order by COD_REQUISITO
                            {paginaActual}
                            {paginacionActual}; ";


                    response.Result.requisitos = connection.Query<fslRequisitosData>(query).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslRequisitos_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }

        public ErrorDTO<List<fslPanesCausasLista>> FslPlanesCausa_Obtener(int CodCliente, string cod_plan)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<fslPanesCausasLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select rtrim(cod_Causa) as 'item', rtrim(descripcion) as 'descripcion'
		                                  from FSL_Planes_Causas where activa = 1 and cod_plan = '{cod_plan}' ";

                    response.Result = connection.Query<fslPanesCausasLista>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslPlanesCausa_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;

        }

        public ErrorDTO<List<fslRequisitoCausa>> FslRequisitoCausa_Obtener(int CodCliente, string cod_plan, string cod_causa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<fslRequisitoCausa>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Rq.COD_REQUISITO, Rq.DESCRIPCION, isnull(Rc.OPCIONAL,0) as 'Opcional' , RC.COD_CAUSA, RC.COD_PLAN,isnull(Rc.ASIGNADO,0) as 'Asignado'
		                            from FSL_REQUISITOS Rq left join FSL_REQUISITOS_CAUSAS Rc on Rq.COD_REQUISITO = Rc.COD_REQUISITO
		                             Where Rq.ACTIVO = 1 AND RC.COD_CAUSA = '{cod_causa}' and Rc.COD_PLAN = '{cod_plan}'";
                    response.Result = connection.Query<fslRequisitoCausa>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslRequisitoCausa_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;
        }

        public ErrorDTO<List<fslPlanes>> FslPlanes_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<fslPlanes>>(); ;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select rtrim(cod_Plan) as 'item', rtrim(descripcion) as 'descripcion' from FSL_Planes where activo = 1 ";

                    response.Result = connection.Query<fslPlanes>(query).ToList();
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslPlanes_Obtener - " + ex.Message;
                response.Result = null;

            }
            return response;

        }


        public ErrorDTO Requisito_Guardar(int CodCliente, fslRequisitosData requisito)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                if (!ExisteRequisito(CodCliente, requisito.cod_requisito))
                {
                    info = FslRequisito_Insertar(CodCliente, requisito);
                }
                else
                {
                    info = FslRequisito_Actualizar(CodCliente, requisito);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message.ToString();
            }
            return info;
        }

        public ErrorDTO FslRequisito_Insertar(int CodCliente, fslRequisitosData requisito)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;


            ;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    if (ExisteRequisito(CodCliente, requisito.cod_requisito))
                    {
                        info.Code = -1;
                        info.Description = "El requisito ya existe";
                        return info;
                    }
                    else
                    {
                        int activo = requisito.activo ? 1 : 0;
                        var query = $@"insert into FSL_REQUISITOS (COD_REQUISITO, DESCRIPCION, ACTIVO,registro_fecha,registro_usuario) 
                                    values ('{requisito.cod_requisito}', '{requisito.descripcion}', {activo},  getdate() , '{requisito.registro_usuario}')";

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

        public bool ExisteRequisito(int CodCliente, string cod_requistos)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            bool existe = false;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"select isnull(count(*),0) as Existe from FSL_REQUISITOS where  COD_REQUISITO = '{cod_requistos}'";
                    existe = connection.Query<int>(query).FirstOrDefault() > 0;
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                existe = true;
            }
            return existe;
        }

        public ErrorDTO FslRequisito_Actualizar(int CodCliente, fslRequisitosData requisito)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    int activo = requisito.activo ? 1 : 0;
                    var query = $@"update FSL_REQUISITOS set DESCRIPCION = '{requisito.descripcion}', ACTIVO = '{activo}'
                                    where COD_REQUISITO = '{requisito.cod_requisito}'";
                                    
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

        public ErrorDTO FslRequisito_Eliminar(int CodCliente, string cod_requisito)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO();
            info.Code = 0;


            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete from FSL_REQUISITOS where COD_REQUISITO = '{cod_requisito}'";
                   
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


        public ErrorDTO<fslRequisitoEditar> FslAsignacion_Editar(int CodCliente, fslRequisitoEditar asignacion)
        {
            var response = new ErrorDTO<fslRequisitoEditar>();

            response.Result = new fslRequisitoEditar();
            response.Code = 0;
            try
            {
                response = FslRequisito_ActualizarOpcional(CodCliente, asignacion);

                response = FslRequisito_ActualizaAsignado(CodCliente, asignacion);

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslAsignacion_Editar - " + ex.Message;
                response.Result = null;
            }
            return response;

        }


        public ErrorDTO<fslRequisitoEditar> FslRequisito_ActualizarOpcional(int CodCliente, fslRequisitoEditar asignacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<fslRequisitoEditar>();

            response.Result = new fslRequisitoEditar();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    int opcional = asignacion.opcional ? 1 : 0;
                    var query = $@"update FSL_REQUISITOS_CAUSAS set Opcional = {opcional}  where COD_PLAN = '{asignacion.cod_plan}'  and cod_Causa = '{asignacion.cod_causa}'  and cod_requisito = '{asignacion.cod_requisito}' ";
                    var resp = connection.Execute(query);

                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslRequisito_ActualizarOpcional - " + ex.Message;
                response.Result = null;
            }
            return response;
        }

        public ErrorDTO<fslRequisitoEditar> FslRequisito_ActualizaAsignado(int CodCliente, fslRequisitoEditar asignacion)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<fslRequisitoEditar>();

            response.Result = new fslRequisitoEditar();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    int asignado = asignacion.asignado ? 1 : 0;
                    var query = $@"update FSL_REQUISITOS_CAUSAS set Asignado = {asignado}  where COD_PLAN = '{asignacion.cod_plan}'  and cod_Causa = '{asignacion.cod_causa}'  and cod_requisito = '{asignacion.cod_requisito}' ";
                    var resp = connection.Execute(query);
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "FslRequisito_ActualizaAsignado - " + ex.Message;
                response.Result = null;
            }
            return response;
        }


    }
}