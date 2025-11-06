using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CPR;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.INV;

namespace PgxAPI.DataBaseTier
{
    public class frmInvRepGeneralDB
    {
        private readonly IConfiguration _config;

        public frmInvRepGeneralDB(IConfiguration config)
        {
            _config = config;
        }


        public ErrorDto<List<BodegaReporteInvDto>> Obtener_Bodegas(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<BodegaReporteInvDto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT COD_BODEGA, DESCRIPCION FROM PV_BODEGAS";

                    response.Result = connection.Query<BodegaReporteInvDto>(query).ToList();

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

        public ErrorDto<List<UnidadesReporteInvDto>> Obtener_Unidades(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<UnidadesReporteInvDto>>();

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT COD_UNIDAD, DESCRIPCION from PV_UNIDADES";

                    response.Result = connection.Query<UnidadesReporteInvDto>(query).ToList();

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

        public ErrorDto<List<DepartamentoReporteInvDto>> Obtener_Departamento(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<DepartamentoReporteInvDto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT COD_DEPARTAMENTO, DESCRIPCION FROM  PV_DEPARTAMENTOS";

                    response.Result = connection.Query<DepartamentoReporteInvDto>(query).ToList();

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

        public ErrorDto<List<ProveedoresInvDto>> Obtener_Proveedor(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<ProveedoresInvDto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "SELECT COD_PROVEEDOR, DESCRIPCION FROM  CXP_PROVEEDORES";

                    response.Result = connection.Query<ProveedoresInvDto>(query).ToList();

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

        public ErrorDto<List<LineasInvDto>> Obtener_Lineas(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<List<LineasInvDto>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = "select COD_PRODCLAS,DESCRIPCION from PV_PROD_CLASIFICA";

                    response.Result = connection.Query<LineasInvDto>(query).ToList();

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


        public ErrorDto<List<CprUensLista>> CprUens_Obtener(int CodEmpresa, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CprUensLista>>();

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select R.COD_UNIDAD item, U.DESCRIPCION, 
               (select TOP 1 DESCRIPCION from CNTX_UNIDADES WHERE COD_UNIDAD = U.CNTX_UNIDAD) AS CNTX_UNIDAD,
               (select TOP 1 DESCRIPCION from CNTX_CENTRO_COSTOS WHERE COD_CENTRO_COSTO = U.CNTX_CENTRO_COSTO) AS CNTX_CENTRO_COSTO
               FROM CORE_UENS_USUARIOS_ROLES R LEFT JOIN CORE_UENS U 
               ON R.COD_UNIDAD = U.COD_UNIDAD WHERE R.CORE_USUARIO = '{usuario}'";
                    response.Result = connection.Query<CprUensLista>(query).ToList();
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