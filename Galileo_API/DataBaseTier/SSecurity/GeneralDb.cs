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
                        List<PadronConsultarResponseDto> respGen = null!;
                        respGen = connection.Query<PadronConsultarResponseDto>(procedure, sincroUsuarioCore, commandType: CommandType.StoredProcedure).ToList();
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
            return resp;
        }

        public ErrorGeneralDto ValidaCuenta(ValidaCuentaRequestDto validaCuentaRequestDto)
        {
            var resp = new ErrorGeneralDto();
            int res = -1;

            var seguridadPortal = new SeguridadPortalDb(_config);
            var pgxClienteDto = seguridadPortal.SeleccionarPgxClientePorCodEmpresa(validaCuentaRequestDto.CodEmpresa);

            var connectionString =
                $"Data Source={pgxClienteDto.PGX_CORE_SERVER};Initial Catalog={pgxClienteDto.PGX_CORE_DB};" +
                $"Integrated Security=False;User Id={pgxClienteDto.PGX_CORE_USER};Password={pgxClienteDto.PGX_CORE_KEY};";

            try
            {
                using var connectionCore = new SqlConnection(connectionString);

                const string query = @"
            SELECT COUNT(*) 
            FROM CntX_cuentas 
            WHERE cod_cuenta = @CodCuenta 
              AND acepta_movimientos = 1 
              AND cod_contabilidad = @CodContabilidad;";

                var param = new
                {
                    CodCuenta = validaCuentaRequestDto.Cuenta,
                    CodContabilidad = 1
                };

                int count = connectionCore.QuerySingle<int>(query, param);

                if (count > 0)
                {
                    const string procedure = "[spSIFValidaCuentas]";
                    var parameters = new { Cuenta = validaCuentaRequestDto.Cuenta };

                    connectionCore.Execute(procedure, parameters, commandType: CommandType.StoredProcedure);
                }

                res = 1;
                resp.Code = res;
                resp.Description = string.Empty;
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
