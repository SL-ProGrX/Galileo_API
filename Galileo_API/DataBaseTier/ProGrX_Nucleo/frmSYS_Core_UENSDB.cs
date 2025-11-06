using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.SYS;

namespace PgxAPI.DataBaseTier
{
    public class frmSYS_Core_UENSDB
    {
        private readonly IConfiguration _config;

        public frmSYS_Core_UENSDB(IConfiguration config)
        {
            _config = config;
        }
        /// <summary>
        /// Obtener UENs
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<CoreUeNsDtoList> Core_UENS_Obtener(int CodCliente, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<CoreUeNsFiltros>(filtros);
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<CoreUeNsDtoList>();
            response.Result = new CoreUeNsDtoList();
            response.Code = 0;
            try
            {
                var query = "";
                string where = " ", paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {
                    if (vfiltro != null)
                    {
                        if (vfiltro.filtro != null)
                        {
                            where = "where COD_UNIDAD LIKE '%" + vfiltro.filtro + "%' OR descripcion LIKE '%" + vfiltro.filtro + "%' ";
                        }

                        if (vfiltro.pagina != null)
                        {
                            paginaActual = " OFFSET " + vfiltro.pagina + " ROWS ";
                            paginacionActual = " FETCH NEXT " + vfiltro.paginacion + " ROWS ONLY ";
                        }
                    }
                    query = $"select COUNT(*) from CORE_UENS {where}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = @$"select COD_UNIDAD, descripcion, CntX_Unidad, CntX_Centro_Costo, Activa, 0 as 'btn' 
                        from CORE_UENS {where} order by COD_UNIDAD desc {paginaActual} {paginacionActual}";
                    response.Result.uens = connection.Query<CoreUeNsDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.uens = null;
                response.Result.Total = 0;
            }
            return response;
        }

        /// <summary>
        /// Insertar y Actualizar Core UENs
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Core_UENS_Upsert(int CodCliente, string usuario, CoreUeNsDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new()
            {
                Code = 0
            };
            var activo = 0;
            try
            {
                if (request.activa == true)
                {
                    activo = 1;
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"select isnull(count(*),0) as Existe from CORE_UENS where COD_UNIDAD = '{request.cod_unidad}'";
                    int Existe = connection.Query<int>(query).FirstOrDefault();

                    if (Existe == 0)
                    {
                        query = @"SELECT CAST(MAX(CAST(COD_UNIDAD AS INT)) + 1 AS VARCHAR) AS NuevoCodigo
                        FROM CORE_UENS WHERE ISNUMERIC(COD_UNIDAD) = 1";
                        int ultimoID = connection.Query<int>(query).FirstOrDefault();
                        string nuevoCodigo = ultimoID < 10 ? "0" + ultimoID.ToString() : ultimoID.ToString();

                        query = @$"insert 
                            into CORE_UENS(COD_UNIDAD, descripcion, Activa, Registro_Fecha, Registro_Usuario) 
                            values('{nuevoCodigo}','{request.descripcion}', {activo}, Getdate(), '{usuario}' )";
                        resp.Description = "Registro agregado satisfactoriamente";
                    }
                    else
                    {
                        query = @$"update CORE_UENS set descripcion = '{request.descripcion}',
                            Activa = {activo}, Modifica_Fecha = Getdate(), Modifica_Usuario = '{usuario}'
                            where COD_UNIDAD = '{request.cod_unidad}' OR UNIDAD_PRINCIPAL = '{request.cod_unidad}'";
                        resp.Description = "Registro actualizado satisfactoriamente";
                    }
                    resp.Code = connection.ExecuteAsync(query).Result;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Insertar y Actualizar una unidad de perteneciente a una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="unidad_anterior"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Core_SubUnidad_Upsert(int CodCliente, string usuario, string? unidad_anterior, CoreUeNsDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new()
            {
                Code = 0
            };
            var activo = 0;
            try
            {
                if (request.activa == true)
                {
                    activo = 1;
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Se obtiene información de la unidad principal
                    var query = @$"select * from CORE_UENS where COD_UNIDAD = '{request.unidad_principal}'";
                    CoreUeNsDto unidadPrincipal = connection.Query<CoreUeNsDto>(query).First();

                    query = @$"select isnull(count(*),0) as Existe from CORE_UENS 
                    where (COD_UNIDAD = '{request.unidad_principal}' OR UNIDAD_PRINCIPAL = '{request.unidad_principal}') AND CNTX_UNIDAD = '{request.cntx_unidad}'";
                    int Existe = connection.Query<int>(query).FirstOrDefault();

                    if (Existe == 0 && unidadPrincipal.cntx_unidad == "")
                    {
                        //Asigna la unidad a la UEN principal, porque no tiene unidad
                        query = @$"update CORE_UENS set 
                            CntX_Unidad = '{request.cntx_unidad}', 
                            Activa = {activo}, Modifica_Fecha = Getdate(), Modifica_Usuario = '{usuario}'
                            where COD_UNIDAD = '{unidadPrincipal.cod_unidad}'";
                        resp.Description = "Registro actualizado satisfactoriamente";
                    }
                    else if (Existe == 0 && request.cod_unidad == "")
                    {
                        //Agrega una nueva unidad
                        query = @"SELECT CAST(MAX(CAST(COD_UNIDAD AS INT)) + 1 AS VARCHAR) AS NuevoCodigo
                        FROM CORE_UENS WHERE ISNUMERIC(COD_UNIDAD) = 1";
                        int ultimoID = connection.Query<int>(query).FirstOrDefault();
                        string nuevoCodigo = ultimoID < 10 ? "0" + ultimoID.ToString() : ultimoID.ToString();

                        query = @$"insert into CORE_UENS(COD_UNIDAD, descripcion, CntX_Unidad, unidad_principal, Activa, Registro_Fecha, Registro_Usuario) 
                        values('{nuevoCodigo}','{unidadPrincipal.descripcion}','{request.cntx_unidad}', '{request.unidad_principal}', {activo}, Getdate(), '{usuario}' )";

                        resp.Description = "Registro agregado satisfactoriamente";
                    }
                    else
                    {
                        //Actualiza la unidad
                        query = @$"update CORE_UENS set 
                            CntX_Unidad = '{request.cntx_unidad}', 
                            Activa = {activo}, Modifica_Fecha = Getdate(), Modifica_Usuario = '{usuario}'
                            where (COD_UNIDAD = '{request.cod_unidad}' OR UNIDAD_PRINCIPAL = '{request.cod_unidad}')
                            AND CNTX_UNIDAD = '{unidad_anterior}'";
                        resp.Description = "Registro actualizado satisfactoriamente";
                    }
                    resp.Code = connection.ExecuteAsync(query).Result;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Insertar y Actualizar un centro de costo de perteneciente a una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Core_SubCentroCosto_Upsert(int CodCliente, string usuario, CoreUeNsDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new()
            {
                Code = 0
            };
            var activo = 0;
            try
            {
                if (request.activa == true)
                {
                    activo = 1;
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"select isnull(count(*),0) as Existe from CORE_UENS where COD_UNIDAD = '{request.cod_unidad}'";
                    int Existe = connection.Query<int>(query).FirstOrDefault();

                    if (Existe == 0)
                    {
                        //Se obtiene informaci�n de la unidad principal
                        var query2 = @$"select * from CORE_UENS where 
                            (COD_UNIDAD = '{request.unidad_principal}' OR UNIDAD_PRINCIPAL = '{request.unidad_principal}') 
                            AND CNTX_UNIDAD = '{request.cntx_unidad}'";
                        CoreUeNsDto unidadPrincipal = connection.Query<CoreUeNsDto>(query2).First();

                        if (unidadPrincipal.cntx_centro_costo == "")
                        {
                            query = @$"update CORE_UENS set CntX_Centro_Costo = '{request.cntx_centro_costo}', 
                                Activa = {activo}, Modifica_Fecha = Getdate(), Modifica_Usuario = '{usuario}'
                                where COD_UNIDAD = '{unidadPrincipal.cod_unidad}'";
                        }
                        else
                        {
                            query = @"SELECT CAST(MAX(CAST(COD_UNIDAD AS INT)) + 1 AS VARCHAR) AS NuevoCodigo
                            FROM CORE_UENS WHERE ISNUMERIC(COD_UNIDAD) = 1";
                            int ultimoID = connection.Query<int>(query).FirstOrDefault();
                            string nuevoCodigo = ultimoID < 10 ? "0" + ultimoID.ToString() : ultimoID.ToString();

                            query = @$"insert 
                                into CORE_UENS(COD_UNIDAD, descripcion, CntX_Unidad, CntX_Centro_Costo, unidad_principal, Activa, Registro_Fecha, Registro_Usuario) 
                                values('{nuevoCodigo}','{unidadPrincipal.descripcion}','{request.cntx_unidad}', '{request.cntx_centro_costo}',
                                   '{request.unidad_principal}', {activo}, Getdate(), '{usuario}' )";
                        }
                        resp.Description = "Registro agregado satisfactoriamente";
                    }
                    else
                    {
                        query = @$"update CORE_UENS set 
                            CntX_Centro_Costo = '{request.cntx_centro_costo}',
                            Activa = {activo}, Modifica_Fecha = Getdate(), Modifica_Usuario = '{usuario}'
                            where COD_UNIDAD = '{request.cod_unidad}'";
                        resp.Description = "Registro actualizado satisfactoriamente";
                    }
                    resp.Code = connection.ExecuteAsync(query).Result;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Borra las UEns
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto Core_UENS_Delete(int CodCliente, string cod_unidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"delete from CORE_UENS where COD_UNIDAD = '{cod_unidad}' OR UNIDAD_PRINCIPAL = '{cod_unidad}'";
                    resp.Code = connection.ExecuteAsync(query).Result;
                    resp.Description = "Registros eliminados satisfactoriamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Borrar las unidades de una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <param name="cntx_unidad"></param>
        /// <returns></returns>
        public ErrorDto Core_SubUnidad_Delete(int CodCliente, string cod_unidad, string cntx_unidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"select COD_UNIDAD from CORE_UENS where UNIDAD_PRINCIPAL = '{cod_unidad}' AND CNTX_UNIDAD = '{cntx_unidad}'";
                    string codUnidad = connection.Query<string>(query).FirstOrDefault();

                    //Valida si es la unidad principal
                    if (codUnidad == null)
                    {
                        //Es la UEN Principal
                        query = $"delete from CORE_UENS_USUARIOS_ROLES where COD_UNIDAD = '{cod_unidad}'";
                        resp.Code = connection.ExecuteAsync(query).Result;

                        query = $"delete from CORE_UENS where COD_UNIDAD = '{cod_unidad}'";
                        resp.Code = connection.ExecuteAsync(query).Result;


                        //Valida si existen otras unidades asociadas a la UEN Principal
                        query = $"select TOP 1 COD_UNIDAD from CORE_UENS where UNIDAD_PRINCIPAL = '{cod_unidad}' order by ACTIVA desc";
                        string nuevaUnidadPrincipal = connection.Query<string>(query).FirstOrDefault();

                        if (nuevaUnidadPrincipal != null)
                        {
                            query = @$"update CORE_UENS set 
                            UNIDAD_PRINCIPAL = '{nuevaUnidadPrincipal}' 
                            where UNIDAD_PRINCIPAL = '{cod_unidad}'";
                            resp.Code = connection.ExecuteAsync(query).Result;

                            query = @$"update CORE_UENS set 
                            UNIDAD_PRINCIPAL = NULL 
                            where COD_UNIDAD = '{nuevaUnidadPrincipal}'";
                            resp.Code = connection.ExecuteAsync(query).Result;
                        }
                    }
                    else
                    {
                        //No es la UEN Principal
                        query = $"delete from CORE_UENS_USUARIOS_ROLES where COD_UNIDAD = '{codUnidad}'";
                        resp.Code = connection.ExecuteAsync(query).Result;

                        query = $"delete from CORE_UENS where COD_UNIDAD = '{codUnidad}'";
                        resp.Code = connection.ExecuteAsync(query).Result;
                    }



                    resp.Description = "Registros eliminado satisfactoriamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Borrar el centro de costo de una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto Core_SubCentroCosto_Delete(int CodCliente, string cod_unidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $"select * from CORE_UENS where COD_UNIDAD = '{cod_unidad}'";
                    CoreUeNsDto unidadInfo = connection.Query<CoreUeNsDto>(query).First();

                    //Valida si es la unidad principal
                    if (unidadInfo.unidad_principal == "")
                    {
                        //Es la UEN Principal
                        query = $"delete from CORE_UENS_USUARIOS_ROLES where COD_UNIDAD = '{cod_unidad}'";
                        resp.Code = connection.ExecuteAsync(query).Result;

                        query = $"delete from CORE_UENS where COD_UNIDAD = '{cod_unidad}'";
                        resp.Code = connection.ExecuteAsync(query).Result;

                        //Valida si existen otras unidades asociadas a la UEN Principal
                        query = $"select TOP 1 COD_UNIDAD from CORE_UENS where UNIDAD_PRINCIPAL = '{cod_unidad}' order by ACTIVA desc";
                        string nuevaUnidadPrincipal = connection.Query<string>(query).FirstOrDefault();
                        if (nuevaUnidadPrincipal != null)
                        {
                            query = @$"update CORE_UENS set 
                            UNIDAD_PRINCIPAL = '{nuevaUnidadPrincipal}' 
                            where UNIDAD_PRINCIPAL = '{cod_unidad}'";
                            resp.Code = connection.ExecuteAsync(query).Result;

                            query = @$"update CORE_UENS set 
                            UNIDAD_PRINCIPAL = NULL 
                            where COD_UNIDAD = '{nuevaUnidadPrincipal}'";
                            resp.Code = connection.ExecuteAsync(query).Result;
                        }
                    }
                    else
                    {
                        //No es la UEN Principal
                        query = $"delete from CORE_UENS_USUARIOS_ROLES where COD_UNIDAD = '{cod_unidad}'";
                        resp.Code = connection.ExecuteAsync(query).Result;

                        query = $"delete from CORE_UENS where COD_UNIDAD = '{cod_unidad}'";
                        resp.Code = connection.ExecuteAsync(query).Result;
                        resp.Description = "Registro eliminado satisfactoriamente";
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

        /// <summary>
        /// Obtener UENs principales
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<CoreUeNsDtoList> Core_UENSPrincipales_Obtener(int CodCliente, string filtros)
        {
            var vfiltro = JsonConvert.DeserializeObject<CoreUeNsFiltros>(filtros);
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<CoreUeNsDtoList>();
            response.Result = new CoreUeNsDtoList();
            response.Code = 0;
            try
            {
                var query = "";
                string where = " ", paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {
                    if (vfiltro.filtro != null)
                    {
                        where = "and (COD_UNIDAD LIKE '%" + vfiltro.filtro + "%' OR descripcion LIKE '%" + vfiltro.filtro + "%') ";
                    }

                    if (vfiltro.pagina != null)
                    {
                        paginaActual = " OFFSET " + vfiltro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + vfiltro.paginacion + " ROWS ONLY ";
                    }

                    query = $"select COUNT(*) from CORE_UENS WHERE UNIDAD_PRINCIPAL IS NULL {where}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = @$"select COD_UNIDAD, descripcion, CntX_Unidad, CntX_Centro_Costo, Activa, 0 as 'btn' 
                        from CORE_UENS WHERE UNIDAD_PRINCIPAL IS NULL {where} order by COD_UNIDAD desc {paginaActual} {paginacionActual}";
                    response.Result.uens = connection.Query<CoreUeNsDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.uens = null;
                response.Result.Total = 0;
            }
            return response;
        }

        /// <summary>
        /// Obtener las unidades pertenecientes a una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<CoreUeNsDtoList> Core_SubUnidades_Obtener(int CodCliente, string cod_unidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<CoreUeNsDtoList>();
            response.Result = new CoreUeNsDtoList();
            response.Code = 0;
            response.Result.Total = 0;
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = @$"select DISTINCT '{cod_unidad}' AS COD_UNIDAD, C.CNTX_UNIDAD, '{cod_unidad}' as UNIDAD_PRINCIPAL,
                    (select TOP 1 DESCRIPCION from CNTX_UNIDADES WHERE COD_UNIDAD = C.CNTX_UNIDAD) AS DESCRIPCION
                    from CORE_UENS C
                    WHERE C.UNIDAD_PRINCIPAL = '{cod_unidad}' OR C.COD_UNIDAD = '{cod_unidad}' 
                    order by C.CNTX_UNIDAD desc";
                    response.Result.uens = connection.Query<CoreUeNsDto>(query).ToList();
                    if (response.Result.uens[0].cntx_unidad == null || response.Result.uens[0].cntx_unidad == "")
                    {
                        response.Result = null;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.uens = null;
                response.Result.Total = 0;
            }
            return response;
        }

        /// <summary>
        /// Obtener los centros de costo pertenecientes a una UEN
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <returns></returns>
        public ErrorDto<CoreUeNsDtoList> Core_SubCentroCosto_Obtener(int CodCliente, string cod_unidad, string sub_unidad)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<CoreUeNsDtoList>();
            response.Result = new CoreUeNsDtoList();
            response.Code = 0;
            response.Result.Total = 0;
            try
            {
                var query = "";
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = @$"select C.COD_UNIDAD, C.CNTX_UNIDAD, C.CNTX_CENTRO_COSTO, C.ACTIVA, '{cod_unidad}' AS UNIDAD_PRINCIPAL,
                    (select TOP 1 DESCRIPCION from CNTX_CENTRO_COSTOS WHERE COD_CENTRO_COSTO = C.CNTX_CENTRO_COSTO) AS DESCRIPCION
                    from CORE_UENS C
                    WHERE (C.UNIDAD_PRINCIPAL = '{cod_unidad}' OR C.COD_UNIDAD = '{cod_unidad}' ) AND C.CNTX_UNIDAD = '{sub_unidad}'
                    order by C.CNTX_CENTRO_COSTO desc";
                    response.Result.uens = connection.Query<CoreUeNsDto>(query).ToList();
                    if (response.Result.uens[0].cntx_centro_costo == null || response.Result.uens[0].cntx_centro_costo == "")
                    {
                        response.Result = null;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.uens = null;
                response.Result.Total = 0;
            }
            return response;
        }

        /// <summary>
        /// Obtiene los miembros
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CoreUsuariosDto>> Core_Miembros_Obtener(int CodCliente, string cod_unidad, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<CoreUsuariosDto>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec spSys_UENS_Miembros_Consultas '{cod_unidad}', '{filtro}'";
                    response.Result = connection.Query<CoreUsuariosDto>(query).ToList();
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
        /// Registra los miembros
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Core_Miembros_Registro(int CodCliente, string cod_unidad, CoreUsuariosDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            var mov = 'E';
            try
            {
                if (request.asignado == true)
                {
                    mov = 'A';
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec spSys_UENS_Miembros_Registro '{cod_unidad}', '{request.core_usuario}',
                        '{request.registro_usuario}', {mov}";
                    connection.Execute(query);
                    resp.Description = "Registro actualizado satisfactoriamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene los roles
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDto<List<CoreRolesDto>> Core_Roles_Obtener(int CodCliente, string cod_unidad, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<CoreRolesDto>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec spSys_UENS_Roles_Consultas '{cod_unidad}', '{filtro}'";
                    response.Result = connection.Query<CoreRolesDto>(query).ToList();
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
        /// Registra los Roles
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_unidad"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Core_Roles_Registro(int CodCliente, string cod_unidad, CoreRolesDto request)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = @$"exec spSys_UENS_Roles_Registro '{cod_unidad}', '{request.core_usuario}',
                        {Convert.ToInt32(request.rol_solicita)}, {Convert.ToInt32(request.rol_consulta)},
                        {Convert.ToInt32(request.rol_autoriza)},{Convert.ToInt32(request.rol_encargado)},
                        {Convert.ToInt32(request.rol_lider)},'{request.registro_usuario}'";
                    connection.Execute(query);
                    resp.Description = "Registro actualizado satisfactoriamente";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Obtiene la lista de UENs
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<UensListaDatos>> Core_UENLista_Obtener(int CodCliente, string? usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<UensListaDatos>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    string query = "";
                    if (usuario == null || usuario == "null")
                    {
                        query = "select U.COD_UNIDAD AS ITEM, U.DESCRIPCION, U.CntX_Unidad, U.CntX_Centro_Costo from CORE_UENS U";
                    }
                    else
                    {
                        query = @$"SELECT S.COD_UNIDAD AS ITEM, U.DESCRIPCION, U.CntX_Unidad, U.CntX_Centro_Costo FROM CORE_UENS_USUARIOS_ROLES S LEFT JOIN 
                                    CORE_UENS U ON S.COD_UNIDAD = U.COD_UNIDAD
                                    WHERE S.CORE_USUARIO = '{usuario}'";
                    }

                    response.Result = connection.Query<UensListaDatos>(query).ToList();
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