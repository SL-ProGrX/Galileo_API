using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvTranReversionDB
    {
        private readonly IConfiguration _config;

        public frmInvTranReversionDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<TranReversionData> InvTranReversion_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TranReversionData>();
            response.Code = 0;

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select X.*,(rtrim(C.cod_entsal) + ' - ' + C.descripcion) as Causa
                        from PV_INVTRANSAC X inner join pv_entrada_salida C on X.cod_entsal = C.cod_entsal
                        where X.boleta = '{CodBoleta}' and X.tipo = '{TipoTran}'";
                    response.Result = connection.Query<TranReversionData>(query).FirstOrDefault();
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

        public ErrorDTO<List<InvProducReversion>> InvProducLineas_Obtener(int CodEmpresa, string CodBoleta, string TipoTran)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<InvProducReversion>>
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select D.Linea,D.cod_producto,P.descripcion,D.cantidad,B.cod_bodega,B.descripcion as Bodega,D.precio,
                        (D.cantidad * D.precio) as Total, isnull(D.despacho,0) as Despacho,D.cod_bodega_destino,X.descripcion as BodegaD 
                        from PV_INVTRADET D inner join pv_productos P on D.cod_producto = P.cod_producto 
                        inner join PV_Bodegas B on D.cod_bodega = B.cod_bodega 
                        left join PV_Bodegas X on D.cod_bodega_destino = X.cod_Bodega 
                        where D.boleta = '{CodBoleta}' and D.tipo = '{TipoTran}'";
                    response.Result = connection.Query<InvProducReversion>(query).ToList();
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

        public ErrorDTO<TranReversionData> InvTranReversion_scroll(int CodEmpresa, int scrollValue, string? CodBoleta, string TipoTran)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<TranReversionData>
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
                    response.Result = connection.Query<TranReversionData>(query).FirstOrDefault();
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

        public ErrorDTO InvTranReversion_Insertar(int CodEmpresa, TranReversionInsert request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDTO resp = new()
            {
                Code = 0
            };
            string boletaInverso = string.Empty;
            string tipoInverso = string.Empty;
            string vDestino = string.Empty;
            string vFecha = request.Fecha.ToString();
            switch (request.Tipo)
            {
                case "E":
                    vDestino = "Salida";
                    tipoInverso = "S";
                    break;
                case "S":
                    vDestino = "Entrada";
                    tipoInverso = "E";
                    break;
                case "T":
                    vDestino = "Traslado";
                    tipoInverso = "T";
                    break;
                case "R":
                    vDestino = "Requisicion";
                    tipoInverso = "R";
                    break;
                default:
                    break;
            }
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var queryV = $@"select estado from pv_InvTranSac where Tipo = '{request.Tipo}' and Boleta = '{request.Boleta}'";
                    var verificaEstado = connection.ExecuteScalar<string>(queryV);

                    if (string.IsNullOrEmpty(verificaEstado))
                    {
                        resp.Code = -1;
                        resp.Description = $"No se encontró la boleta '{request.Boleta}', verifique...";
                        return resp;
                    }
                    else if (verificaEstado != "P")
                    {
                        resp.Code = -1;
                        resp.Description = "La Boleta consultada no se encuentra procesada...";
                        return resp;
                    }

                    var periodo = new mProGrX_AuxiliarDB(_config).fxInvPeriodos(CodEmpresa, vFecha);

                    if (periodo == true)
                    {
                        var queryC = $@"select isnull(max(Boleta),0)+1 as Ultimo from pv_InvTranSac where Tipo = '{tipoInverso}'";
                        var consecutivoInverso = connection.Query<string>(queryC).FirstOrDefault();
                        consecutivoInverso = consecutivoInverso.PadLeft(10, '0');
                        boletaInverso = consecutivoInverso.ToString();

                        var query = $@"insert pv_InvTranSac(Boleta,Tipo,cod_entsal,genera_fecha,documento,notas,genera_user,
                        estado,plantilla,fecha,fecha_sistema,autoriza_fecha,autoriza_user,procesa_fecha,procesa_user) 
                        values('{boletaInverso}','{tipoInverso}','{request.Cod_Entsal}',getdate(),'Rev.{request.Boleta}','{request.Notas}',
                        '{request.User}','P',0,'{request.Fecha}',getdate(),getdate(),'{request.User}',getdate(),'{request.User}')";
                        resp.Code = connection.Execute(query);

                        if (resp.Code == 1)
                        {
                            if (request.Tipo == "E" || request.Tipo == "S")
                            {
                                query = $@"insert into pv_invTraDet(Linea,Boleta,Tipo,Cod_Bodega,cod_Producto,Cod_Bodega_destino,cantidad,Precio,despacho)
                                (select Linea,'{boletaInverso}','{tipoInverso}',Cod_Bodega,cod_Producto,Cod_Bodega_destino,cantidad,Precio,cantidad as Desp 
                                From pv_invTraDet Where Tipo = '{request.Tipo}' And Boleta = '{request.Boleta}')";
                                connection.Execute(query);
                            }
                            else if (request.Tipo == "T")
                            {
                                query = $@"insert into pv_invTraDet(Linea,Boleta,Tipo,Cod_Bodega,cod_Producto,Cod_Bodega_destino,cantidad,Precio,despacho)
                                (select Linea,'{boletaInverso}','{tipoInverso}',Cod_Bodega_destino,cod_Producto,Cod_Bodega,cantidad,Precio,cantidad as Desp 
                                From pv_invTraDet Where Tipo = '{request.Tipo}' And Boleta = '{request.Boleta}')";
                                connection.Execute(query);
                            }

                            resp.Description = $"Reversion de '{request.Tipo}', Boleta : '{request.Boleta}' realizada con '{vDestino}' boleta : '{boletaInverso}'";
                        }
                    }
                    else
                    {
                        resp.Code = -1;
                        resp.Description = "El periodo en el que desea realizar el movimiento se encuentra cerrado ...";
                        return resp;
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

    }
}