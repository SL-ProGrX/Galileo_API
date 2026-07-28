using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosRequisitosDB
    {
        /// <summary>
        /// Inserta un requisito; si ya existe, delega en la actualización.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="requisito">Datos del requisito.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneRequisitos_Insertar(int CodCliente, BeneRequisitosData requisito)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                const string sqlExiste = @"SELECT ISNULL(COUNT(*), 0)
                                           FROM AFI_BENE_REQUISITOS
                                           WHERE UPPER(TRIM(COD_REQUISITO)) = UPPER(TRIM(@cod_requisito))";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste, new { requisito.cod_requisito });

                if (existe > 0)
                {
                    return AfBeneRequisitos_Actualizar(CodCliente, requisito);
                }

                const string sql = @"INSERT INTO AFI_BENE_REQUISITOS
                                        (COD_REQUISITO, descripcion, Registro_Fecha, Activo, requerido, registro_usuario)
                                     VALUES
                                        (@cod_requisito, @descripcion, GETDATE(), @activo, @requerido, @registro_usuario)";

                connection.Execute(sql, new
                {
                    requisito.cod_requisito,
                    requisito.descripcion,
                    activo = requisito.activo ? 1 : 0,
                    requerido = requisito.requerido ? 1 : 0,
                    requisito.registro_usuario
                });

                return DbHelper.OkResponse("Catalogo de Requisitos para Beneficios Id " + requisito.cod_requisito);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza un requisito existente.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="requisito">Datos del requisito.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneRequisitos_Actualizar(int CodCliente, BeneRequisitosData requisito)
        {
            const string sql = @"UPDATE AFI_BENE_REQUISITOS
                                 SET descripcion = @descripcion,
                                     Activo = @activo,
                                     Requerido = @requerido,
                                     Modifica_Fecha = GETDATE(),
                                     Modifica_Usuario = @registro_usuario
                                 WHERE COD_REQUISITO = @cod_requisito";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new
            {
                requisito.descripcion,
                activo = requisito.activo ? 1 : 0,
                requerido = requisito.requerido ? 1 : 0,
                requisito.registro_usuario,
                requisito.cod_requisito
            });

            if (result.Code == 0)
            {
                result.Description = "Catalogo de Requisitos para Beneficios Id " + requisito.cod_requisito;
            }

            return result;
        }

        /// <summary>
        /// Elimina un requisito.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="cod_requisito">Código del requisito a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneRequisitos_Eliminar(int CodCliente, string cod_requisito)
        {
            const string sql = "DELETE FROM AFI_BENE_REQUISITOS WHERE COD_REQUISITO = @cod_requisito";

            var result = DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { cod_requisito });

            if (result.Code == 0)
            {
                result.Description = "Catalogo de Requisitos para Beneficios Id " + cod_requisito;
            }

            return result;
        }
    }
}
