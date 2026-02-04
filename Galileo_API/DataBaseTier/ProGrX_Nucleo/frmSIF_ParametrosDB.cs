using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.SIF;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier
{
    public class FrmSifParametrosDB
    {
        private readonly IConfiguration _config;
        private readonly int vModulo = 10; // Módulo Núcleo

        public FrmSifParametrosDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtiene los parametros del sistema
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<SifParametrosDto>> obtener_ParametrosSistema(int CodEmpresa)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<SifParametrosDto>>();
            response.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                    connection.Execute("spSIFParametros", commandType: System.Data.CommandType.StoredProcedure);

                    var query = "SELECT cod_parametro, descripcion, valor FROM SIF_PARAMETROS ORDER BY cod_parametro;";
                    response.Result = connection.Query<SifParametrosDto>(query).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }


        /// <summary>
        /// Método actualizar parametros
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public ErrorDto Parametros_Actualizar(int CodEmpresa, string usuario, SifParametrosDto parametros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                     string sql = "UPDATE SIF_PARAMETROS SET valor = @valor WHERE cod_parametro = @codParametro";
                     var parameters = new DynamicParameters();
                     parameters.Add("valor", parametros.valor, DbType.String);
                     parameters.Add("codParametro", parametros.cod_parametro, DbType.String);

                    connection.Execute(sql, parameters);

                    //Bitácora
                    var securityDb = new MSecurityMainDb(_config);
                    securityDb.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Parametro del SIF : {parametros.cod_parametro} - {parametros.valor}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

    }
}