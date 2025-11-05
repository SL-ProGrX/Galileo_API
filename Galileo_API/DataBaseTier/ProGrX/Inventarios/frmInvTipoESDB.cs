using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmInvTipoESDB
    {
        private readonly IConfiguration _config;

        public frmInvTipoESDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene TipoES
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDTO<TipoESList> TipoES_Obtener(int CodEmpresa, string filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            TipoESFiltros filtro = JsonConvert.DeserializeObject<TipoESFiltros>(filtros);
            var response = new ErrorDTO<TipoESList>
            {
                Code = 0,
                Result = new TipoESList()
            };
            response.Result.Total = 0;

            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                if (filtro.filtro != null)
                {
                    filtro.filtro = " WHERE T.cod_entsal LIKE '%" + filtro.filtro + "%' OR T.descripcion LIKE '%" + filtro.filtro + "%' "+
                        " OR T.cod_cuenta LIKE '%" + filtro.filtro + "%' OR C.descripcion LIKE '%" + filtro.filtro + "%' ";
                }

                if (filtro.pagina != null)
                {
                    paginaActual = " OFFSET " + filtro.pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + filtro.paginacion + " ROWS ONLY ";
                }
                using var connection = new SqlConnection(stringConn);
                {
                    query = "SELECT COUNT(*) FROM pv_entrada_salida T LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    query = $@"SELECT T.cod_entsal,T.descripcion as descripcion,T.tipo,T.cod_cuenta,T.activo,C.descripcion AS ctaDesc, T.mancomunado
                    FROM pv_entrada_salida T LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta {filtro.filtro}  
                    ORDER BY T.cod_entsal {paginaActual} {paginacionActual}";
                    response.Result.Lista = connection.Query<TipoESDTO>(query).ToList();

                    //elimina duplicados
                    response.Result.Lista = response.Result.Lista.GroupBy(x => x.Cod_Entsal).Select(x => x.FirstOrDefault()).ToList();

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
        /// Busca tipos de Transacciones/Movimientos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Tipo"></param>
        /// <returns></returns>
        public ErrorDTO<List<TipoESDTO>> TipoES_Buscar(int CodEmpresa, string Tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);


            var response = new ErrorDTO<List<TipoESDTO>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT DISTINCT T.cod_entsal,T.descripcion as descripcion,T.tipo,T.cod_cuenta,T.activo,C.descripcion AS ctaDesc
                    FROM pv_entrada_salida T LEFT JOIN CntX_cuentas C ON T.cod_cuenta = C.cod_cuenta WHERE T.tipo = '{Tipo}'
                    ORDER BY T.cod_entsal";

                    response.Result = connection.Query<TipoESDTO>(query).ToList();
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
        ///  Actualiza los tipos de Transacciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO TipoES_Actualizar(int CodEmpresa, TipoESDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDTO
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "Update pv_entrada_salida set descripcion = @Descripcion, cod_cuenta = @Cod_Cuenta, tipo = @Tipo, activo = @Activo, mancomunado = @Mancomunado " +
                        "where cod_Entsal = @Cod_Entsal";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Entsal", request.Cod_Entsal, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Tipo", request.Tipo, DbType.String);
                    parameters.Add("Cod_Cuenta", request.Cod_Cuenta.Replace("-", ""), DbType.String);
                    parameters.Add("Activo", request.Activo, DbType.Boolean);
                    parameters.Add("Mancomunado", request.Mancomunado, DbType.Boolean);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Registro actualizado correctamente";
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
        ///   Inserta un nuevo tipo de Transaccion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDTO TipoES_Insertar(int CodEmpresa, TipoESDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var resp = new ErrorDTO
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "insert into pv_entrada_salida(cod_Entsal,descripcion,tipo,cod_cuenta,activo,mancomunado)values(@Cod_Entsal,@Descripcion,@Tipo,@Cod_Cuenta,@Activo,@Mancomunado)";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Entsal", request.Cod_Entsal, DbType.String);
                    parameters.Add("Descripcion", request.Descripcion, DbType.String);
                    parameters.Add("Tipo", request.Tipo, DbType.String);
                    parameters.Add("Cod_Cuenta", request.Cod_Cuenta.Replace("-", ""), DbType.String);
                    parameters.Add("Activo", request.Activo, DbType.Boolean);
                    parameters.Add("Mancomunado", request.Mancomunado, DbType.Boolean);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Registro agregado correctamente";
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
        ///  Elimina un tipo de Transaccion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codTiposES"></param>
        /// <returns></returns>
        public ErrorDTO TipoES_Eliminar(int CodEmpresa, string codTiposES)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var resp = new ErrorDTO
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "DELETE pv_entrada_salida where cod_Entsal = @Cod_Entsal";

                    var parameters = new DynamicParameters();
                    parameters.Add("Cod_Entsal", codTiposES, DbType.String);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = "Registro eliminado correctamente";
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