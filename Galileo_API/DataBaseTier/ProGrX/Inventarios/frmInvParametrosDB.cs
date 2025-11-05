using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvParametrosDB
    {
        private readonly IConfiguration _config;

        public frmInvParametrosDB(IConfiguration config)
        {
            _config = config;
        }



        public ErrorDto<ParametrosGenDTO> Parametros_Obtener(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<ParametrosGenDTO>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query =
                    $@"SELECT * FROM PV_PARAMETROS_GEN";

                    response.Result = connection.Query<ParametrosGenDTO>(query).FirstOrDefault();
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

        public ErrorDto<List<CntX_ContaDTO>> obtenerContabilidades(int CodEmpresa)
        {

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<CntX_ContaDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"SELECT * FROM CntX_Contabilidades";

                    response.Result = connection.Query<CntX_ContaDTO>(query).ToList();
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
        public ErrorDto actualizar_Parametros(int CodEmpresa, ParametrosGenDTO data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            ErrorDto resp = new ErrorDto();
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    var query = $@"UPDATE PV_PARAMETROS_GEN SET
                                            Cta_Comisiones = '{data.Cta_Comisiones}',
                                            Cta_Imp_Renta = '{data.Cta_Imp_Renta}',
                                            Cta_Imp_Consumo = '{data.Cta_Imp_Consumo}',
                                            Cta_Gastos = '{data.Cta_Gastos}',
                                            Cta_Costo_Ventas = '{data.Cta_Costo_Ventas}',
                                            Cta_Recibos = '{data.Cta_Recibos}',
                                            Cta_Notas = '{data.Cta_Notas}',
                                            Cta_Ventas_Ing = '{data.Cta_Ventas_Ing}',
                                            Ta_Factura_Man = '{data.Ta_Factura_Man}',
                                            Ta_Factura_Auto = '{data.Ta_Factura_Auto}',
                                            Ta_Entradas = '{data.Ta_Entradas}',
                                            Ta_Salidas = '{data.Ta_Salidas}',
                                            Ta_Traslados = '{data.Ta_Traslados}',
                                            Ta_Devoluciones = '{data.Ta_Devoluciones}',
                                            Ta_Nc = '{data.Ta_Nc}',
                                            Ta_Recibos = '{data.Ta_Recibos}',
                                            Ta_Nd = '{data.Ta_Nd}',
                                            Ta_Gen = '{data.Ta_Gen}',
                                            Enlace_Conta = '{data.Enlace_Conta}',
                                            Enlace_Sif = '{data.Enlace_Sif}'
                                            WHERE COD_PAR = {data.Cod_Par};";

                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    resp.Description = "Ok";


                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        public ErrorDto<List<DescripcionCuentasDTO>> Obtener_DescripcionesCuenta(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<DescripcionCuentasDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select Cod_Cuenta, Descripcion from CNTX_CUENTAS";

                    response.Result = connection.Query<DescripcionCuentasDTO>(query).ToList();

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
        public ErrorDto<List<DescripcionTipoAsientoDTO>> Obtener_DescripcionesAsiento(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DescripcionTipoAsientoDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select Tipo_Asiento, Descripcion from CntX_Tipos_Asientos";

                    response.Result = connection.Query<DescripcionTipoAsientoDTO>(query).ToList();

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

        public ErrorDto<List<DescripcionTipoAsientoDTO>> Asientos_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DescripcionTipoAsientoDTO>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select * from CNTX_TIPOS_ASIENTOS";

                    response.Result = connection.Query<DescripcionTipoAsientoDTO>(query).ToList();
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