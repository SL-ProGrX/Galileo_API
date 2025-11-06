using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPProvCargoPerDB
    {

        private readonly IConfiguration _config;
        MSecurityMainDb DBBitacora;

        public frmCxPProvCargoPerDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new MSecurityMainDb(_config);
        }

        public ErrorDto<List<Secuencia>> Secuencias_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<Secuencia>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT TOP 200 [ID], registro_fecha, registro_usuario, saldo
                                FROM cxp_cargosPer  WHERE COD_PROVEEDOR = {Cod_Proveedor}
                                order by  [id] desc";

                    response.Result = connection.Query<Secuencia>(query).ToList();

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

        public ErrorDto<List<Cargo>> Cargos_Obtener(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<Cargo>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select trim(cod_cargo) as cod_cargo,descripcion from cxp_cargos";

                    response.Result = connection.Query<Cargo>(query).ToList();

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

        public ErrorDto<CargoPerDto> CargoDetalle_Obtener(int CodEmpresa, int Cod_Proveedor, int Id)
        {

            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<CargoPerDto>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select C.*, isnull(C.Fecha_Cobro_Cargo, C.registro_Fecha) as FechaInicioCobro
                                ,P.descripcion  as Proveedor,D.descripcion as Cargo_Desc
                                 from cxp_proveedores P inner join cxp_cargosper C on P.cod_proveedor = C.cod_proveedor
                                 inner join cxp_cargos D on C.cod_cargo = D.cod_cargo
                                 where C.ID = {Id} and C.cod_proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<CargoPerDto>(query).FirstOrDefault();

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

        public ErrorDto<ProveedorInfo> ProveedorDetalle_Obtener(int CodEmpresa, int Cod_Proveedor)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<ProveedorInfo>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Top 1 cod_proveedor,descripcion,cod_divisa,saldo
                                ,dbo.fxCntXTipoCambio(1,COD_DIVISA,Getdate(),'V') as 'Tipo_Cambio'
                                 from cxp_proveedores where cod_proveedor = {Cod_Proveedor}";

                    response.Result = connection.Query<ProveedorInfo>(query).FirstOrDefault();

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

        public ErrorDto<CargoPerDtoList> CargosPer_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<CargoPerDtoList>
            {
                Code = 0,
                Result = new CargoPerDtoList()
            };
            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT COUNT(C.cod_proveedor)
                                FROM cxp_cargosper C INNER JOIN cxp_cargos D ON C.cod_cargo = D.cod_cargo
								WHERE C.cod_proveedor = '{Cod_Proveedor}'";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND (C.cod_Cargo LIKE '%" + filtro + "%' OR D.descripcion LIKE '%" + filtro + "%' OR C.concepto LIKE '%" + filtro + "%' OR C.id LIKE '%" + filtro + "%')";
                    }
                    if (pagina != null)
                    {
                        paginaActual = "OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"SELECT C.*,D.descripcion AS Cargo_Desc
                                FROM cxp_cargosper C INNER JOIN cxp_cargos D ON C.cod_cargo = D.cod_cargo
                                WHERE C.cod_proveedor = {Cod_Proveedor} {filtro} ORDER BY C.ID desc
                                {paginaActual} {paginacionActual}";

                    response.Result.Cargoper = connection.Query<CargoPerDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Cargoper = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDto<PagoProvCargosDtoList> Pagos_Obtener(int CodEmpresa, int Cod_Proveedor, int Id, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<PagoProvCargosDtoList>
            {
                Code = 0,
                Result = new PagoProvCargosDtoList()
            };
            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT COUNT(C.IDX_CONSEC)
                                FROM cxp_pagoprov P INNER JOIN cxp_pagoprovcargos C
                                ON P.npago = C.npago AND P.cod_proveedor = C.cod_proveedor
                                AND P.cod_factura = C.cod_factura AND P.tesoreria IS NOT NULL
                                WHERE C.id = {Id} AND C.cod_proveedor = {Cod_Proveedor}";
                    response.Result.Total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND (C.cod_Cargo LIKE '%" + filtro + "%' OR C.IDX_CONSEC LIKE '%" + filtro + "%' OR C.concepto LIKE '%" + filtro + "%' OR C.id LIKE '%" + filtro + "%')";
                    }
                    if (pagina != null)
                    {
                        paginaActual = "ORDER BY C.id desc OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }
                    query = $@"SELECT C.*,P.fecha_Traslada,P.tesoreria
                                FROM cxp_pagoprov P INNER JOIN cxp_pagoprovcargos C
                                ON P.npago = C.npago AND P.cod_proveedor = C.cod_proveedor
                                AND P.cod_factura = C.cod_factura AND P.tesoreria IS NOT NULL
                                WHERE C.id = {Id} AND C.cod_proveedor = {Cod_Proveedor}";

                    response.Result.Pagos = connection.Query<PagoProvCargosDto>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result.Pagos = null;
                response.Result.Total = 0;
            }
            return response;
        }

        public ErrorDto Cargo_Actualizar(int CodEmpresa, CargoPerDto data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE cxp_cargosper SET 
                                detalle = '{data.Detalle}'
                                ,concepto =  '{data.Concepto}'
                                ,Fecha_Cobro_Cargo = '{data.FechaInicioCobro}'
                                ,Vence = '{data.Vence}'
                                WHERE id = {data.Id} AND cod_proveedor = {data.Cod_Proveedor}";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Cargo actualizado correctamente";

                    if (resp.Code == 0)
                    {
                        Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = data.Usuario_Sesion,
                            DetalleMovimiento = "Cargo Adicional a Prov: " + data.Cod_Proveedor + " Sec: " + data.Id,
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

        public ErrorDto Cargo_Insertar(int CodEmpresa, CargoPerDto data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var query = string.Empty;
            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    query = $@"SELECT ISNULL(MAX(id),0) + 1 FROM cxp_cargosper WHERE cod_proveedor = {data.Cod_Proveedor}";

                    int siguiente = connection.Query<int>(query).FirstOrDefault();


                    query = $@"INSERT cxp_cargosper(id,cod_proveedor,cod_cargo,tipo,valor,vence,saldo,concepto,detalle,recaudado
                            ,importe_divisa_real,registro_fecha,registro_usuario,cod_divisa,tipo_cambio, fecha_cobro_cargo)
                            values({siguiente},{data.Cod_Proveedor},'{data.Cod_Cargo}','{data.Tipo}',{data.Valor},'{data.Vence}',{data.Valor},'{data.Concepto}','{data.Detalle}', {0},
                            {data.Valor / data.Tipo_Cambio},'{DateTime.Now}','{data.Registro_Usuario}','{data.Cod_Divisa}',{data.Tipo_Cambio},'{data.Fecha_Cobro_Cargo}')";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();

                    if (resp.Code == 0)
                    {

                        query = $@"UPDATE cxp_proveedores
                                SET saldo = isnull(saldo, 0) - {data.Valor},
                                SALDO_DIVISA_REAL = isnull(SALDO_DIVISA_REAL, 0) -  {data.Valor / data.Tipo_Cambio}
                                where cod_proveedor = {data.Cod_Proveedor}";

                        resp.Code = connection.Query<int>(query).FirstOrDefault();
                        resp.Description = "Cargo agregado correctamente";

                        if (resp.Code == 0)
                        {
                            Bitacora(new BitacoraInsertarDto
                            {
                                EmpresaId = CodEmpresa,
                                Usuario = data.Registro_Usuario,
                                DetalleMovimiento = "Cargo Adicional a Prov: " + data.Cod_Proveedor + " Sec: " + siguiente,
                                Movimiento = "REGISTRA - WEB",
                                Modulo = 30
                            });
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

        public ErrorDto Cargo_Eliminar(int CodEmpresa, CargoPerDto data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"DELETE cxp_cargosper WHERE cod_proveedor = {data.Cod_Proveedor}
                                   AND id = {data.Id} AND recaudado = 0 ";


                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Cargo eliminado correctamente";

                    if (resp.Code == 0)
                    {
                        query = $@"UPDATE cxp_proveedores
                                SET saldo = isnull(saldo, 0) + {data.Valor},
                                SALDO_DIVISA_REAL = isnull(SALDO_DIVISA_REAL, 0) +  {data.Valor / data.Tipo_Cambio}
                                where cod_proveedor = {data.Cod_Proveedor}";

                        resp.Code = connection.Query<int>(query).FirstOrDefault();
                        resp.Description = "Registro actualizado correctamente";

                        if (resp.Code == 0)
                        {
                            Bitacora(new BitacoraInsertarDto
                            {
                                EmpresaId = CodEmpresa,
                                Usuario = data.Usuario_Sesion,
                                DetalleMovimiento = "Cargo Adicional a Prov: " + data.Cod_Proveedor + " Sec: " + data.Id + "..Mnt..:" + data.Valor,
                                Movimiento = "ELIMINA - WEB",
                                Modulo = 30
                            });
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

        public ErrorDto Bitacora(BitacoraInsertarDto data)
        {
            return DBBitacora.Bitacora(data);
        }

    }
}
