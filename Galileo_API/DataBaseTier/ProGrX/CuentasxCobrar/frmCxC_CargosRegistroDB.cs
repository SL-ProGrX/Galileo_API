using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using System.Data;
using System.Data.Common;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCargosRegistrolDb
    {
        private readonly PortalDB _portalDB;

        public FrmCxCCargosRegistrolDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }



        public ErrorDto<string> CxCCargosRegistroCargos_Consultar(int CodEmpresa, string codCargo)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
                        SELECT TOP 1 COD_CUENTA
                        FROM dbo.CxC_CARGOS
                        WHERE cod_cargo = @codCargo;";


                var cuenta = conn.QuerySingleOrDefault<string>(sql, new { codCargo });

                return new ErrorDto<string>
                {
                    Code = 0,
                    Description = cuenta is null ? "No encontrado" : "Ok",
                    Result = cuenta
                };
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<string>(ex.Message);
            }

        }
        public ErrorDto<decimal> CxCCargosRegistroCargoReposicion_Consultar(int CodEmpresa, string operacion)
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

                const string sql = @"
                SELECT dbo.fxCxC_CuentaCargoReposicion(@operacion, NULL) AS Cargo;";

                var monto = conn.QuerySingleOrDefault<decimal>(sql, new { operacion });

                return new ErrorDto<decimal>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = monto
                };
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse<decimal>(ex.Message);
            }

        }
        public ErrorDto<List<DropDownListaGenericaModel>> CxCCargosRegistroCargoAdicionales_Consultar(int CodEmpresa)
        {

            const string sql = @"
        SELECT 
            COD_CARGO     AS item,
            DESCRIPCION   AS descripcion
        FROM dbo.CARGOS_ADICIONALES
        WHERE TIPO = @tipo
        ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDB, CodEmpresa, sql, new { tipo = "M" });

        }



        public ErrorDto CxCCargosRegistroCuentaCargo_Aplicar(int CodEmpresa, CargosRegistroData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string spName = "spCxC_CuentaCargoAdd";
                var cmd = new CommandDefinition(
                    commandText: spName,
                    parameters: new
                    {
                        datos.Operacion,
                        datos.Monto,
                        datos.Unidad,
                        datos.CentroCosto,
                        datos.Detalle,
                        datos.Usuario,
                        datos.Cuenta,
                        datos.CargoCod,
                        datos.Linea

                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60
                );

                conn.Execute(cmd);

                return DbHelper.OkResponse("Cargo registrado satisfactoriamente...");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }



        }
        public ErrorDto CxCCargosRegistroCargoReposicion_Aplicar(int CodEmpresa, CargosRegistroData datos)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string spName = "spCxC_CuentaCargoReposicion";
                var cmd = new CommandDefinition(
                    commandText: spName,
                    parameters: new
                    {
                        datos.Operacion,
                        datos.Usuario,
                        datos.Unidad,
                        datos.CentroCosto
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60
                );

                conn.Execute(cmd);

                return DbHelper.OkResponse("Cargo registrado satisfactoriamente...");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }



        }

    }
}
