using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmInvTranESDB
    {
        private readonly IConfiguration _config;

        public frmInvTranESDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDto<TranESData> InvTranES_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TranESData>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select X.*,rtrim(C.descripcion) as Causa
                    from PV_INVTRANSAC X inner join pv_entrada_salida C on X.cod_entsal = C.cod_entsal
                    where X.boleta = '{CodBoleta}' and X.tipo = '{TipoTran}'";
                    response.Result = connection.Query<TranESData>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            if (response.Result != null)
            {
                switch (response.Result.Estado)
                {
                    case "S":
                        response.Result.Estado = "Solicitada";
                        break;
                    case "A":
                        response.Result.Estado = "Autorizada";
                        break;
                    case "P":
                        response.Result.Estado = "Procesada";
                        break;
                    case "R":
                        response.Result.Estado = "Rechazada";
                        break;
                    default:
                        break;
                }
            }
            return response;
        }

        public ErrorDto<List<InvProducLineas>> InvProducLineas_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<InvProducLineas>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string traslado = $"";

                    if (TipoTran == "T")
                    {
                        traslado = $",D.COD_BODEGA_DESTINO";
                    }
                    var query = $@"select D.linea,D.cod_producto,P.descripcion,D.cantidad,B.cod_bodega,B.descripcion as Bodega {traslado} ,D.precio,
                    (D.cantidad * D.precio) as Total, isnull(D.despacho,0) as Despacho 
                    from PV_INVTRADET D inner join pv_productos P on D.cod_producto = P.cod_producto
                    inner join PV_Bodegas B on D.cod_bodega = B.cod_bodega
                    where D.boleta = '{CodBoleta}' and D.tipo = '{TipoTran}'";
                    response.Result = connection.Query<InvProducLineas>(query).ToList();
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

        public ErrorDto<TranESData> InvTranES_scroll(int CodEmpresa, int scrollValue, string? CodBoleta, string TipoTran)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<TranESData>
            {
                Code = 0
            };
            try
            {
                string filtro = $"where tipo = '{TipoTran}' ";

                if (scrollValue == 1)
                {
                    filtro += $"and boleta > '{CodBoleta}' order by boleta asc";
                }
                else
                {
                    filtro += $"and boleta < '{CodBoleta}' order by boleta desc";
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Top 1 Boleta from pv_invTransac {filtro}";
                    response.Result = connection.Query<TranESData>(query).FirstOrDefault();
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

        public ErrorDto InvTranES_Insertar(int CodEmpresa, string TipoTran, TranESData request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            string ultimaBoleta = string.Empty;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryC = $@"select isnull(max(Boleta),0)+1 as Ultimo from pv_InvTranSac where Tipo = '{TipoTran}'";

                    var consecutivo = connection.Query<string>(queryC).FirstOrDefault();
                    consecutivo = consecutivo.PadLeft(10, '0');
                    ultimaBoleta = consecutivo.ToString();

                    var query = "insert pv_InvTranSac(Boleta,Tipo,cod_entsal,genera_fecha,documento," +
                        "notas,genera_user,estado,plantilla,fecha,fecha_sistema,total)" +
                        "values(@Boleta,@Tipo,@Cod_Entsal,getdate(),@Documento,@Notas," +
                        "@Genera_User,'S',@Plantilla, getdate(),getdate(),@Total)";

                    var parameters = new DynamicParameters();
                    parameters.Add("Boleta", ultimaBoleta, DbType.String);
                    parameters.Add("Tipo", TipoTran, DbType.String);
                    parameters.Add("Cod_Entsal", request.Cod_Entsal, DbType.String);
                    parameters.Add("Documento", request.Documento, DbType.String);
                    parameters.Add("Notas", request.Notas, DbType.String);
                    parameters.Add("Genera_User", request.Genera_User, DbType.String);
                    parameters.Add("Total", request.Total, DbType.Decimal);
                    parameters.Add("Plantilla", request.Plantilla, DbType.Boolean);

                    resp.Code = connection.ExecuteAsync(query, parameters).Result;
                    resp.Description = ultimaBoleta;
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto InvTranES_Actualizar(int CodEmpresa, TranESUpdate request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = "Update pv_InvTranSac SET cod_Entsal = @Cod_Entsal, fecha = getdate(), " +
                        "documento = @Documento, notas = @Notas, Total = @Total, plantilla = @Plantilla " +
                        "WHERE Boleta = @Boleta and Tipo = @Tipo ";

                    var parameters = new DynamicParameters();
                    parameters.Add("Boleta", request.Boleta, DbType.String);
                    parameters.Add("Tipo", request.Tipo, DbType.String);
                    parameters.Add("Cod_Entsal", request.Cod_Entsal, DbType.String);
                    parameters.Add("Documento", request.Documento, DbType.String);
                    parameters.Add("Notas", request.Notas, DbType.String);
                    parameters.Add("Total", request.Total, DbType.Decimal);
                    parameters.Add("Plantilla", request.Plantilla, DbType.Boolean);

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

        public ErrorDto InvTranES_Eliminar(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete pv_InvTraDet where tipo = '{TipoTran}' and Boleta = '{CodBoleta}'";
                    resp.Code = connection.Execute(query);

                    if (resp.Code >= 0)
                    {
                        query = $@"delete pv_InvTranSac where tipo = '{TipoTran}' and Boleta = '{CodBoleta}'";
                        connection.Execute(query);

                        resp.Description = "Registro eliminado correctamente";
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

        public ErrorDto InvProducLineas_Insertar(int CodEmpresa, string CodBoleta, string TipoTran, List<InvProducLineasInsert> producLineas)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto errorDTO = new()
            {
                Code = 0
            };
            InvProducLineasInsert lineasInsert = new InvProducLineasInsert();
            
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete pv_InvTraDet where boleta = '{CodBoleta}' and tipo = '{TipoTran}'";
                    var resp = connection.Execute(query);

                    if (resp >= 0)
                    {
                        string trasladoCol = $"";
                        string trasladoVal = $"";

                        int contador = 0;
                        foreach (InvProducLineasInsert item in producLineas)
                        {
                            contador++;

                            if (TipoTran == "T")
                            {
                                trasladoCol = $",cod_bodega_destino";
                                trasladoVal = $",'{item.Cod_Bodega_Destino}'";
                            }

                            query = $@"insert pv_InvTraDet(linea,Boleta,tipo,cod_producto,cod_bodega,cantidad,despacho,precio {trasladoCol})
                            values( {contador}, '{CodBoleta}', '{TipoTran}', '{item.Cod_Producto}', '{item.Cod_Bodega}', 
                            {item.Cantidad}, {item.Despacho}, {item.Precio} {trasladoVal})";

                            connection.Execute(query);
                        }

                        errorDTO.Description = "Informaci�n guardada correctamente";
                    }
                }
            }
            catch (Exception ex)
            {
                errorDTO.Code = -1;
                errorDTO.Description = ex.Message;
            }
            return errorDTO;
        }

        public ErrorDto<List<InvTranPlantilla>> InvTranPlantilla_Obtener(int CodEmpresa, string TipoTran, string? CodBoleta, string? GeneraUser, string? GeneraFecha)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<InvTranPlantilla>>
            {
                Code = 0
            };
            try
            {

                string filtro = $"where plantilla = 1 and tipo = '{TipoTran}' ";

                if (!string.IsNullOrEmpty(CodBoleta))
                {
                    filtro += $"and boleta = '{CodBoleta}' ";
                }
                if (!string.IsNullOrEmpty(GeneraUser))
                {
                    filtro += $"and genera_user like '%{GeneraUser}%' ";
                }
                if (!string.IsNullOrEmpty(GeneraFecha))
                {
                    filtro += $"and genera_fecha between '{GeneraFecha} 00:00:00' and '{GeneraFecha} 23:59:59' ";
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select boleta,genera_user,genera_fecha,documento,notas from pv_InvTransac {filtro}";
                    response.Result = connection.Query<InvTranPlantilla>(query).ToList();
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

        public ErrorDto InvProducLineas_Eliminar(int CodEmpresa, string CodBoleta, string TipoTran, int Linea)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new()
            {
                Code = 0
            };
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete pv_InvTraDet where tipo = '{TipoTran}' and Boleta = '{CodBoleta}' and linea = '{Linea}'";
                    resp.Code = connection.Execute(query);

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