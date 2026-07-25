using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.FSL;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    public partial class FrmFslComiteDB
    {
        /// <summary>
        /// Guarda un comité (inserta si no existe, o actualiza si ya existe).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="comite">Datos del comité.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Comite_Guardar(int CodCliente, FslComitesDto comite)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                return Comite_Existe(connection, comite.cod_comite)
                    ? FslComites_Actualizar(connection, comite)
                    : FslComites_Insertar(connection, comite);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static bool Comite_Existe(SqlConnection connection, string cod_comite)
        {
            const string sql = "SELECT COUNT(*) FROM FSL_COMITES WHERE COD_COMITE = @cod_comite";
            return connection.QueryFirstOrDefault<int>(sql, new { cod_comite }) > 0;
        }

        private static ErrorDto FslComites_Insertar(SqlConnection connection, FslComitesDto comite)
        {
            const string sql = @"INSERT FSL_COMITES (COD_COMITE, Descripcion, Numero_Resolutores, Activo, registro_fecha, registro_usuario)
                                 VALUES (@cod_comite, @descripcion, @numero_resolutores, @activo, GETDATE(), @registro_usuario)";
            connection.Execute(sql, new
            {
                comite.cod_comite,
                comite.descripcion,
                comite.numero_resolutores,
                activo = comite.activo ? 1 : 0,
                comite.registro_usuario
            });
            return new ErrorDto { Code = 0 };
        }

        private static ErrorDto FslComites_Actualizar(SqlConnection connection, FslComitesDto comite)
        {
            const string sql = @"UPDATE FSL_COMITES SET Descripcion = @descripcion, Numero_Resolutores = @numero_resolutores, ACTIVO = @activo
                                 WHERE COD_COMITE = @cod_comite";
            connection.Execute(sql, new
            {
                comite.descripcion,
                comite.numero_resolutores,
                activo = comite.activo ? 1 : 0,
                comite.cod_comite
            });
            return new ErrorDto { Code = 0 };
        }

        /// <summary>
        /// Elimina un comité.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="comite">Código del comité.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslComites_Eliminar(int CodCliente, string comite)
        {
            const string sql = "DELETE FROM FSL_COMITES WHERE COD_COMITE = @comite";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { comite });
        }

        /// <summary>
        /// Guarda un miembro de comité (inserta si no existe, o actualiza si ya existe).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="miembro">Datos del miembro.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto ComiteMiembro_Guardar(int CodCliente, FslMiembrosComitesDto miembro)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                return MiembroComite_Existe(connection, miembro.cod_comite, miembro.cedula)
                    ? FslMiembrosComite_Actualizar(connection, miembro)
                    : FslMiembrosComite_Insertar(connection, miembro);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static bool MiembroComite_Existe(SqlConnection connection, string cod_comite, string cedula)
        {
            const string sql = "SELECT COUNT(*) FROM FSL_COMITES_MIEMBROS WHERE COD_COMITE = @cod_comite AND CEDULA = @cedula";
            return connection.QueryFirstOrDefault<int>(sql, new { cod_comite, cedula }) > 0;
        }

        private static ErrorDto FslMiembrosComite_Insertar(SqlConnection connection, FslMiembrosComitesDto miembro)
        {
            const string sql = @"INSERT FSL_COMITES_MIEMBROS (CEDULA, COD_COMITE, Nombre, USUARIO_VINCULADO, Activo, registro_fecha, registro_usuario)
                                 VALUES (@cedula, @cod_comite, @nombre, @usuario_Vinculado, @activo, GETDATE(), @registro_Usuario)";
            connection.Execute(sql, new
            {
                miembro.cedula,
                miembro.cod_comite,
                miembro.nombre,
                miembro.usuario_Vinculado,
                activo = miembro.activo ? 1 : 0,
                miembro.registro_Usuario
            });
            return new ErrorDto { Code = 0 };
        }

        private static ErrorDto FslMiembrosComite_Actualizar(SqlConnection connection, FslMiembrosComitesDto miembro)
        {
            var activo = miembro.activo ? 1 : 0;
            var salidaSet = activo == 0
                ? ", Salida_Fecha = GETDATE(), Salida_Usuario = @salida_usuario"
                : ", Salida_Fecha = NULL, Salida_Usuario = NULL";

            var sql = $@"UPDATE FSL_COMITES_MIEMBROS
                         SET Nombre = @nombre, USUARIO_VINCULADO = @usuario_Vinculado, Activo = @activo {salidaSet}
                         WHERE COD_COMITE = @cod_comite AND CEDULA = @cedula";

            connection.Execute(sql, new
            {
                miembro.nombre,
                miembro.usuario_Vinculado,
                activo,
                miembro.salida_usuario,
                miembro.cod_comite,
                miembro.cedula
            });
            return new ErrorDto { Code = 0 };
        }

        /// <summary>
        /// Elimina un miembro de un comité.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cedula">Cédula del miembro.</param>
        /// <param name="comite">Código del comité.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto FslMiembrosComite_Eliminar(int CodCliente, string cedula, string comite)
        {
            const string sql = "DELETE FROM FSL_COMITES_MIEMBROS WHERE COD_COMITE = @comite AND CEDULA = @cedula";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { comite, cedula });
        }
    }
}
