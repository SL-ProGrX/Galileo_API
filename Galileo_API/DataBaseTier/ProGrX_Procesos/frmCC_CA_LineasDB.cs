using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.GEN;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCC_CA_LineasDB
    {
        private readonly IConfiguration _config;
        MSecurityMainDb DBBitacora;

        public frmCC_CA_LineasDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        public List<PrmCaLineasData> CC_CA_Lineas_Obtener(int CodEmpresa, string Filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            FiltroLazy filtros = JsonConvert.DeserializeObject<FiltroLazy>(Filtros);
            List<PrmCaLineasData> resp = new List<PrmCaLineasData>();
            string paginaActual = " ", paginacionActual = " ";

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string filtro = "";

                    if (filtros.filtro != null)
                    {
                        filtro = " Where Cod_Linea LIKE '%" + filtros.filtro + "%' OR descripcion LIKE '%" + filtros.filtro + "%' ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    var query = $@"select Cod_Linea,descripcion,cod_plan, activo 
                        from PRM_CA_LINEAS 
                            {filtro}
                        order by Cod_Linea
                            {paginaActual} {paginacionActual}";
                    resp = connection.Query<PrmCaLineasData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<CcCaLineasActivasData> CC_CA_Lineas_ActivasObtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<CcCaLineasActivasData> resp = new List<CcCaLineasActivasData>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select rtrim(Cod_Linea) + ' - ' + descripcion as 'ItmX' from PRM_CA_LINEAS where activo = 1";
                    resp = connection.Query<CcCaLineasActivasData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CA_Linea_Upsert(int CodEmpresa, PrmCaLineaUpsert request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(count(*),0) as Existe from PRM_CA_LINEAS where Cod_Linea = '{request.cod_linea}'";
                    bool existe = connection.Query<bool>(query).FirstOrDefault();

                    if (existe == false)
                    {
                        var query1 = "insert into PRM_CA_LINEAS(Cod_Linea,descripcion,cod_plan,Activo,Registro_Usuario,Registro_Fecha) " +
                            "values(@cod_Linea, @descripcion, @cod_plan, @activo, @registro_usuario, Getdate())";
                        var parameters1 = new DynamicParameters();
                        parameters1.Add("cod_Linea", request.cod_linea, DbType.String);
                        parameters1.Add("descripcion", request.descripcion, DbType.String);
                        parameters1.Add("cod_plan", request.cod_plan, DbType.String);
                        parameters1.Add("activo", request.activo, DbType.Boolean);
                        parameters1.Add("registro_usuario", request.registro_usuario, DbType.String);
                        resp.Code = connection.ExecuteAsync(query1, parameters1).Result;
                        resp.Description = "Linea agregada exitosamente!";

                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = request.registro_usuario.ToUpper(),
                            DetalleMovimiento = "Cargo Automatico - Tipo Linea: " + request.cod_linea,
                            Movimiento = "REGISTRA",
                            Modulo = 10
                        });
                    }
                    else
                    {
                        var query2 = "UPDATE PRM_CA_LINEAS SET descripcion = @descripcion, cod_plan = @cod_plan, activo = @activo " +
                            "WHERE cod_Linea = @cod_Linea";
                        var parameters2 = new DynamicParameters();
                        parameters2.Add("cod_Linea", request.cod_linea, DbType.String);
                        parameters2.Add("descripcion", request.descripcion, DbType.String);
                        parameters2.Add("cod_plan", request.cod_plan, DbType.String);
                        parameters2.Add("activo", request.activo, DbType.Boolean);
                        resp.Code = connection.ExecuteAsync(query2, parameters2).Result;
                        resp.Description = "Linea actualizada exitosamente!";

                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = request.registro_usuario.ToUpper(),
                            DetalleMovimiento = "Cargo Automatico - Tipo Linea: " + request.cod_linea,
                            Movimiento = "MODIFICA",
                            Modulo = 10
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = 0;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CA_Linea_Delete(int CodEmpresa, string Usuario, string Codigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var validaQuery = $@"SELECT CASE WHEN EXISTS (
	                        SELECT 1 FROM Catalogo Cat
                            LEFT JOIN prm_Ca_Lineas_Dt Dt ON Cat.codigo = Dt.Codigo 
	                        AND Dt.cod_Linea = '{Codigo}'
                            WHERE Dt.Codigo IS NOT NULL
                        ) THEN 'true' ELSE 'false' END AS Existe
                    ";
                    var valida = connection.QuerySingle<string>(validaQuery, new { Codigo });

                    if (valida == "true")
                    {
                        var query2 = $@"delete prm_ca_lineas_dt where cod_linea = '{Codigo}'";
                        connection.Execute(query2);
                    }
                    var query = $@"delete PRM_CA_LINEAS where cod_linea = '{Codigo}'";

                    resp.Code = connection.Execute(query);
                    resp.Description = "Linea eliminada exitosamente!";

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Cargo Automatico - Tipo Linea: " + Codigo,
                        Movimiento = "ELIMINA",
                        Modulo = 10
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = 0;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public List<CcCaCodigosAsignadosData> CC_CA_CodigosAsignados_Obtener(int CodEmpresa, string Filtros, string Codigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            FiltroLazy filtros = JsonConvert.DeserializeObject<FiltroLazy>(Filtros);
            List<CcCaCodigosAsignadosData> resp = new List<CcCaCodigosAsignadosData>();
            string paginaActual = " ", paginacionActual = " ";

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string filtro = "";

                    if (filtros.filtro != null)
                    {
                        filtro = " Where Cat.Codigo LIKE '%" + filtros.filtro + "%' OR Cat.Descripcion LIKE '%" + filtros.filtro + "%' ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    var query = $@"select Cat.Codigo,Cat.Descripcion, isnull(Dt.Codigo,'-1') as 'Existe'  
                        from Catalogo Cat left join prm_Ca_Lineas_Dt Dt on Cat.codigo = Dt.Codigo and Dt.cod_Linea = '{Codigo}' 
                            {filtro}
                        Order by isnull(Dt.Codigo,'ZZZZZZZ'),Cat.Codigo
                            {paginaActual} {paginacionActual}";
                    resp = connection.Query<CcCaCodigosAsignadosData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CA_CodigoAsignado_Insert(int CodEmpresa, PrmCaLineasDtInsert request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query1 = "insert prm_ca_lineas_dt(cod_linea,codigo,registro_usuario,registro_Fecha) " +
                            "values(@cod_Linea, @codigo, @registro_usuario, Getdate())";
                    var parameters1 = new DynamicParameters();
                    parameters1.Add("cod_Linea", request.cod_linea, DbType.String);
                    parameters1.Add("codigo", request.codigo, DbType.String);
                    parameters1.Add("registro_usuario", request.registro_usuario, DbType.String);
                    resp.Code = connection.ExecuteAsync(query1, parameters1).Result;

                    resp.Description = "C�digo " + request.codigo + " asignado!";

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = request.registro_usuario.ToUpper(),
                        DetalleMovimiento = "Cargo Automatico: Linea: " + request.cod_linea + " Cod: " + request.codigo,
                        Movimiento = "REGISTRA",
                        Modulo = 10
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = 0;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto CC_CA_CodigoAsignado_Delete(int CodEmpresa, string CodLinea, string Codigo, string Usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete prm_ca_lineas_dt where cod_linea = '{CodLinea}' and codigo = '{Codigo}'";
                    resp.Code = connection.Execute(query);
                    resp.Description = "C�digo " + Codigo + " desasignado!";

                    Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Cargo Automatico: Linea: " + CodLinea + " Cod: " + Codigo,
                        Movimiento = "ELIMINA",
                        Modulo = 10
                    });
                }
            }
            catch (Exception ex)
            {
                resp.Code = 0;
                resp.Description = ex.Message;
            }
            return resp;
        }
    }
}