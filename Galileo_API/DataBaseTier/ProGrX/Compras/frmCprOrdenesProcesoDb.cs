using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCprOrdenesProcesoDB
    {
        private readonly IConfiguration _config;

        public frmCprOrdenesProcesoDB(IConfiguration config)
        {
            _config = config;
        }
        /// <summary>
        /// Obtiene la lista de ordenes de compra por proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOrden"></param>
        /// <returns></returns>
        public ErrorDto<List<ProveedorOrdenesData>> ProveedorOrden_Obtener(int CodEmpresa, string CodOrden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<ProveedorOrdenesData>>();
            try
            {

                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"select Prov.COD_PROVEEDOR, Prov.DESCRIPCION, Op.*
                                    from CPR_ORDENES O inner join CPR_ORDENES_PROCESO Op on O.COD_ORDEN = Op.COD_ORDEN
                                    inner join CXP_PROVEEDORES Prov on Op.cod_proveedor = Prov.COD_PROVEEDOR 
                                  WHERE O.COD_ORDEN = '{CodOrden}' ";
                    response.Result = connection.Query<ProveedorOrdenesData>(query).ToList();
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
        /// Obtiene la lista de ordenes de compra por proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        public ErrorDto Cpr_Orden_Proceso(int CodEmpresa, OrdenProceso orden)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = "[spCpr_Orden_Proceso]";
                    var values = new
                    {
                        Orden = orden.cod_orden,
                        Usuario = orden.genera_user,
                        Movimiento = orden.funcion,
                        Proveedor = orden.cod_proveedor,
                        Notas = ""
                    };

                    var SqlInfo = connection.Query(procedure, values, commandType: CommandType.StoredProcedure);

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
        /// Reemplaza el pin de la orden de compra
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pinIngreso"></param>
        /// <param name="pin"></param>
        /// <param name="CodOrden"></param>
        /// <returns></returns>
        public ErrorDto OrdenProceso_ReemplazarPin(int CodEmpresa, bool pinIngreso, string pin, string CodOrden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            int ingreso_pin = pinIngreso ? 1 : 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update cpr_ordenes set pin_entrada = '{pin}',pin_autorizacion = {ingreso_pin} 
                                    where cod_orden = '{CodOrden}' and proceso not in('D','X') ";
                    var respuesta = connection.Execute(query);
                    if (respuesta == 0)
                    {
                        resp.Description = "Pin reemplazado Satisfactoriamente...";
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
        /// Autoriza o Rechaza la orden de compra
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOrden"></param>
        /// <param name="Usuario"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public ErrorDto Orden_Autoriza(int CodEmpresa, string CodOrden, string Usuario, int Index)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var queryCodUnidad = $@"select COD_UNIDAD from CPR_SOLICITUD WHERE ADJUDICA_ORDEN = {CodOrden}";
                    var codunidad = connection.Query<string>(queryCodUnidad).FirstOrDefault();

                    var queryMonto = $@"SELECT TOTAL FROM CPR_ORDENES WHERE COD_ORDEN = {CodOrden}";
                    var montoColones = connection.Query<int>(queryMonto).FirstOrDefault();

                    var queryTipoCambio = $@"SELECT VALOR FROM SIF_PARAMETROS WHERE COD_PARAMETRO = 'TC'";
                    string tipoCambioStr = connection.Query<string>(queryTipoCambio).FirstOrDefault();
                    decimal tipoCambio = Convert.ToDecimal(tipoCambioStr);

                    decimal montoDolares = montoColones / tipoCambio;

                    var queryRangoMontoMinimo = $@"SELECT MONTO_MINIMO FROM cpr_orden_rangos AS r JOIN CPR_RANGO_USUARIO AS u ON r.cod_rango = u.cod_rango
                     WHERE USUARIO = '{Usuario}' AND ACTIVO = 1 AND UEN = '{codunidad}'";
                    var montoMinimo = connection.Query<int>(queryRangoMontoMinimo).FirstOrDefault();

                    var queryRangoMontoMaximo = $@"SELECT MONTO_MAXIMO FROM cpr_orden_rangos AS r JOIN CPR_RANGO_USUARIO AS u ON r.cod_rango = u.cod_rango
                     WHERE USUARIO = '{Usuario}' AND ACTIVO = 1 AND UEN = '{codunidad}'";
                    var montoMaximo = connection.Query<int>(queryRangoMontoMaximo).FirstOrDefault();

                    var queryRangos = $@"
                            SELECT MONTO_MINIMO, MONTO_MAXIMO 
                            FROM cpr_orden_rangos AS r 
                            JOIN CPR_RANGO_USUARIO AS u ON r.cod_rango = u.cod_rango
                            WHERE USUARIO = '{Usuario}' AND ACTIVO = 1 AND UEN = '{codunidad}'";

                    var rangos = connection.Query<(int MONTO_MINIMO, int MONTO_MAXIMO)>(queryRangos).ToList();

                    if (montoDolares == 0)
                    {
                        resp.Code = 1;
                        resp.Description = "El monto de la orden de compra no puede ser 0.";
                        return resp;
                    }

                    bool dentroDeRango = false;

                    foreach (var rango in rangos)
                    {
                        if (montoDolares >= rango.MONTO_MINIMO && montoDolares <= rango.MONTO_MAXIMO)
                        {
                            dentroDeRango = true;
                            break;
                        }
                    }

                    if (!dentroDeRango)
                    {
                        resp.Code = 1;
                        resp.Description = "El Usuario actual no está dentro del rango para esta Gestión.";
                        return resp;
                    }
                    else { 


                    //Verifica que el usuario tenga autorizacion para anular la orden
                    var query = $@"select count(*) as 'Existe'  
                            from cpr_ordenes O inner join cpr_Tipo_Orden C on O.Tipo_Orden = C.Tipo_Orden 
                             where O.autoriza_fecha is null and O.estado = 'S' 
                            and O.genera_user in( select usuario_asignado from cpr_orden_autousers where usuario = '{Usuario}' )";
                    var msj = connection.Query<int>(query).FirstOrDefault();

                    if (msj == 0)
                    {
                        resp.Code = 1;
                        resp.Description = "El Usuario actual no está Autorizado para esta Gestión";
                    }
                    else
                    {
                        if (Index == 0)
                        {
                            query = $@"update cpr_ordenes set autoriza_fecha = Getdate(),autoriza_user = '{Usuario}' 
                                 ,estado = 'A' where cod_orden = '{CodOrden}' ";
                        }
                        else
                        {
                            query = $@"update cpr_ordenes set autoriza_fecha = Getdate(),autoriza_user = '{Usuario}' 
                                 ,estado = 'R' where cod_orden = '{CodOrden}' ";
                        }

                        var respuesta = connection.Execute(query);

                        if (respuesta == 0)
                        {
                            resp.Description = "Orden Autorizada Satisfactoriamente...";
                        }
                    }
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
        /// Cierra la orden de compra
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodOrden"></param>
        /// <returns></returns>
        public ErrorDto Orden_Cerrar(int CodEmpresa, string CodOrden)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"update CPR_Ordenes set Proceso = 'Y' 
                                    where cod_Orden = '{CodOrden}' and Estado = 'A' and Proceso in('A','D','X') ";
                    var respuesta = connection.Execute(query);
                    if (respuesta == 0)
                    {
                        resp.Description = "Orden Cerrada satisfactoriamente!";
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
        /// Obtiene el estado del proveedor
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodProveedor"></param>
        /// <returns></returns>
        public ErrorDto ProveedorEstado_Obtener(int CodEmpresa, int CodProveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select Estado from CxP_Proveedores where cod_proveedor = {CodProveedor} ";
                    resp.Description = connection.Query<string>(query).FirstOrDefault();
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
