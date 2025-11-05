using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCprOrdenesAutorizacionDB
    {
        private readonly IConfiguration _config;

        public frmCprOrdenesAutorizacionDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<OrdenCompraDto> OrdenesCompra_Autorizacion_Obtener(int CodEmpresa, int pagina, int paginacion, string? filtro, OrdenCompraRequestDto ordenCompraRequestDto)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<OrdenCompraDto>();
            response.Result = new OrdenCompraDto();

            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = string.Empty;

                    string filtroFecha = string.Empty;

                    if (!ordenCompraRequestDto.todosPendientes)
                    {
                        filtroFecha += $@" and O.genera_fecha between '{ordenCompraRequestDto.fechaInicio}' and '{ordenCompraRequestDto.fechaCorte}' ";
                    }

                    query = $@"select COUNT(O.cod_orden) total from cpr_ordenes O inner join cpr_Tipo_Orden C on O.Tipo_Orden = C.Tipo_Orden 
                                        where O.autoriza_fecha is null and O.estado = 'S' and O.tipo_orden = '{ordenCompraRequestDto.tipo}' and O.genera_user in(
                                        select usuario_asignado from cpr_orden_autousers where usuario = '{ordenCompraRequestDto.usuario}') {filtroFecha}";

                    response.Result.total = connection.QueryFirstOrDefault<int>(query);
                    string vFiltro = "";
                    if (filtro != null)
                    {
                        vFiltro = " WHERE cod_orden LIKE '%" + filtro + "%' OR TipoOrdenDesc LIKE '%" + filtro + "%'" +
                            " OR nota LIKE '%" + filtro + "%'" +
                            " OR proceso LIKE '%" + filtro + "%' ";
                    }

                    if (pagina != null)
                    {
                        paginaActual = " OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"select * from ( select O.cod_orden,C.Descripcion as 'TipoOrdenDesc',O.total,O.genera_user,O.genera_fecha,C.Descripcion as 
                                     TipoOrden,O.nota,O.proceso from cpr_ordenes O inner join cpr_Tipo_Orden C on O.Tipo_Orden = C.Tipo_Orden 
                                        where O.estado = 'S' and O.tipo_orden = '{ordenCompraRequestDto.tipo}' and O.genera_user in(
                                        select usuario_asignado from cpr_orden_autousers where usuario = '{ordenCompraRequestDto.usuario}')
                                        {filtroFecha} ORDER BY O.cod_orden 
                                        {paginaActual}
                                        {paginacionActual} ) T {vFiltro}";


                    response.Result.ordenes = connection.Query<OrdenCompra>(query).ToList();

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

        public ErrorDto OrdenCompra_Autorizar(int CodEmpresa, OrdenCompraResolucionRequestDto ordenCompraRequestDto)
        {
            ErrorDto ErrorDto = new ErrorDto();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                if (!string.IsNullOrEmpty(ordenCompraRequestDto.codigosOrden))
                {
                    string[] codigosOrden = ordenCompraRequestDto.codigosOrden.Split(",");

                    using var connection = new SqlConnection(stringConn);
                    {

                        var queryCodUnidad = $@"select COD_UNIDAD from CPR_SOLICITUD WHERE ADJUDICA_ORDEN = {ordenCompraRequestDto.codigosOrden}";
                        var codunidad = connection.Query<string>(queryCodUnidad).FirstOrDefault();

                        var queryMonto = $@"SELECT TOTAL FROM CPR_ORDENES WHERE COD_ORDEN = {ordenCompraRequestDto.codigosOrden}";
                        var montoColones = connection.Query<int>(queryMonto).FirstOrDefault();

                        var queryTipoCambio = $@"SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = 'TC'";
                        string tipoCambioStr = connection.Query<string>(queryTipoCambio).FirstOrDefault();
                        decimal tipoCambio = Convert.ToDecimal(tipoCambioStr);

                        decimal montoDolares = montoColones / tipoCambio;

                        var queryRangoMontoMinimo = $@"SELECT MONTO_MINIMO FROM cpr_orden_rangos AS r JOIN CPR_RANGO_USUARIO AS u ON r.cod_rango = u.cod_rango
                     WHERE USUARIO = '{ordenCompraRequestDto.usuario}' AND ACTIVO = 1 AND UEN = '{codunidad}'";
                        var montoMinimo = connection.Query<int>(queryRangoMontoMinimo).FirstOrDefault();

                        var queryRangoMontoMaximo = $@"SELECT MONTO_MAXIMO FROM cpr_orden_rangos AS r JOIN CPR_RANGO_USUARIO AS u ON r.cod_rango = u.cod_rango
                     WHERE USUARIO = '{ordenCompraRequestDto.usuario}' AND ACTIVO = 1 AND UEN = '{codunidad}'";
                        var montoMaximo = connection.Query<int>(queryRangoMontoMaximo).FirstOrDefault();

                        if (montoDolares == 0)
                        {
                            ErrorDto.Code = -1;
                            ErrorDto.Description = "El monto de la orden de compra no puede ser 0.";
                            return ErrorDto;
                        }

                        if (montoDolares < montoMinimo || montoDolares > montoMaximo)
                        {
                            ErrorDto.Code = -1;
                            ErrorDto.Description = "El Usuario actual no está dentro del rango para esta Gestión.";
                        }
                        else
                        {


                            var query = string.Empty;
                            foreach (string s in codigosOrden)
                            {
                                var parameters = new DynamicParameters();

                                parameters.Add("codigoOrden", s, DbType.String);
                                parameters.Add("usuario", ordenCompraRequestDto.usuario, DbType.String);

                                query = "update cpr_ordenes set autoriza_fecha = Getdate(), autoriza_user =@usuario, estado = 'A' where cod_orden = @codigoOrden";

                                int res = connection.ExecuteAsync(query, parameters).Result;

                                if (res <= 0)
                                {
                                    ErrorDto.Code = -1;
                                    ErrorDto.Description = "Error al autorizar ordenes de compra";
                                    break;
                                }

                            }
                            ErrorDto.Code = 1;
                            ErrorDto.Description = string.Empty;
                        }
                    }
                }
                else
                    {
                    ErrorDto.Code = -1;
                    ErrorDto.Description = "Error al autorizar ordenes de compra";
                }
            }
            catch (Exception ex)
            {
                ErrorDto.Code = -1;
                ErrorDto.Description = ex.Message;
            }

            return ErrorDto;
        }

        public ErrorDto OrdenCompra_Rechazar(int CodEmpresa, OrdenCompraResolucionRequestDto ordenCompraRequestDto)
        {
            ErrorDto ErrorDto = new ErrorDto();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                if (!string.IsNullOrEmpty(ordenCompraRequestDto.codigosOrden))
                {
                    string[] codigosOrden = ordenCompraRequestDto.codigosOrden.Split(",");

                    using var connection = new SqlConnection(stringConn);
                    {
                        var query = string.Empty;
                        foreach (string s in codigosOrden)
                        {
                            var parameters = new DynamicParameters();

                            parameters.Add("codigoOrden", ordenCompraRequestDto.codigosOrden, DbType.String);
                            parameters.Add("usuario", ordenCompraRequestDto.usuario, DbType.String);

                            query = "update cpr_ordenes set autoriza_fecha = Getdate(), autoriza_user =@usuario, estado = 'R' where cod_orden = @codigoOrden";

                            int res = connection.ExecuteAsync(query, parameters).Result;

                            if (res <= 0)
                            {
                                ErrorDto.Code = -1;
                                ErrorDto.Description = "Error al autorizar ordenes de compra";
                                break;
                            }

                        }
                        ErrorDto.Code = 1;
                        ErrorDto.Description = string.Empty;
                    }
                }
                else
                {
                    ErrorDto.Code = -1;
                    ErrorDto.Description = "Error al autorizar ordenes de compra";
                }
            }
            catch (Exception ex)
            {
                ErrorDto.Code = -1;
                ErrorDto.Description = ex.Message;
            }

            return ErrorDto;
        }
    }
}
