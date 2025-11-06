using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPControlReprogramacionDB
    {
        private readonly IConfiguration _config;
        MSecurityMainDb DBBitacora;

        public frmCxPControlReprogramacionDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

        public ErrorDto<FacturaLista> Facturas_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<FacturaLista>
            {
                Code = 0,
                Result = new FacturaLista()
            };
            response.Result.Total = 0;
            try
            {
                var query = "";
                string paginaActual = " ", paginacionActual = " ";
                using var connection = new SqlConnection(clienteConnString);
                {

                    //Busco Total
                    query = $@"SELECT COUNT(*) from vCxP_ProgramacionPago where cod_proveedor = {Cod_Proveedor} ";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();


                    if (filtro != null)
                    {
                        filtro = " and COD_FACTURA LIKE '%" + filtro + "%'";
                    }

                    paginaActual = " OFFSET " + pagina + " ROWS ";
                    paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";


                    if (Cod_Proveedor > 0)
                    {
                        query = $@"select cod_factura,cod_proveedor,total as total_factura ,fecha,tipo  From vCxP_ProgramacionPago 
                                    WHERE cxp_estado = 'G' AND
                                    cod_proveedor = {Cod_Proveedor}
                                         {filtro} 
                                        order by cod_factura
                                        {paginaActual}
                                        {paginacionActual} ";
                    }


                    response.Result.Facturas = connection.Query<Factura>(query).ToList();

                    foreach (Factura ft in response.Result.Facturas)
                    {
                        ft.DataKey = ft.Cod_Factura + '-' + ft.Cod_Proveedor;
                    }
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

        public ErrorDto<VCxpProgramacionPago> ProgramacionDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<VCxpProgramacionPago>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT *  FROM vCxP_ProgramacionPago WHERE cxp_estado = 'G' AND Cod_Factura = '{Cod_Factura}' AND Cod_Proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<VCxpProgramacionPago>(query).FirstOrDefault();
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

        public ErrorDto<Pago> PagoMontos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<Pago>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT isnull(sum(Monto),0) as Monto, isnull(sum(IMPORTE_DIVISA_REAL),0) as Importe_Real
                                    FROM CxP_PagoPRov 
                                    WHERE Tesoreria IS NULL AND Cod_Factura = '{Cod_Factura}' AND Cod_Proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<Pago>(query).FirstOrDefault();
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

        public ErrorDto FacturaMonto_Ajuste(int CodEmpresa, AjusteFactura data)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = "[spCxP_AjusteMontoFactura]";
                    var values = new
                    {
                        Proveedor = data.Cod_Proveedor,
                        Factura = data.Cod_Factura,
                        Ajuste = data.Monto_Ajuste,
                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";

                    if (resp.Code == 0)
                    {
                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = data.Registro_Usuario,
                            DetalleMovimiento = "Ajuste Monto Factura: " + data.Cod_Factura + " [Prov." + data.Cod_Proveedor + "] Mnt.Ant.: " + data.Monto + " -> Mnt.Nv.: " + data.Monto_Ajuste,
                            Movimiento = "MODIFICA - WEB",
                            Modulo = 30
                        });
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

        public ErrorDto<FacturaDet> FacturaDetalle_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<FacturaDet>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT isnull(max(Npago),0) AS 'Pago', isnull(sum(Monto),0) AS 'Monto'
                                FROM CxP_PagoPRov WHERE Tesoreria IS NOT NULL
                                AND Cod_Factura = '{Cod_Factura}' AND Cod_Proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<FacturaDet>(query).FirstOrDefault();
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

        public ErrorDto<List<CargoAdicional>> CargosAdicionales_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {
            var response = new ErrorDto<List<CargoAdicional>>();

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            string query = $@"SELECT C.Cod_Cargo,C.descripcion,isnull(Sum(Monto),0) AS Monto
                FROM CxP_Cargos C LEFT JOIN cxp_PagoProvCargos D ON C.cod_Cargo = D.cod_Cargo 
                AND D.cod_Proveedor = {Cod_Proveedor} AND D.cod_Factura = '{Cod_Factura}' AND D.NPago > 1
                GROUP BY C.Cod_Cargo, C.descripcion";

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.Query<CargoAdicional>(query).ToList();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;

        }

        public ErrorDto<FacturaDatos> CompraDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<FacturaDatos>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT CxP_Estado,Total,Imp_ventas FROM CPR_COMPRAS WHERE cod_factura = '{Cod_Factura}'
                                    AND cod_proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<FacturaDatos>(query).FirstOrDefault();

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

        public ErrorDto<FacturaDatos> FacturaDatos_Obtener(int CodEmpresa, string Cod_Factura, int Cod_Proveedor)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<FacturaDatos>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT CxP_Estado,Total, 0 AS 'Imp_ventas' 
                                FROM cxp_facturas 
                                WHERE cod_factura = '{Cod_Factura}' AND cod_proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<FacturaDatos>(query).FirstOrDefault();

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

        public ErrorDto CargosPagos_Borrar(int CodEmpresa, int Pago, string Cod_Factura, int Cod_Proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var query = "";

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    query = $@"DELETE cxp_pagoProvCargos WHERE nPago >= {Pago}
                                AND cod_factura = '{Cod_Factura}' AND cod_proveedor = {Cod_Proveedor}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";

                    query = $@"DELETE cxp_pagoProv WHERE nPago >= {Pago}
                                AND cod_factura = '{Cod_Factura}' AND cod_proveedor = {Cod_Proveedor}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
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