using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public partial class FrmFslRequisitosDB
    {
        /// <summary>
        /// Guarda un requisito (inserta si no existe, o actualiza si ya existe).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="requisito">Datos del requisito.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Requisito_Guardar(int CodCliente, FslRequisitosData requisito)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                return ExisteRequisito(connection, requisito.cod_requisito)
                    ? FslRequisito_Actualizar(connection, requisito)
                    : FslRequisito_Insertar(connection, requisito);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Verifica si existe un requisito por código.
        /// </summary>
        private static bool ExisteRequisito(SqlConnection connection, string cod_requisito)
        {
            const string sql = "SELECT ISNULL(COUNT(*), 0) FROM FSL_REQUISITOS WHERE COD_REQUISITO = @cod_requisito";
            return connection.QueryFirstOrDefault<int>(sql, new { cod_requisito }) > 0;
        }

        /// <summary>
        /// Inserta un requisito.
        /// </summary>
        private static ErrorDto FslRequisito_Insertar(SqlConnection connection, FslRequisitosData requisito)
        {
            const string sql = @"INSERT INTO FSL_REQUISITOS (COD_REQUISITO, DESCRIPCION, ACTIVO, registro_fecha, registro_usuario)
                                 VALUES (@cod_requisito, @descripcion, @activo, GETDATE(), @registro_usuario)";
            connection.Execute(sql, new
            {
                requisito.cod_requisito,
                requisito.descripcion,
                activo = requisito.activo ? 1 : 0,
                requisito.registro_usuario
            });
            return new ErrorDto { Code = 0 };
        }

        /// <summary>
        /// Actualiza un requisito.
        /// </summary>
        private static ErrorDto FslRequisito_Actualizar(SqlConnection connection, FslRequisitosData requisito)
        {
            const string sql = @"UPDATE FSL_REQUISITOS SET DESCRIPCION = @descripcion, ACTIVO = @activo
                                 WHERE COD_REQUISITO = @cod_requisito";
            connection.Execute(sql, new
            {
                requisito.descripcion,
                activo = requisito.activo ? 1 : 0,
                requisito.cod_requisito
            });
            return new ErrorDto { Code = 0 };
        }

        /// <summary>
        /// Elimina un requisito.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_requisito">Código del requisito.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslRequisito_Eliminar(int CodCliente, string cod_requisito)
        {
            const string sql = "DELETE FROM FSL_REQUISITOS WHERE COD_REQUISITO = @cod_requisito";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { cod_requisito });
        }

        /// <summary>
        /// Edita la asignación de un requisito a una causa/plan (opcional y asignado).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="asignacion">Datos de la asignación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslAsignacion_Editar(int CodCliente, FslRequisitoEditar asignacion)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                const string sqlOpcional = @"UPDATE FSL_REQUISITOS_CAUSAS SET Opcional = @opcional
                                             WHERE COD_PLAN = @cod_plan AND cod_Causa = @cod_causa AND cod_requisito = @cod_requisito";
                connection.Execute(sqlOpcional, new
                {
                    opcional = asignacion.opcional ? 1 : 0,
                    asignacion.cod_plan,
                    asignacion.cod_causa,
                    asignacion.cod_requisito
                });

                const string sqlAsignado = @"UPDATE FSL_REQUISITOS_CAUSAS SET Asignado = @asignado
                                             WHERE COD_PLAN = @cod_plan AND cod_Causa = @cod_causa AND cod_requisito = @cod_requisito";
                connection.Execute(sqlAsignado, new
                {
                    asignado = asignacion.asignado ? 1 : 0,
                    asignacion.cod_plan,
                    asignacion.cod_causa,
                    asignacion.cod_requisito
                });

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }
    }
}
