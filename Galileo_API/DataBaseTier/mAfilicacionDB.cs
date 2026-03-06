using Dapper;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class MAfilicacionDB
    {
        private readonly IConfiguration _config;

        public MAfilicacionDB(IConfiguration config)
        {
            _config = config;
        }

        public string fxgAFIParametroComision(int CodEmpresa, string pCodigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string result = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                 
                    var query = $@"select valor from AFI_COMISIONES_PARAMETROS where cod_parametro = @codigo";
                    result = connection.QueryFirstOrDefault<string>(query, new { codigo = pCodigo }) ?? "";
                
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public string fxNombre(int CodEmpresa, string strCedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string result = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                
                    var query = $@"select nombre from socios where cedula = @cedula";
                    result = connection.QueryFirstOrDefault<string>(query, new { cedula = strCedula }) ?? "";
                
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }
    
        public bool fxgCongelamiento(int CodEmpresa, string vCedula, string vParametro)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                var query = "";
                switch (vParametro)
                {
                    case "per_abono_cajas":
                        query = $@"select isnull(count(*),0) as Existe
                                       from afi_congelar where estado = 'A'
                                       and per_abono_cajas = 0 and cedula = @vCedula and dbo.MyGetdate() 
                                between fecha_inicia and fecha_finaliza";
                        break;
                    case "VALOR_CUOTA":
                        query = $@"select isnull(count(*),0) as Existe
                                       from afi_congelar where estado = 'A'
                                       and VALOR_CUOTA = 0 and cedula = @vCedula and dbo.MyGetdate() 
                                between fecha_inicia and fecha_finaliza";
                        break;
                    default:
                        return false;
                }
                var existe = connection.QueryFirstOrDefault<int>(query, new {vCedula = vCedula });
                if (existe > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string fxgAFIParametro(int CodEmpresa, string pCodigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string result = "";
            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = $@"select valor from afi_parametros where cod_parametro = @codigo";
                result = connection.QueryFirstOrDefault<string>(query, new { codigo = pCodigo }) ?? "";

            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        public void sbgAFIBitacora(int CodEmpresa, string pMovimiento,  string pDetalle, string pCedula, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);

                var query = $@"exec spAFI_Persona_Bitacora_Especial_Add @Cedula, @Movimiento,  @Detalle,  @Usuario";
                connection.Execute(query, new
                {
                    Cedula = pCedula,
                    Movimiento = pMovimiento,
                    Detalle = pDetalle,
                    Usuario = usuario
                });

            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }
    }
}
