using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficioRolesDB
    {
        /// <summary>
        /// Asocia un usuario a un grupo de beneficios.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="usuario">Usuario a asociar.</param>
        /// <param name="cod_grupo">Código del grupo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto GrupoUsuario_Insertar(int CodCliente, string usuario, string cod_grupo)
        {
            const string sql = "INSERT AFI_BENE_USERG (usuario, cod_grupo) VALUES (@usuario, @cod_grupo)";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql,
                new { usuario = usuario.Trim(), cod_grupo = cod_grupo.Trim() });

            if (result.Code == 0)
            {
                result.Description = "Grupo Insertado!";
            }

            return result;
        }

        /// <summary>
        /// Desasocia un usuario de un grupo de beneficios.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="usuario">Usuario a desasociar.</param>
        /// <param name="cod_grupo">Código del grupo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto GrupoUsuario_Eliminar(int CodCliente, string usuario, string cod_grupo)
        {
            const string sql = "DELETE FROM AFI_BENE_USERG WHERE usuario = @usuario AND cod_grupo = @cod_grupo";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { usuario, cod_grupo });

            if (result.Code == 0)
            {
                result.Description = "Grupo Eliminado!";
            }

            return result;
        }

        /// <summary>
        /// Guarda un grupo de beneficios: inserta si no existe, o actualiza su descripción.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="grupo">Datos del grupo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficioGrupo_Guardar(int CodCliente, BeneficioGrupoData grupo)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                return BeneficioGrupo_Existe(connection, grupo.cod_grupo)
                    ? BeneficioGrupo_Actualizar(connection, grupo)
                    : BeneficioGrupo_Insertar(connection, grupo);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta un nuevo grupo de beneficios.
        /// </summary>
        private static ErrorDto BeneficioGrupo_Insertar(SqlConnection connection, BeneficioGrupoData grupo)
        {
            const string sql = "INSERT AFI_BENEFICIO_GRUPOS (cod_grupo, descripcion) VALUES (@cod_grupo, @descripcion)";
            connection.Execute(sql, new { grupo.cod_grupo, grupo.descripcion });
            return DbHelper.OkResponse("Grupo Insertado!");
        }

        /// <summary>
        /// Actualiza la descripción de un grupo de beneficios existente.
        /// </summary>
        private static ErrorDto BeneficioGrupo_Actualizar(SqlConnection connection, BeneficioGrupoData grupo)
        {
            const string sql = "UPDATE AFI_BENEFICIO_GRUPOS SET descripcion = @descripcion WHERE cod_grupo = @cod_grupo";
            connection.Execute(sql, new { grupo.descripcion, grupo.cod_grupo });
            return DbHelper.OkResponse("Grupo Actualizado!");
        }
    }
}
