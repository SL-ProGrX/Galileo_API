using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using Galileo.Models.PRE;

namespace Galileo.DataBaseTier
{
    public class FrmPresParametrosDb
    {
        private readonly IConfiguration _config;

        public FrmPresParametrosDb(IConfiguration config)
        {
            _config = config;
        }

        #region Helpers

        private SqlConnection CreateConnection(int codEmpresa)
        {
            var stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            return new SqlConnection(stringConn);
        }

        #endregion

        /// <summary>
        /// Método para insertar o actualizar parámetros
        /// </summary>
        public ErrorDto PresParametros_Guardar(int codEmpresa, PresParametrosDto parametros)
        {
            var response = new ErrorDto
            {
                Code = 0
            };

            const string selectSql = @"
                SELECT 1
                FROM PRES_PARAMETROS
                WHERE COD_PARAMETRO = @CodParametro;";

            const string insertSql = @"
                INSERT INTO [dbo].[PRES_PARAMETROS]
                (
                    [COD_PARAMETRO],
                    [DESCRIPCION],
                    [NOTAS],
                    [VALOR],
                    [REGISTRO_USUARIO],
                    [REGISTRO_FECHA]
                )
                VALUES
                (
                    @CodParametro,
                    @Descripcion,
                    @Notas,
                    @Valor,
                    @RegistroUsuario,
                    GETDATE()
                );";

            const string updateSql = @"
                UPDATE [dbo].[PRES_PARAMETROS]
                SET 
                    [DESCRIPCION]      = @Descripcion,
                    [NOTAS]            = @Notas,
                    [VALOR]            = @Valor,
                    [MODIFICA_USUARIO] = @ModificaUsuario,
                    [MODIFICA_FECHA]   = GETDATE()
                WHERE COD_PARAMETRO   = @CodParametro;";

            try
            {
                using var connection = CreateConnection(codEmpresa);

                var keyParams = new { CodParametro = parametros.cod_parametro };

                // Valido si existe el parámetro
                var existe = connection.ExecuteScalar<int?>(selectSql, keyParams);

                if (existe == null)
                {
                    // INSERT
                    var insertParams = new
                    {
                        CodParametro = parametros.cod_parametro,
                        Descripcion = parametros.descripcion,
                        Notas = parametros.notas,
                        Valor = parametros.valor,
                        RegistroUsuario = parametros.registro_usuario
                    };

                    response.Code = connection.Execute(insertSql, insertParams);
                }
                else
                {
                    // UPDATE
                    var updateParams = new
                    {
                        CodParametro = parametros.cod_parametro,
                        Descripcion = parametros.descripcion,
                        Notas = parametros.notas,
                        Valor = parametros.valor,
                        ModificaUsuario = parametros.modifica_usuario
                    };

                    response.Code = connection.Execute(updateSql, updateParams);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "PresParametros_Guardar: " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Método para obtener la lista de parámetros
        /// </summary>
        public ErrorDto<List<PresParametrosDto>> PresParametrosLista_Obtener(int codEmpresa)
        {
            var response = new ErrorDto<List<PresParametrosDto>>
            {
                Code = 0
            };

            const string sql = @"
                SELECT 
                    COD_PARAMETRO      AS cod_parametro,
                    DESCRIPCION        AS descripcion,
                    NOTAS              AS notas,
                    VALOR              AS valor,
                    REGISTRO_USUARIO   AS registro_usuario,
                    REGISTRO_FECHA     AS registro_fecha,
                    MODIFICA_USUARIO   AS modifica_usuario,
                    MODIFICA_FECHA     AS modifica_fecha
                FROM PRES_PARAMETROS;";

            try
            {
                using var connection = CreateConnection(codEmpresa);
                response.Result = connection.Query<PresParametrosDto>(sql).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = "PresParametrosLista_Obtener: " + ex.Message;
                response.Result = null;
            }

            return response;
        }
    }
}