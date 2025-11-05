using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.GEN;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCC_CA_EntidadesDB
    {
        private readonly IConfiguration _config;
        mSecurityMainDb DBBitacora;

        public frmCC_CA_EntidadesDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
        }

        public ErrorDTO Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        public List<PRM_CA_EntidadData> CC_CA_Entidades_Obtener(int CodEmpresa, string Filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            FiltroLazy filtros = JsonConvert.DeserializeObject<FiltroLazy>(Filtros);
            List<PRM_CA_EntidadData> resp = new List<PRM_CA_EntidadData>();
            string paginaActual = " ", paginacionActual = " ";

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string filtro = "";

                    if (filtros.filtro != null)
                    {
                        filtro = " Where cod_entidad LIKE '%" + filtros.filtro + "%' OR descripcion LIKE '%" + filtros.filtro + "%' ";
                    }

                    if (filtros.pagina != null)
                    {
                        paginaActual = " OFFSET " + filtros.pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + filtros.paginacion + " ROWS ONLY ";
                    }

                    var query = $@"select cod_entidad,descripcion,NUMERO_AFILIADO,formato,cod_cuenta,activo 
                        from prm_ca_Entidad 
                            {filtro}
                        order by cod_entidad
                            {paginaActual} {paginacionActual}";
                    resp = connection.Query<PRM_CA_EntidadData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public ErrorDTO CC_CA_Entidad_Upsert(int CodEmpresa, PRM_CA_EntidadUpsert request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(count(*),0) as Existe from prm_ca_Entidad where cod_entidad = '{request.cod_entidad}'";
                    bool existe = connection.Query<bool>(query).FirstOrDefault();

                    if (existe == false)
                    {
                        var query1 = "insert into prm_ca_Entidad(cod_entidad,descripcion,NUMERO_AFILIADO,formato,cod_cuenta,activo,registro_Fecha,registro_usuario)" +
                            "values(@cod_entidad, @descripcion, @numero_afiliado, @formato, @cod_cuenta, @activo, Getdate(), @registro_usuario)";
                        var parameters1 = new DynamicParameters();
                        parameters1.Add("cod_entidad", request.cod_entidad, DbType.String);
                        parameters1.Add("descripcion", request.descripcion, DbType.String);
                        parameters1.Add("numero_afiliado", request.numero_afiliado, DbType.String);
                        parameters1.Add("formato", request.formato, DbType.String);
                        parameters1.Add("cod_cuenta", request.cod_cuenta, DbType.String);
                        parameters1.Add("activo", request.activo, DbType.Boolean);
                        parameters1.Add("registro_usuario", request.registro_usuario, DbType.String);
                        resp.Code = connection.ExecuteAsync(query1, parameters1).Result;
                        resp.Description = "Entidad agregada exitosamente!";

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = request.registro_usuario.ToUpper(),
                            DetalleMovimiento = "Cargos Automáticos - Entidad: " + request.cod_entidad,
                            Movimiento = "REGISTRA",
                            Modulo = 10
                        });
                    }
                    else
                    {
                        var query2 = "UPDATE prm_ca_Entidad SET descripcion = @descripcion, numero_afiliado = @numero_afiliado, " +
                            "formato = @formato, cod_cuenta = @cod_cuenta, activo = @activo " +
                            "WHERE cod_entidad = @cod_entidad";
                        var parameters2 = new DynamicParameters();
                        parameters2.Add("cod_entidad", request.cod_entidad, DbType.String);
                        parameters2.Add("descripcion", request.descripcion, DbType.String);
                        parameters2.Add("numero_afiliado", request.numero_afiliado, DbType.String);
                        parameters2.Add("formato", request.formato, DbType.String);
                        parameters2.Add("cod_cuenta", request.cod_cuenta, DbType.String);
                        parameters2.Add("activo", request.activo, DbType.Boolean);
                        resp.Code = connection.ExecuteAsync(query2, parameters2).Result;
                        resp.Description = "Entidad actualizada exitosamente!";

                        Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = request.registro_usuario.ToUpper(),
                            DetalleMovimiento = "Cargos Automáticos - Entidad: " + request.cod_entidad,
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

        public ErrorDTO CC_CA_Entidad_Delete(int CodEmpresa, string Usuario, string Codigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new ErrorDTO();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete prm_ca_Entidad where cod_entidad = '{Codigo}'";

                    resp.Code = connection.Execute(query);
                    resp.Description = "Entidad eliminada exitosamente!";

                    Bitacora(new BitacoraInsertarDTO
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = Usuario.ToUpper(),
                        DetalleMovimiento = "Cargos Automáticos - Entidad: " + Codigo,
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