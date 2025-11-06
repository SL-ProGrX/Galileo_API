using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmCxPCargosAdicionalesDB
    {
        private readonly IConfiguration _config;

        public frmCxPCargosAdicionalesDB(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto<List<CargosAdicionalDto>> ObtenerCargosAdicionales(int CodEmpresa)
        {
            var response = new ErrorDto<List<CargosAdicionalDto>>();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            mCntLinkDB obj = new mCntLinkDB(_config);

            string sql = "select * from CXP_CARGOS order by COD_CARGO";

            try
            {
                using var connection = new SqlConnection(stringConn);
                response.Result = connection.Query<CargosAdicionalDto>(sql).ToList();
                foreach (var item in response.Result)
                {
                    item.Cod_Cuenta = obj.fxgCntCuentaFormato(CodEmpresa, true, item.Cod_Cuenta, 1);
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

        public ErrorDto ExisteCargoAdicional(int CodEmpresa, string CodCargo)
        {
            ErrorDto resp = new()
            {
                Code = 0
            };
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            string sql = "select isnull(count(*),0) as Existe from CXP_CARGOS where COD_CARGO = @CodCargo";
            var values = new
            {
                CodCargo = CodCargo,
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                resp.Code = connection.Query<int>(sql, values).FirstOrDefault();
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto EliminarCargoAdicional(int CodEmpresa, string CodCargo)
        {
            ErrorDto resp = new()
            {
                Code = 0
            };
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            string sql = "delete from CXP_CARGOS where COD_CARGO = @CodCargo";
            var values = new
            {
                CodCargo = CodCargo,
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                resp.Code = connection.Query<int>(sql, values).FirstOrDefault();
                resp.Description = "Cargo eliminado correctamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto InsertarCargoAdicional(int CodEmpresa, CargosAdicionalDto Info)
        {
            ErrorDto resp = new()
            {
                Code = 0
            };
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            string sql = "insert into CXP_CARGOS (COD_CARGO, DESCRIPCION, COD_CUENTA, ACTIVO) values (@COD_CARGO, @DESCRIPCION, @COD_CUENTA, @ACTIVO)";
            var values = new
            {
                COD_CARGO = Info.Cod_Cargo,
                DESCRIPCION = Info.Descripcion,
                COD_CUENTA = Info.Cod_Cuenta.Replace("-", ""),
                ACTIVO = Info.Activo,
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                resp.Code = connection.Query<int>(sql, values).FirstOrDefault();
                resp.Description = "Cargo agregado correctamente";
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }

            return resp;
        }

        public ErrorDto ActualizarCargoAdicional(int CodEmpresa, CargosAdicionalDto Info)
        {
            ErrorDto resp = new()
            {
                Code = 0
            };
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            string sql = "update CXP_CARGOS set DESCRIPCION = @DESCRIPCION, COD_CUENTA = @COD_CUENTA, ACTIVO = @ACTIVO where COD_CARGO = @COD_CARGO";
            var values = new
            {
                DESCRIPCION = Info.Descripcion,
                COD_CUENTA = Info.Cod_Cuenta.Replace("-", ""),
                ACTIVO = Info.Activo,
                COD_CARGO = Info.Cod_Cargo,
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                resp.Code = connection.Query<int>(sql, values).FirstOrDefault();
                resp.Description = "Cargo actualizado correctamente";
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
