using System.Data;
using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;
using Galileo.Models.Security;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class FrmSifParametrosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityDb;
        private readonly int vModulo = 10; // Módulo Núcleo

        public FrmSifParametrosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityDb = new MSecurityMainDb(config);
        }

        private ErrorDto WithEmpresaConn(int codEmpresa, Action<SqlConnection> action, string okMsg = "OK")
        {
            try
            {
                using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
                action(conn);
                return DbHelper.OkResponse(okMsg);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message ?? "Error inesperado");
            }
        }

        public ErrorDto<List<SifParametrosDto>> obtener_ParametrosSistema(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, connection =>
            {
                connection.Execute("spSIFParametros", commandType: CommandType.StoredProcedure);

                const string query = "SELECT cod_parametro, descripcion, valor FROM SIF_PARAMETROS ORDER BY cod_parametro;";
                return connection.Query<SifParametrosDto>(query).ToList();
            });
        }

        public ErrorDto Parametros_Actualizar(int CodEmpresa, string usuario, SifParametrosDto parametros)
        {
            return WithEmpresaConn(CodEmpresa, connection =>
            {
                const string sql = "UPDATE SIF_PARAMETROS SET valor = @valor WHERE cod_parametro = @codParametro";
                var parameters = new DynamicParameters();
                parameters.Add("valor", parametros.valor, DbType.String);
                parameters.Add("codParametro", parametros.cod_parametro, DbType.String);

                connection.Execute(sql, parameters);

                // Bitácora
                _securityDb.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = CodEmpresa,
                    Usuario = usuario,
                    DetalleMovimiento = $"Parametro del SIF : {parametros.cod_parametro} - {parametros.valor}",
                    Movimiento = "Modifica - WEB",
                    Modulo = vModulo
                });
            }, "Registro actualizado satisfactoriamente");
        }

    }
}