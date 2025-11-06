using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CxP;
using System.Data;
using PgxAPI.Models.ERROR;
using Newtonsoft.Json;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPAnticiposDB
    {

        private readonly IConfiguration _config;

        public frmCxPAnticiposDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDto ExeAnticipos(int CodCliente, string filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            CxpAnticiposFiltros filtro = JsonConvert.DeserializeObject<CxpAnticiposFiltros>(filtros) ?? new CxpAnticiposFiltros();
            ErrorDto response = new()
            {
                Code = 0
            };
            
            try
            {
                string procedure = "spCxP_Anticipos";
                var values = new
                {
                    Proveedor = filtro.proveedor,
                    CargoCod = filtro.cargoCod,
                    Monto = filtro.monto,
                    Divisa = filtro.divisa,
                    Documento = filtro.documento,
                    Notas = filtro.notas,
                    Usuario = filtro.usuario,
                    FechaCargo = filtro.fechaCargo,

                };

                using var connection = new SqlConnection(stringConn);
                {
                    response.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    response.Description = "Los datos han sido guardados satisfactoriamente!";
                }

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;

        }

        public ErrorDto<List<CargoDto>> ObtenerCargos(int CodCliente)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<CargoDto>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select COD_CARGO, DESCRIPCION, 0 as MONTO from CXP_CARGOS where ACTIVO = 1";
                    response.Result = connection.Query<CargoDto>(query).ToList();
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

        public ErrorDto<List<AdelantoRegistradoDto>> ObtenerAdelantosRegistrados(int CodCliente, int CodProveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AdelantoRegistradoDto>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select A.*,P.tesoreria,P.fecha_vencimiento,C.descripcion as Cargo," +
                    "dbo.fxCxP_CargoFlotanteSaldoCorte(A.cod_Proveedor,A.ID_Cargo, Getdate()) as 'Saldo' " +
                    "from cxp_anticipos A left join cxp_pagoProv P on A.cod_proveedor = P.cod_proveedor and A.Anticipos " +
                    "= P.cod_factura inner join CxP_Cargos C on A.cod_cargo = C.cod_cargo where A.cod_proveedor " +
                    "= @CodProveedor order by Fecha desc";
                    var values = new
                    {
                        CodProveedor = CodProveedor,
                    };
                    response.Result = connection.Query<AdelantoRegistradoDto>(query, values).ToList();
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

        public ErrorDto<List<HistorialPagoDto>> ObtenerHistorialDePagos(int CodCliente, int CodProveedor, string Anticipos)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<HistorialPagoDto>>
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select A.ANTICIPOS, P.COD_PROVEEDOR, P.COD_FACTURA, P.REGISTRO_FECHA, REGISTRO_USUARIO, P.MONTO , P.COD_DIVISA, P.TIPO_CAMBIO, P.NPAGO from CXP_ANTICIPOS A inner join CXP_PAGOPROVCARGOS P on A.ID_CARGO = P.[ID] AND A.COD_PROVEEDOR = P.COD_PROVEEDOR where A.COD_PROVEEDOR = @CodProveedor and A.ANTICIPOS = @Anticipos";
                    var values = new
                    {
                        CodProveedor = CodProveedor,
                        Anticipos = Anticipos,
                    };
                    response.Result = connection.Query<HistorialPagoDto>(query, values).ToList();
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

        public ErrorDto<List<Proveedor>> ObtenerProveedores(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<Proveedor>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "select * from pv_parametros_mod";
                    response.Result = connection.Query<Proveedor>(query).ToList();
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


        public ErrorDto ConsecutivoAdelanto(int CodEmpresa, int Proveedor)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"select (isnull(max(IDX),0) + 1) as Consecutivo from cxp_anticipos where cod_proveedor = {Proveedor}";
                    response.Code = connection.Query<int>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


    }//end class
}//end namespace
