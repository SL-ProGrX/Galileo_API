using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPControlPagosDB
    {
        private readonly IConfiguration _config;

        public frmCxPControlPagosDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDTO<List<ControlPagosData>> CxPControlPagos_Obtener(int CodEmpresa, CxPControlPagosParametros pagosParametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<ControlPagosData>>
            {
                Code = 0
            };

            string ColumnaFecha = "";

            switch (pagosParametros.cboFecha)
            {
                case "E": //Fecha de Envio a Tesoreria
                    ColumnaFecha = "C.Fecha_Traslada";
                    break;
                case "V": //Fecha de Vencimiento
                    ColumnaFecha = "C.Fecha_Vencimiento";
                    break;
                case "C": //Fecha de Emisión
                    ColumnaFecha = "T.Fecha_Emision";
                    break;
                default:
                    ColumnaFecha = "C.Fecha_Vencimiento";
                    break;
            }

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.*,P.descripcion as Proveedor,B.descripcion as 'Banco', T.ndocumento
                                from cxp_PagoProv C inner join CxP_Proveedores P on P.cod_proveedor = C.cod_proveedor
                                left join Tes_Transacciones T on C.tesoreria = T.nsolicitud
                                left join Tes_Bancos B on T.id_banco = B.id_Banco
                                where {ColumnaFecha} between '{pagosParametros.fechaInicio} 00:00:00'
                                and '{pagosParametros.fechaCorte} 23:59:59' 
                                and C.Tipo_Cancelacion = '{pagosParametros.tipo_Cancelacion}' ";

                    if (pagosParametros.cboProveedor == false && pagosParametros.codProveedor != 0)
                    {
                        query += $" and C.cod_proveedor = {pagosParametros.codProveedor}";
                    }

                    switch (pagosParametros.cboEstado)
                    {
                        case "S": //Sin enviar a tesoreria
                            query += " and C.tesoreria is null ";
                            break;
                        case "E": //Todas las Enviadas
                            query += " and C.tesoreria is not null";
                            break;
                        case "P": //Pendientes
                            query += " and C.tesoreria is not null and C.TESORERIA_ESTADO = 'P'";
                            break;
                        case "C"://Canceladas
                            query += " and C.tesoreria is not null and C.TESORERIA_ESTADO in('I','T','E') ";
                            break;
                        case "A"://Anuladas
                            query += " and C.tesoreria is not null and C.TESORERIA_ESTADO in('A','N')";
                            break;
                        case "T"://Todas
                            break;
                    }

                    if (pagosParametros.factura != "")
                    {
                        query += $" and C.cod_factura like '%{pagosParametros.factura}%'";
                    }

                    if (pagosParametros.documento != "")
                    {
                        query += $" and T.Ndocumento like '%{pagosParametros.documento}%' ";
                    }

                    if (pagosParametros.noSolicitud != "")
                    {
                        query += $"  and C.Tesoreria = {pagosParametros.noSolicitud} ";
                    }

                    response.Result = connection.Query<ControlPagosData>(query).ToList();

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

        public ErrorDTO<List<ControlPagosResumenData>> CxPCOntrolPagos_Resumen(int CodEmpresa, CxPControlPagosParametros pagosParametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDTO<List<ControlPagosResumenData>>
            {
                Code = 0
            };

            string ColumnaFecha = "";

            switch (pagosParametros.cboFecha)
            {
                case "E": //Fecha de Envio a Tesoreria
                    ColumnaFecha = "C.Fecha_Traslada";
                    break;
                case "V": //Fecha de Vencimiento
                    ColumnaFecha = "C.Fecha_Vencimiento";
                    break;
                case "C": //Fecha de Emisión
                    ColumnaFecha = "T.Fecha_Emision";
                    break;
                default:
                    ColumnaFecha = "C.Fecha_Vencimiento";
                    break;
            }

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select count(*) as Pagos, sum(C.monto) as Monto,P.descripcion as Proveedor, P.cod_proveedor 
                                     from cxp_PagoProv C inner join CxP_Proveedores P on P.cod_proveedor = C.cod_proveedor
                                     left join Tes_Transacciones T on C.tesoreria = T.nsolicitud 
                                     left join Tes_Bancos B on T.id_banco = B.id_Banco
                                     where {ColumnaFecha} between '{pagosParametros.fechaInicio} 00:00:00'
                                           and '{pagosParametros.fechaCorte} 23:59:59'
                                           and C.Tipo_Cancelacion = '{pagosParametros.tipo_Cancelacion}' ";

                    if (pagosParametros.cboProveedor == false && pagosParametros.codProveedor != 0)
                    {
                        query += $" and C.cod_proveedor = {pagosParametros.codProveedor}";
                    }

                    switch (pagosParametros.cboEstado)
                    {
                        case "S": //Sin enviar a tesoreria
                            query += " and C.tesoreria is null ";
                            break;
                        case "E": //Todas las Enviadas
                            query += " and C.tesoreria is not null";
                            break;
                        case "P": //Pendientes
                            query += " and C.tesoreria is not null and C.TESORERIA_ESTADO = 'P'";
                            break;
                        case "C"://Canceladas
                            query += " and C.tesoreria is not null and C.TESORERIA_ESTADO in('I','T','E') ";
                            break;
                        case "A"://Anuladas
                            query += " and C.tesoreria is not null and C.TESORERIA_ESTADO in('A','N')";
                            break;
                        case "T"://Todas
                            break;
                    }

                    if (pagosParametros.factura != "")
                    {
                        query += $" and C.cod_factura like '%{pagosParametros.factura}%'";
                    }

                    if (pagosParametros.documento != "")
                    {
                        query += $" and T.Ndocumento like '%{pagosParametros.documento}%' ";
                    }

                    if (pagosParametros.noSolicitud != "")
                    {
                        query += $"  and C.Tesoreria = {pagosParametros.noSolicitud} ";
                    }
                    query += $"   group by P.cod_proveedor,P.descripcion ";
                    response.Result = connection.Query<ControlPagosResumenData>(query).ToList();
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
