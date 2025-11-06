using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.Security;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class GeneralDB
    {
        private readonly IConfiguration _config;

        public GeneralDB(IConfiguration config)
        {
            _config = config;
        }

        public List<PadronConsultarResponseDto> PadronConsultar(PadronConsultarRequestDto padronConsultarDto)
        {
            List<PadronConsultarResponseDto> resp = null!;

            Seguridad_PortalDB seguridadPortal = new Seguridad_PortalDB(_config);

            try
            {

                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                {
                    var procedure = "[spSYS_Consulta_Padron]";

                    var sincroUsuarioCore = new
                    {
                        Identificacion = padronConsultarDto.Identificacion,
                        Pais = padronConsultarDto.Pais,
                        TInfo = padronConsultarDto.TInfo
                    };

                    switch (padronConsultarDto.TInfo)
                    {
                        case "General":
                            //List<PadronGeneralConsultarResponseDto> respGen = null!;
                            List<PadronConsultarResponseDto> respGen = null!;
                            //respGen = connectionCore.Query<PadronGeneralConsultarResponseDto>(procedure, sincroUsuarioCore, commandType: CommandType.StoredProcedure).ToList();
                            respGen = connection.Query<PadronConsultarResponseDto>(procedure, sincroUsuarioCore, commandType: CommandType.StoredProcedure).ToList();
                            //resp = respGen.Cast<PadronConsultarResponseDto>().ToList();
                            resp = respGen;
                            break;
                        case "Telefonos":
                            List<PadronTelefonosConsultarResponseDto> respTel = null!;
                            respTel = connection.Query<PadronTelefonosConsultarResponseDto>(procedure, sincroUsuarioCore, commandType: CommandType.StoredProcedure).ToList();
                            resp = respTel.Cast<PadronConsultarResponseDto>().ToList();
                            break;
                        case "Direccion":
                            List<PadronDireccionesConsultarResponseDto> respDir = null!;
                            respDir = connection.Query<PadronDireccionesConsultarResponseDto>(procedure, sincroUsuarioCore, commandType: CommandType.StoredProcedure).ToList();
                            resp = respDir.Cast<PadronConsultarResponseDto>().ToList();
                            break;
                        case "Empresas":
                            List<PadronEmpresasConsultarResponseDto> respEmp = null!;
                            respEmp = connection.Query<PadronEmpresasConsultarResponseDto>(procedure, sincroUsuarioCore, commandType: CommandType.StoredProcedure).ToList();
                            resp = respEmp.Cast<PadronConsultarResponseDto>().ToList();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                seguridadPortal = null!;
            }
            return resp;
        }

        public ErrorGeneralDto ValidaCuenta(ValidaCuentaRequestDto validaCuentaRequestDto)
        {

            ErrorGeneralDto resp = new ErrorGeneralDto();
            int res = -1;
            PgxClienteDto pgxClienteDto;
            Seguridad_PortalDB seguridadPortal = new Seguridad_PortalDB(_config);

            pgxClienteDto = seguridadPortal.SeleccionarPgxClientePorCodEmpresa(validaCuentaRequestDto.CodEmpresa);
            string nombreServidorCore = pgxClienteDto.PGX_CORE_SERVER;
            string nombreBDCore = pgxClienteDto.PGX_CORE_DB;
            string userId = pgxClienteDto.PGX_CORE_USER;
            string pass = pgxClienteDto.PGX_CORE_KEY;

            string connectionString = $"Data Source={nombreServidorCore};" +
                                  $"Initial Catalog={nombreBDCore};" +
                                  $"Integrated Security=False;User Id={userId};Password={pass};";
            try
            {


                using (var connectionCore = new SqlConnection(connectionString))
                {

                    /*strSQL = "select isnull(count(*),0) as Existe from CntX_cuentas where cod_cuenta = '" +
                 vCuenta.Trim() + "' and acepta_movimientos = 1 and cod_contabilidad = " + GLOBALES.gEnlace;*/

                    //var query = "SELECT * FROM US_USUARIOS WHERE USUARIO = @user";

                    var query = "select isnull(count(*),0) as Existe from CntX_cuentas where cod_cuenta = @CodCuenta and acepta_movimientos = 1 and cod_contabilidad = @CodContabilidad";
                    var param = new
                    {
                        CodCuenta = validaCuentaRequestDto.Cuenta,
                        CodContabilidad = 1
                    };

                    int? resQuery1 = connectionCore.Query<int>(query, param).FirstOrDefault();

                    if (resQuery1 != null && resQuery1 > 0)
                    {
                        var procedure = "[spSIFValidaCuentas]";

                        var parameters = new
                        {
                            Cuenta = validaCuentaRequestDto.Cuenta
                        };
                        res = connectionCore.Execute(procedure, parameters, commandType: CommandType.StoredProcedure);
                    }

                    res = 1;

                    resp.Code = res;
                    resp.Description = string.Empty;

                }
            }
            catch (Exception ex)
            {
                //throw;
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

    }
}
