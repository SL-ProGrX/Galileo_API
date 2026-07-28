using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public partial class FrmFslTipoAplicacionDB
    {
        /// <summary>
        /// Inserta un plan; si ya existe, delega en la actualización.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="planData">Datos del plan.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Planes_Insertar(int CodCliente, PlanDataInsert planData)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                if (Plan_Existe(connection, planData.cod_plan))
                {
                    return Planes_Actualizar(connection, planData);
                }

                const string sql = @"INSERT FSL_PLANES (COD_PLAN, Descripcion, Tipo_Desembolso, Activo, registro_fecha, registro_usuario)
                                     VALUES (@cod_plan, @descripcion, @tipo_desembolso, @activo, GETDATE(), @registro_usuario)";
                connection.Execute(sql, new
                {
                    planData.cod_plan,
                    planData.descripcion,
                    planData.tipo_desembolso,
                    activo = planData.activo ? 1 : 0,
                    planData.registro_usuario
                });

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static ErrorDto Planes_Actualizar(SqlConnection connection, PlanDataInsert planData)
        {
            const string sql = @"UPDATE FSL_PLANES SET Descripcion = @descripcion, Tipo_Desembolso = @tipo_desembolso, Activo = @activo
                                 WHERE COD_PLAN = @cod_plan";
            connection.Execute(sql, new
            {
                planData.descripcion,
                planData.tipo_desembolso,
                activo = planData.activo ? 1 : 0,
                planData.cod_plan
            });
            return new ErrorDto { Code = 0 };
        }

        /// <summary>
        /// Elimina un plan.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_plan">Código del plan.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Planes_Eliminar(int CodCliente, string cod_plan)
        {
            const string sql = "DELETE FSL_PLANES WHERE COD_PLAN = @cod_plan";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { cod_plan });
        }

        private static bool Plan_Existe(SqlConnection connection, string cod_plan)
        {
            const string sql = "SELECT ISNULL(COUNT(*), 0) FROM FSL_PLANES WHERE COD_PLAN = @cod_plan";
            return connection.QueryFirstOrDefault<int>(sql, new { cod_plan }) > 0;
        }

        /// <summary>
        /// Inserta una causa; si ya existe, delega en la actualización.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="causaData">Datos de la causa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Causas_Insertar(int CodCliente, CausaDataInsert causaData)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                if (Causa_Existe(connection, causaData.cod_causa, causaData.cod_plan))
                {
                    return Causas_Actualizar(connection, causaData);
                }

                const string sql = @"INSERT FSL_PLANES_CAUSAS
                                        (COD_CAUSA, cod_plan, Descripcion, Monto_Base, Tipo_Tabla, Activa, registro_fecha, registro_usuario)
                                     VALUES
                                        (@cod_causa, @cod_plan, @descripcion, @monto_base, @tipo_tabla, @activa, GETDATE(), @registro_usuario)";
                connection.Execute(sql, new
                {
                    causaData.cod_causa,
                    causaData.cod_plan,
                    causaData.descripcion,
                    causaData.monto_base,
                    causaData.tipo_tabla,
                    activa = causaData.activa ? 1 : 0,
                    causaData.registro_usuario
                });

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una causa existente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="causaData">Datos de la causa.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Causas_Actualizar(int CodCliente, CausaDataInsert causaData)
        {
            const string sql = @"UPDATE FSL_PLANES_CAUSAS
                                 SET descripcion = @descripcion, Tipo_Tabla = @tipo_tabla, Monto_Base = @monto_base, Activa = @activa
                                 WHERE COD_CAUSA = @cod_causa AND COD_PLAN = @cod_plan";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new
            {
                causaData.descripcion,
                causaData.tipo_tabla,
                causaData.monto_base,
                activa = causaData.activa ? 1 : 0,
                causaData.cod_causa,
                causaData.cod_plan
            });
        }

        /// <summary>
        /// Elimina una causa.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_causa">Código de la causa.</param>
        /// <param name="cod_plan">Código del plan.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Causas_Eliminar(int CodCliente, string cod_causa, string cod_plan)
        {
            const string sql = "DELETE FSL_PLANES_CAUSAS WHERE COD_CAUSA = @cod_causa AND COD_PLAN = @cod_plan";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { cod_causa, cod_plan });
        }

        private static bool Causa_Existe(SqlConnection connection, string cod_causa, string cod_plan)
        {
            const string sql = "SELECT ISNULL(COUNT(*), 0) FROM FSL_PLANES_CAUSAS WHERE COD_CAUSA = @cod_causa AND COD_PLAN = @cod_plan";
            return connection.QueryFirstOrDefault<int>(sql, new { cod_causa, cod_plan }) > 0;
        }

        /// <summary>
        /// Actualiza una causa usando una conexión abierta (uso interno).
        /// </summary>
        private static ErrorDto Causas_Actualizar(SqlConnection connection, CausaDataInsert causaData)
        {
            const string sql = @"UPDATE FSL_PLANES_CAUSAS
                                 SET descripcion = @descripcion, Tipo_Tabla = @tipo_tabla, Monto_Base = @monto_base, Activa = @activa
                                 WHERE COD_CAUSA = @cod_causa AND COD_PLAN = @cod_plan";
            connection.Execute(sql, new
            {
                causaData.descripcion,
                causaData.tipo_tabla,
                causaData.monto_base,
                activa = causaData.activa ? 1 : 0,
                causaData.cod_causa,
                causaData.cod_plan
            });
            return new ErrorDto { Code = 0 };
        }
    }
}
