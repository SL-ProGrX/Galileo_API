using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneSancionesTiposDB
    {
        /// <summary>
        /// Inserta un tipo de sanción; si ya existe, delega en la actualización.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="tipo_sancion">Datos del tipo de sanción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneTipoSancion_Insertar(int CodCliente, AfTipoSancionesDto tipo_sancion)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0)
                                           FROM AFI_BENE_SANCIONES_TIPOS WHERE TIPO_SANCION = @tipo_sancion";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste, new { tipo_sancion.tipo_sancion });

                if (existe > 0)
                {
                    return AfBeneTipoSancion_Actualizar(CodCliente, tipo_sancion);
                }

                const string sql = @"INSERT INTO AFI_BENE_SANCIONES_TIPOS
                                        (TIPO_SANCION, DESCRIPCION, CODIGO_COBRO, PLAZO_MAXIMO, ACTIVO, REGISTRO_FECHA, REGISTRO_USUARIO)
                                     VALUES
                                        (@tipo_sancion, @descripcion, @codigo_cobro, @plazo_maximo, @activo, GETDATE(), @registro_usuario)";

                connection.Execute(sql, new
                {
                    tipo_sancion.tipo_sancion,
                    tipo_sancion.descripcion,
                    tipo_sancion.codigo_cobro,
                    tipo_sancion.plazo_maximo,
                    activo = tipo_sancion.activo ? 1 : 0,
                    tipo_sancion.registro_usuario
                });

                return DbHelper.OkResponse("Ok");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un tipo de sanción existente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="tipo_sancion">Datos del tipo de sanción.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneTipoSancion_Actualizar(int CodCliente, AfTipoSancionesDto tipo_sancion)
        {
            const string sql = @"UPDATE AFI_BENE_SANCIONES_TIPOS
                                 SET DESCRIPCION = @descripcion, CODIGO_COBRO = @codigo_cobro, PLAZO_MAXIMO = @plazo_maximo,
                                     REGISTRO_USUARIO = @registro_usuario, ACTIVO = @activo,
                                     MODIFICA_FECHA = GETDATE(), MODIFICA_USUARIO = @modifica_usuario
                                 WHERE TIPO_SANCION = @tipo_sancion";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new
            {
                tipo_sancion.descripcion,
                tipo_sancion.codigo_cobro,
                tipo_sancion.plazo_maximo,
                tipo_sancion.registro_usuario,
                activo = tipo_sancion.activo ? 1 : 0,
                tipo_sancion.modifica_usuario,
                tipo_sancion.tipo_sancion
            });
        }

        /// <summary>
        /// Elimina un tipo de sanción.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="tipo_sancion">Código del tipo de sanción a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneTipoSancion_Eliminar(int CodCliente, int tipo_sancion)
        {
            const string sql = "DELETE FROM AFI_BENE_SANCIONES_TIPOS WHERE TIPO_SANCION = @tipo_sancion";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { tipo_sancion });
        }
    }
}
