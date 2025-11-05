using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCpr_ProveedoresDB
    {
        private readonly IConfiguration _config;
        private readonly mProGrX_AuxiliarDB mProGrX_AuxiliarDB;

        public frmCpr_ProveedoresDB(IConfiguration config)
        {
            _config = config;
            mProGrX_AuxiliarDB = new mProGrX_AuxiliarDB(config);
        }

        public ErrorDto CprProveedores_Importar(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"exec spCPR_Proveedores_Importar";
                    var sp = connection.Execute(query);
                }

                resp.Description = "Proveedores Sincronizados/Importados Satisfactoriamente!";

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto<cprProveedoresDTO> CprProveedor_Scroll(int CodEmpresa, int scroll, string? codigo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<cprProveedoresDTO>();
            try
            {
                string where = " ", orderBy = " ";
                if (scroll == 1)
                {
                    where = $@" where PROVEEDOR_CODIGO > '{codigo}' ";
                    orderBy = " order by PROVEEDOR_CODIGO asc";
                }
                else
                {
                    where = $@" where PROVEEDOR_CODIGO < '{codigo}' ";
                    orderBy = " order by PROVEEDOR_CODIGO desc";
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Top 1 * from CPR_PROVEEDORES_TEMPO {where} {orderBy}";
                    response.Result = connection.Query<cprProveedoresDTO>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<cprProveedoresLista> CprProveedoresLista_Obtener(int CodEmpresa, string filtros)
        {
            cprProveedoresFiltros filtro = JsonConvert.DeserializeObject<cprProveedoresFiltros>(filtros) ?? new cprProveedoresFiltros();
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<cprProveedoresLista>();
            response.Result = new cprProveedoresLista();
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";

                using var connection = new SqlConnection(clienteConnString);
                {
                    //Busco Total
                    query = "SELECT COUNT(*) FROM CPR_PROVEEDORES_TEMPO";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();
                    if (filtro.filtro != null)
                    {
                        filtro.filtro = " WHERE (PROVEEDOR_CODIGO LIKE '%" + filtro.filtro + "%' " +
                             "OR cedjur LIKE '%" + filtro.filtro + "%' " +
                             "OR descripcion LIKE '%" + filtro.filtro + "%') ";
                    }

                    if (filtro.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                    }

                    query = $@"select PROVEEDOR_CODIGO, cedjur, descripcion from CPR_PROVEEDORES_TEMPO 
                                         {filtro.filtro} 
                                        ORDER BY PROVEEDOR_CODIGO
                                        {paginaActual}
                                        {paginacionActual} ";

                    response.Result.proveedores = connection.Query<cprProveedoresDTO>(query).ToList();

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

        public ErrorDto<cprProveedoresDTO> CprProveedores_Obtener(int CodEmpresa, string codigo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto<cprProveedoresDTO> info = new ErrorDto<cprProveedoresDTO>();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select P.* from CPR_PROVEEDORES_TEMPO P where P.PROVEEDOR_CODIGO = '{codigo}' ";
                    info.Result = connection.QueryFirstOrDefault<cprProveedoresDTO>(query);
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
                info.Result = null;
            }

            return info;
        }

        public ErrorDto CprProveedores_Guardar(int CodEmpresa, bool vEdita, cprProveedoresDTO proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            resp.Description = "";

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    if (!vEdita)
                    {
                        //Verifica que exista ningun otro proveedor con la misma cedula juridica
                        var query = $@"select isnull(count(*),0) as Existe from CXP_PROVEEDORES  where COD_PROVEEDOR in ({proveedor.proveedor_codigo}) 
                                        OR REPLACE(REPLACE(CEDJUR, ' ', ''), '-', '')  = '{proveedor.cedjur.Replace("-","").Replace(" ", "")}' ";
                        var existe = connection.QueryFirstOrDefault<int>(query);
                        if (existe > 0)
                        {
                            resp.Description += " - Existe ya un Proveedor registrado con la misma C�dula Jur�dica ...";
                        }

                        if (!mProGrX_AuxiliarDB.fxCorreoValido(proveedor.email))
                        {
                            resp.Description += " - El Email principal no es v�lido!";
                        }

                        if (proveedor.descripcion == null)
                        {
                            resp.Description += " - Nombre del Proveedor no es v�lido ...";
                        }

                        if (resp.Description == "")
                        {
                            resp = CprProveedores_Insertar(CodEmpresa, proveedor);
                        }
                        else
                        {
                            resp.Code = -1;
                        }
                    }
                    else
                    {
                        resp = CprProveedores_Actualizar(CodEmpresa, proveedor);
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

        private ErrorDto CprProveedores_Insertar(int CodEmpresa, cprProveedoresDTO proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Obtengo consecutivo
                    var query = $@"select isnull(max(COD_PROVEEDOR), 10000) +1 as ultimo from CXP_PROVEEDORES";
                    proveedor.proveedor_codigo = connection.QueryFirstOrDefault<int>(query);

                    query = $@"INSERT INTO [dbo].[CPR_PROVEEDORES_TEMPO]
                                   ([PROVEEDOR_CODIGO]
                                   ,[TIPO]
                                   ,[CEDJUR]
                                   ,[DESCRIPCION]
                                   ,[OBSERVACION]
                                   ,[TELEFONO]
                                   ,[EMAIL]
                                   ,[ESTADO]
                                   ,[COD_PROVEEDOR]
                                   ,[REGISTRO_FECHA]
                                   ,[REGISTRO_USUARIO]
                                   
                                   )
                             VALUES
                                   ({proveedor.proveedor_codigo}
                                   ,'{proveedor.tipo}'
                                   ,'{proveedor.cedjur}'
                                   ,'{proveedor.descripcion}'
                                   ,'{proveedor.observacion}'
                                   ,'{proveedor.telefono}'
                                   ,'{proveedor.email}'
                                   ,'{proveedor.estado}'
                                   ,{proveedor.proveedor_codigo}
                                   ,getDate()
                                   ,'{proveedor.registro_usuario}'
                                   )";
                    connection.Execute(query);

                    resp.Description = proveedor.proveedor_codigo.ToString();

                    //actualizo proveedores en CxP
                    query = $@"INSERT INTO CXP_PROVEEDORES(
                                COD_PROVEEDOR, COD_CLASIFICACION ,TIPO, 
                                CEDJUR, DESCRIPCION, OBSERVACION, 
                                ESTADO, TELEFONO, EMAIL, 
                                REGISTRO_FECHA, REGISTRO_USUARIO, 
                                CREDITO_PLAZO, CREDITO_MONTO, DESCUENTO_PORC, 
                                SALDO )
                                SELECT 
                                P.COD_PROVEEDOR, (SELECT TOP 1 COD_CLASIFICACION  FROM CXP_PROV_CLAS WHERE ACTIVO = 1), P.TIPO,
                                P.CEDJUR, P.DESCRIPCION, P.OBSERVACION,
                                P.ESTADO, P.TELEFONO, P.EMAIL,
                                 GETDATE(), P.REGISTRO_USUARIO,
                                0,0,0,0
                                FROM CPR_PROVEEDORES_TEMPO P WHERE NOT EXISTS (
                                SELECT 1 
                                    FROM CXP_PROVEEDORES t2 
                                    WHERE P.COD_PROVEEDOR = t2.COD_PROVEEDOR
                                ) ";
                    connection.Execute(query);

                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        private ErrorDto CprProveedores_Actualizar(int CodEmpresa, cprProveedoresDTO proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"update CPR_PROVEEDORES_TEMPO set descripcion = '{proveedor.descripcion}', CedJur = '{proveedor.cedjur}', 
                                    Tipo = '{proveedor.tipo}',observacion = '{proveedor.observacion}', Estado = '{proveedor.estado}', 
                                    email = '{proveedor.email}', telefono = '{proveedor.telefono}', MODIFICA_FECHA = Getdate(), 
                                    MODIFICA_USUARIO = '{proveedor.modifica_usuario}' where PROVEEDOR_CODIGO = {proveedor.proveedor_codigo}";
                    connection.Execute(query);

                    resp.Description = proveedor.proveedor_codigo.ToString();
                }

            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CprProveedores_Eliminar(int CodEmpresa, string codigo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete CPR_PROVEEDORES_TEMPO where PROVEEDOR_CODIGO = {codigo}";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<float> CprProveedorPuntaje_Obtener(int CodEmpresa, string codigo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<float>();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@" SELECT 
                                       AVG(V.[VALORA_PUNTAJE]) as VALORA_PUNTAJE 
                                  FROM CPR_SOLICITUD_PROV V 
                                  WHERE V.PROVEEDOR_CODIGO = '{codigo}'
                                  GROUP  BY V.PROVEEDOR_CODIGO
                                  ORDER  BY V.PROVEEDOR_CODIGO DESC ";
                   resp.Result = connection.QueryFirstOrDefault<float>(query);
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<List<cprProveedorBitacoraData>> CprProveedoreBitacoraPuntaje(int CodEmpresa, string codigo)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDto<List<cprProveedorBitacoraData>>();
            resp.Code = 0;
            resp.Result = new List<cprProveedorBitacoraData>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT V.[CPR_ID]
                                      ,V.[ESTADO]
                                      ,V.[VALORA_FECHA]
                                      ,V.[VALORA_USUARIO]
                                      ,V.[VALORA_PUNTAJE]
                                  FROM CPR_SOLICITUD_PROV V LEFT JOIN 
                                  CPR_PROVEEDORES_TEMPO P ON V.PROVEEDOR_CODIGO = P.COD_PROVEEDOR
                                  WHERE V.PROVEEDOR_CODIGO = '{codigo}'
                                  ORDER BY V.PROVEEDOR_CODIGO DESC ";
                    resp.Result = connection.Query<cprProveedorBitacoraData>(query).ToList();
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