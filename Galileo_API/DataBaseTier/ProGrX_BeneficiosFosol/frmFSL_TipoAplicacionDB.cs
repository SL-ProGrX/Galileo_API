using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier
{
    public class frmFSL_TipoAplicacionDB
    {
        private readonly IConfiguration _config;

        public frmFSL_TipoAplicacionDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<CausasDataLista> Causas_Obtener(int CodCliente, string TipoCausa, string Jfiltro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            FiltroLazy filtro = JsonConvert.DeserializeObject<FiltroLazy>(Jfiltro);

            var response = new ErrorDto<CausasDataLista>();
            response.Result = new CausasDataLista();
            response.Result.total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";

                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    query = $"SELECT COUNT(*) FROM FSL_PLANES_CAUSAS where COD_PLAN = '{TipoCausa}' ";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro.filtro != "")
                    {
                        filtro.filtro = " AND COD_CAUSA LIKE '%" + filtro.filtro + "%' OR descripcion LIKE '%" + filtro.filtro + "%' ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@" select COD_CAUSA,descripcion
                                       , case when MONTO_BASE = 'F' then 'Formalizado' else 'Saldo' end as 'MontoBase'
                                       , case when TIPO_TABLA = 'F' then 'Fallecimiento' when TIPO_TABLA = 'I' then 'Incapacidad' 
                                              when TIPO_TABLA = 'X' then '100 %' when TIPO_TABLA = 'S' then 'Suicidio' Else 'Fallecimiento' end as 'TipoTabla'
                                       ,Activa
                                        from FSL_PLANES_CAUSAS
                                        where COD_PLAN = '{TipoCausa}' {filtro.filtro} order by COD_CAUSA {paginaActual}
                                        {paginacionActual}";

                    response.Result.causas = connection.Query<TiposCausaData>(query).ToList();

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

        public ErrorDto<List<TiposCausaData>> CausasListas_Exportar(int CodCliente, string TipoCausa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);

            var response = new ErrorDto<List<TiposCausaData>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@" select COD_CAUSA,descripcion
                                       , case when MONTO_BASE = 'F' then 'Formalizado' else 'Saldo' end as 'MontoBase'
                                       , case when TIPO_TABLA = 'F' then 'Fallecimiento' when TIPO_TABLA = 'I' then 'Incapacidad' 
                                              when TIPO_TABLA = 'X' then '100 %' when TIPO_TABLA = 'S' then 'Suicidio' Else 'Fallecimiento' end as 'TipoTabla'
                                       ,Activa
                                        from FSL_PLANES_CAUSAS
                                        where COD_PLAN = '{TipoCausa}' order by COD_CAUSA";

                    response.Result = connection.Query<TiposCausaData>(query).ToList();

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

        public ErrorDto<PlanesDataLista> Planes_Obtener(int CodCliente, string Jfiltro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            FiltroLazy filtro = JsonConvert.DeserializeObject<FiltroLazy>(Jfiltro) ?? new FiltroLazy();

            var response = new ErrorDto<PlanesDataLista>();
            response.Result = new PlanesDataLista();
            response.Result.total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    query = "SELECT COUNT(*) FROM FSL_PLANES ";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro.filtro != null)
                    {
                        filtro.filtro = " WHERE COD_PLAN LIKE '%" + filtro.filtro + "%' OR DESCRIPCION LIKE '%" + filtro.filtro + "%' ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select COD_PLAN,descripcion,case when isnull(Tipo_Desembolso,'F') = 'F' then 'Fondos' 
                                        else 'Tesorería' end as 'TIPO' 
                                                ,Activo
                                                from FSL_PLANES {filtro.filtro} order by COD_PLAN {paginaActual}
                                        {paginacionActual} ";

                    response.Result.planes = connection.Query<ListaPlanesData>(query).ToList();

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

        public ErrorDto<List<ListaPlanesData>> PlanesLista_Exportar(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<ListaPlanesData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select COD_PLAN,descripcion,case when isnull(Tipo_Desembolso,'F') = 'F' then 'Fondos' 
                                        else 'Tesorería' end as 'TIPO' 
                                                ,Activo
                                                from FSL_PLANES order by COD_PLAN ";
                    response.Result = connection.Query<ListaPlanesData>(query).ToList();
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

        private ErrorDto<bool> Plan_Existe(int CodCliente, string cod_plan)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<bool>();
            response.Result = false;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select isnull(count(*),0) as Existe from FSL_PLANES where COD_PLAN = '{cod_plan}' ";

                    var info = connection.Query<string>(query).ToList();
                    if (info.Count > 0)
                    {
                        response.Result = info[0] == "0" ? false : true;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = true;
            }
            return response;
        }  

        public ErrorDto Planes_Insertar(int CodCliente, PlanDataInsert planData)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                var existe = Plan_Existe(CodCliente, planData.cod_plan);

                if(existe.Code == -1)
                {
                    resp.Code = -1;
                    resp.Description = existe.Description;
                    return resp;
                }

                if (existe.Result)
                {
                    resp = Planes_Actualizar(CodCliente, planData);
                }
                else
                {
                    using var connection = new SqlConnection(stringConn);
                    {
                        int activo = planData.activo ? 1 : 0;
                        var query = $@"insert FSL_PLANES (COD_PLAN, Descripcion,Tipo_Desembolso, Activo,registro_fecha,registro_usuario ) 
                                    values ('{planData.cod_plan}', '{planData.descripcion}', '{planData.tipo_desembolso}', '{activo}', getdate() , '{planData.registro_usuario}' )
                                        ";
                        resp.Code = connection.Execute(query);
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

        private ErrorDto Planes_Actualizar(int CodCliente, PlanDataInsert planData)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int activo = planData.activo ? 1 : 0;

                    var query = $@"update FSL_PLANES set Descripcion = '{planData.descripcion}', 
                             Tipo_Desembolso = '{planData.tipo_desembolso}', Activo = '{activo}' where COD_PLAN = '{planData.cod_plan}' ";
                    resp.Code = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto Planes_Eliminar(int CodCliente, string cod_plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete FSL_PLANES where COD_PLAN = '{cod_plan}' ";
                    resp.Code = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private ErrorDto<bool> Causa_Existe(int CodCliente, string cod_causa , string cod_plan)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<bool>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select isnull(count(*),0) as Existe from FSL_PLANES_CAUSAS 
                                    where COD_CAUSA = '{cod_causa}' AND COD_PLAN = '{cod_plan}' ";

                    var info = connection.Query<string>(query).ToList();
                    if (info.Count > 0)
                    {
                        response.Result = info[0] == "0" ? false : true;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = true;
            }
            return response;
        }

        public ErrorDto Causas_Insertar(int CodCliente, CausaDataInsert causaData)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            try
            {

                var existe = Causa_Existe(CodCliente, causaData.cod_causa, causaData.cod_plan);
                if(existe.Code == -1)
                {
                    resp.Code = -1;
                    resp.Description = existe.Description;
                }

                if (existe.Result)
                {
                    resp = Causas_Actualizar(CodCliente, causaData);
                }
                else
                {
                    using var connection = new SqlConnection(stringConn);
                    {
                        int activo = causaData.activa ? 1 : 0;
                        var query = $@"insert FSL_PLANES_CAUSAS (
                                            COD_CAUSA, 
                                            cod_plan, 
                                            Descripcion,
                                            Monto_Base,
                                            Tipo_Tabla,
                                            Activa,
                                            registro_fecha,
                                            registro_usuario
                                            ) values (
                                            '{causaData.cod_causa}', 
                                            '{causaData.cod_plan}', 
                                            '{causaData.descripcion}', 
                                            '{causaData.monto_base}', 
                                            '{causaData.tipo_tabla}', 
                                            '{activo}', getdate(), '{causaData.registro_usuario}') ";
                        resp.Code = connection.Execute(query);
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

        public ErrorDto Causas_Actualizar(int CodCliente, CausaDataInsert causaData)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    int activo = causaData.activa ? 1 : 0;
                    var query = $@"update FSL_PLANES_CAUSAS set descripcion = '{causaData.descripcion}', 
                                        Tipo_Tabla = '{causaData.tipo_tabla}', Monto_Base = '{causaData.monto_base}',
                                       Activa = '{activo}' where COD_CAUSA = '{causaData.cod_causa}' and COD_PLAN = '{causaData.cod_plan}' ";
                    resp.Code = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto Causas_Eliminar(int CodCliente, string cod_causa, string cod_plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete FSL_PLANES_CAUSAS where COD_CAUSA = '{cod_causa}' and COD_PLAN = '{cod_plan}' ";
                    resp.Code = connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<List<ListaPlanesData>> ListaPlanes_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<ListaPlanesData>>();
            response.Result = new List<ListaPlanesData>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select RTRIM(COD_PLAN) cod_plan , RTRIM(COD_PLAN) + ' - ' + descripcion as descripcion FROM FSL_PLANES WHERE ACTIVO = 1";

                    response.Result = connection.Query<ListaPlanesData>(query).ToList();

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

    }
}