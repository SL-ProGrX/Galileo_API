using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosAptProfesionalesDB
    {
        /// <summary>
        /// Inserta un profesional; si ya existe, delega en la actualización.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="profesional">Datos del profesional.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneAptPro_Insertar(int CodCliente, BeneAptProfesionalesData profesional)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0)
                                           FROM AFI_BENE_APT_PROFESIONALES
                                           WHERE ID_PROFESIONAL = @id_profesional";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste, new { profesional.id_profesional });

                if (existe > 0)
                {
                    return AfBeneAptPro_Actualizar(CodCliente, profesional);
                }

                const string sql = @"INSERT INTO AFI_BENE_APT_PROFESIONALES
                                        (IDENTIFICACION, NOMBRE, USUARIO, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO)
                                     VALUES
                                        (@identificacion, @nombre, @usuario, @activo, GETDATE(), @registro_usuario)";

                connection.Execute(sql, new
                {
                    profesional.identificacion,
                    profesional.nombre,
                    profesional.usuario,
                    activo = profesional.activo ? 1 : 0,
                    profesional.registro_usuario
                });

                return DbHelper.OkResponse("Ok");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un profesional existente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="profesional">Datos del profesional.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneAptPro_Actualizar(int CodCliente, BeneAptProfesionalesData profesional)
        {
            const string sql = @"UPDATE AFI_BENE_APT_PROFESIONALES
                                 SET IDENTIFICACION = @identificacion, NOMBRE = @nombre, USUARIO = @usuario,
                                     ACTIVO = @activo, MODIFICA_FECHA = GETDATE(), MODIFICA_USUARIO = @modifica_usuario
                                 WHERE ID_PROFESIONAL = @id_profesional";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new
            {
                profesional.identificacion,
                profesional.nombre,
                profesional.usuario,
                activo = profesional.activo ? 1 : 0,
                profesional.modifica_usuario,
                profesional.id_profesional
            });
        }

        /// <summary>
        /// Elimina un profesional.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="id_profesional">Identificador del profesional.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneAptPro_Eliminar(int CodCliente, int id_profesional)
        {
            const string sql = "DELETE FROM AFI_BENE_APT_PROFESIONALES WHERE ID_PROFESIONAL = @id_profesional";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { id_profesional });
        }
    }
}
