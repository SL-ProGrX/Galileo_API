using Dapper;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneficiosCategoriasDB
    {
        /// <summary>
        /// Inserta una categoría de beneficios, validando que el código no exista.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la categoría.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosCategorias_Agregar(int CodEmpresa, BeneCategoria request)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodEmpresa);
            try
            {
                const string sqlExiste = "SELECT COUNT(*) FROM AFI_BENE_CATEGORIAS WHERE COD_CATEGORIA = @cod_categoria";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste, new { request.cod_categoria });

                if (existe > 0)
                {
                    return DbHelper.ErrorResponse("Ya existe una categoria con el codigo: " + request.cod_categoria + ", por favor verifique");
                }

                const string sql = @"INSERT INTO AFI_BENE_CATEGORIAS
                                        (cod_categoria, descripcion, activo, i_apremiante, i_reconocimientos, i_crece, i_fena,
                                         i_sepelio, i_desastres, registro_fecha, registro_usuario)
                                     VALUES
                                        (@cod_categoria, @descripcion, @activo, @i_apremiante, @i_reconocimientos, @i_crece, @i_fena,
                                         @i_sepelio, @i_desastres, GETDATE(), @registro_usuario)";

                connection.Execute(sql, MapCategoriaParams(request));
                return DbHelper.OkResponse("Ok");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una categoría de beneficios.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la categoría.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosCategorias_Actualizar(int CodEmpresa, BeneCategoria request)
        {
            const string sql = @"UPDATE AFI_BENE_CATEGORIAS
                                 SET descripcion = @descripcion, activo = @activo, i_apremiante = @i_apremiante,
                                     i_reconocimientos = @i_reconocimientos, i_crece = @i_crece, i_fena = @i_fena,
                                     i_sepelio = @i_sepelio, i_desastres = @i_desastres,
                                     modifica_fecha = GETDATE(), modifica_usuario = @modifica_usuario
                                 WHERE cod_categoria = @cod_categoria";

            var parametros = MapCategoriaParams(request);
            parametros.Add("modifica_usuario", request.modifica_usuario);

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, parametros);
        }

        /// <summary>
        /// Elimina una categoría de beneficios.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="id">Código de la categoría a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneficiosCategorias_Eliminar(int CodEmpresa, string id)
        {
            const string sql = "DELETE FROM AFI_BENE_CATEGORIAS WHERE COD_CATEGORIA = @id";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new { id });
        }

        /// <summary>
        /// Guarda una validación de categoría: inserta si no existe o actualiza si ya existe.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="valida">Datos de la validación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto BeneCategoriaValida_Guardar(int CodCliente, BeneCategoriaValidaLista valida)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                const string sqlExiste = @"SELECT COUNT(*) FROM AFI_BENE_VALIDA_CATEGORIA
                                           WHERE cod_categoria = @cod_categoria AND cod_val = @cod_val";
                var existe = connection.QueryFirstOrDefault<int>(sqlExiste, new { valida.cod_categoria, valida.cod_val });

                var parametros = new
                {
                    valida.cod_categoria,
                    valida.cod_val,
                    registro = valida.registro ? 1 : 0,
                    registro_justifica = valida.registro_justifica ? 1 : 0,
                    registro_info = valida.registro_info ? 1 : 0,
                    pago = valida.pago ? 1 : 0,
                    pago_justifica = valida.pago_justifica ? 1 : 0,
                    pago_info = valida.pago_info ? 1 : 0,
                    estado = valida.estado ? 1 : 0,
                    valida.registro_usuario,
                    valida.modifica_usuario
                };

                var sql = existe > 0
                    ? @"UPDATE AFI_BENE_VALIDA_CATEGORIA
                        SET registro = @registro, registro_justifica = @registro_justifica, registro_info = @registro_info,
                            pago = @pago, pago_justifica = @pago_justifica, pago_info = @pago_info, estado = @estado,
                            modifica_usuario = @modifica_usuario, modifica_fecha = GETDATE()
                        WHERE cod_categoria = @cod_categoria AND cod_val = @cod_val"
                    : @"INSERT INTO AFI_BENE_VALIDA_CATEGORIA
                            (cod_categoria, cod_val, registro, registro_justifica, registro_info, pago, pago_justifica,
                             pago_info, estado, registro_usuario, registro_fecha)
                        VALUES
                            (@cod_categoria, @cod_val, @registro, @registro_justifica, @registro_info, @pago, @pago_justifica,
                             @pago_info, @estado, @registro_usuario, GETDATE())";

                connection.Execute(sql, parametros);
                return DbHelper.OkResponse("Ok");
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Arma los parámetros comunes de inserción/actualización de una categoría.
        /// </summary>
        private static DynamicParameters MapCategoriaParams(BeneCategoria request)
        {
            var parametros = new DynamicParameters();
            parametros.Add("cod_categoria", request.cod_categoria);
            parametros.Add("descripcion", request.descripcion);
            parametros.Add("activo", request.activo ? 1 : 0);
            parametros.Add("i_apremiante", request.i_apremiante ? 1 : 0);
            parametros.Add("i_reconocimientos", request.i_reconocimientos ? 1 : 0);
            parametros.Add("i_crece", request.i_crece ? 1 : 0);
            parametros.Add("i_fena", request.i_fena ? 1 : 0);
            parametros.Add("i_sepelio", request.i_sepelio ? 1 : 0);
            parametros.Add("i_desastres", request.i_desastres ? 1 : 0);
            parametros.Add("registro_usuario", request.registro_usuario);
            return parametros;
        }
    }
}
