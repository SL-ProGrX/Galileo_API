using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneFormulariosDB
    {
        /// <summary>
        /// Agrega un formulario nuevo con sus preguntas y opciones, y deja traza en bitácora.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="formulario">JSON completo del formulario.</param>
        /// <returns>Resultado con el ID del formulario en Description.</returns>
        public ErrorDto AfBeneFormularios_Agregar(int CodCliente, string formulario)
        {
            var frm = JsonConvert.DeserializeObject<Formulario>(formulario) ?? new Formulario();

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                frm.id_form = connection.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(ID_FORM), 0) + 1 FROM AFI_BENE_FORM_MAIN_W");

                const string sqlMain = @"INSERT INTO [dbo].[AFI_BENE_FORM_MAIN_W]
                                            (ID_FORM, COD_FORMULARIO, COD_BENEFICIO, FRM_TITULO, REGISTRO_USUARIO, REGISTRO_FECHA, ACTIVO, BORRADO)
                                         VALUES
                                            (@id_form, @id_form, @cod_beneficio, @frm_titulo, @registro_usuario, GETDATE(), 1, 0)";
                connection.Execute(sqlMain, new { frm.id_form, frm.cod_beneficio, frm.frm_titulo, frm.registro_usuario });

                foreach (var question in frm.formulario.questions ?? new List<FormQuestion>())
                {
                    InsertarPregunta(connection, frm.id_form, frm.registro_usuario, question);
                }

                _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
                {
                    EmpresaId = CodCliente,
                    cod_beneficio = frm.cod_beneficio,
                    consec = 0,
                    movimiento = "Creacion Form-WEB",
                    detalle = $"Creacion de Formulario [{frm.frm_titulo}] con codigo [{frm.id_form}] para Beneficio [{frm.cod_beneficio}]",
                    registro_usuario = frm.registro_usuario
                });

                return new ErrorDto { Code = 0, Description = frm.id_form.ToString() };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Inserta una pregunta del formulario y sus opciones.
        /// </summary>
        private static void InsertarPregunta(SqlConnection connection, int id_form, string usuario, FormQuestion question)
        {
            question.id_frm_pregunta = connection.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(ID_FRM_PREGUNTA), 0) + 1 FROM AFI_BENE_FORM_PREGUNTAS_W");

            const string sql = @"INSERT INTO [dbo].[AFI_BENE_FORM_PREGUNTAS_W]
                                    (ID_FORM, ID_FRM_PREGUNTA, PREGUNTA_ORDEN, PREGUNTA_TITULO, PREGUNTA_TIPO, REGISTRO_FECHA, REGISTRO_USUARIO, REQUERIDO, BORRADO)
                                 VALUES
                                    (@id_form, @id_frm_pregunta, @pregunta_orden, @pregunta_titulo, @pregunta_tipo, GETDATE(), @usuario, @requerido, 0)";
            connection.Execute(sql, new
            {
                id_form,
                question.id_frm_pregunta,
                question.pregunta_orden,
                question.pregunta_titulo,
                question.pregunta_tipo,
                usuario,
                requerido = question.requerido == true ? 1 : 0
            });

            foreach (var option in question.opciones ?? new List<OptionabledQuestion>())
            {
                InsertarOpcion(connection, question.id_frm_pregunta, usuario, option);
            }
        }

        /// <summary>
        /// Inserta una opción de una pregunta del formulario.
        /// </summary>
        private static void InsertarOpcion(SqlConnection connection, int id_frm_pregunta, string usuario, OptionabledQuestion option)
        {
            var id_opciones = connection.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(ID_OPCIONES), 0) + 1 FROM AFI_BENE_FORM_OPCIONES_W");

            const string sql = @"INSERT INTO [dbo].[AFI_BENE_FORM_OPCIONES_W]
                                    (ID_OPCIONES, ID_FRM_PREGUNTA, ITEM, DESCRIPCION, SELECCION, REGISTRO_FECHA, REGISTRO_USUARIO, BORRADO)
                                 VALUES
                                    (@id_opciones, @id_frm_pregunta, @item, @descripcion, @seleccion, GETDATE(), @usuario, 0)";
            connection.Execute(sql, new
            {
                id_opciones,
                id_frm_pregunta,
                option.item,
                option.descripcion,
                seleccion = option.selected == true ? 1 : 0,
                usuario
            });
        }

        /// <summary>
        /// Actualiza el título y estado (activo/inactivo) de un formulario.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="formulario">JSON del formulario.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneFrmEncabezado_Actualizar(int CodCliente, string formulario)
        {
            var frm = JsonConvert.DeserializeObject<Formulario>(formulario) ?? new Formulario();

            const string sql = @"UPDATE AFI_BENE_FORM_MAIN_W
                                 SET FRM_TITULO = @frm_titulo, MODIFICA_USUARIO = @modifica_usuario, MODIFICA_FECHA = GETDATE(), ACTIVO = @activo
                                 WHERE COD_BENEFICIO = @cod_beneficio AND ID_FORM = @id_form";

            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new
            {
                frm.frm_titulo,
                frm.modifica_usuario,
                activo = frm.activo == true ? 1 : 0,
                frm.cod_beneficio,
                frm.id_form
            });
        }

        /// <summary>
        /// Elimina (marca como borrado) un formulario con sus preguntas y opciones.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="if_frm">Identificador del formulario.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneFormulario_Eliminar(int CodCliente, int if_frm)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                connection.Execute(@"UPDATE AFI_BENE_FORM_OPCIONES_W SET BORRADO = 1
                                     WHERE ID_FRM_PREGUNTA IN (SELECT ID_FRM_PREGUNTA FROM AFI_BENE_FORM_PREGUNTAS_W WHERE ID_FORM = @if_frm)", new { if_frm });
                connection.Execute("UPDATE AFI_BENE_FORM_PREGUNTAS_W SET BORRADO = 1 WHERE ID_FORM = @if_frm", new { if_frm });
                connection.Execute("UPDATE AFI_BENE_FORM_MAIN_W SET BORRADO = 1 WHERE ID_FORM = @if_frm", new { if_frm });

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una pregunta del formulario, insertándola si es nueva (id = 0).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="id_from">Identificador del formulario.</param>
        /// <param name="usuario">Usuario que modifica.</param>
        /// <param name="formulario">JSON de la pregunta.</param>
        /// <returns>Resultado con el ID de la pregunta en Description.</returns>
        public ErrorDto AfBeneFrmDetalle_Actualizar(int CodCliente, int id_from, string usuario, string formulario)
        {
            var frm = JsonConvert.DeserializeObject<FormQuestion>(formulario) ?? new FormQuestion();

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var requerido = frm.requerido == true ? 1 : 0;

                if (frm.id_frm_pregunta == 0)
                {
                    frm.id_frm_pregunta = connection.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(ID_FRM_PREGUNTA), 0) + 1 FROM AFI_BENE_FORM_PREGUNTAS_W");

                    const string sqlInsert = @"INSERT INTO [dbo].[AFI_BENE_FORM_PREGUNTAS_W]
                                                (ID_FORM, ID_FRM_PREGUNTA, PREGUNTA_ORDEN, PREGUNTA_TITULO, PREGUNTA_TIPO, REGISTRO_FECHA, REGISTRO_USUARIO, REQUERIDO, BORRADO, CAMPO_HOMOLOGADO)
                                               VALUES
                                                (@id_from, @id_frm_pregunta, @pregunta_orden, @pregunta_titulo, @pregunta_tipo, GETDATE(), @usuario, @requerido, 0, @campo_homologado)";
                    connection.Execute(sqlInsert, new
                    {
                        id_from,
                        frm.id_frm_pregunta,
                        frm.pregunta_orden,
                        frm.pregunta_titulo,
                        frm.pregunta_tipo,
                        usuario,
                        requerido,
                        frm.campo_homologado
                    });
                }
                else
                {
                    const string sqlUpdate = @"UPDATE AFI_BENE_FORM_PREGUNTAS_W
                                               SET PREGUNTA_ORDEN = @pregunta_orden, PREGUNTA_TITULO = @pregunta_titulo, PREGUNTA_TIPO = @pregunta_tipo,
                                                   REQUERIDO = @requerido, MODIFICA_FECHA = GETDATE(), MODIFICA_USUARIO = @usuario, CAMPO_HOMOLOGADO = @campo_homologado
                                               WHERE ID_FORM = @id_from AND ID_FRM_PREGUNTA = @id_frm_pregunta";
                    connection.Execute(sqlUpdate, new
                    {
                        frm.pregunta_orden,
                        frm.pregunta_titulo,
                        frm.pregunta_tipo,
                        requerido,
                        usuario,
                        frm.campo_homologado,
                        id_from,
                        frm.id_frm_pregunta
                    });
                }

                return new ErrorDto { Code = 0, Description = frm.id_frm_pregunta.ToString() };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina (marca como borrada) una pregunta del formulario y sus opciones.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="frm_pregunta">Identificador de la pregunta.</param>
        /// <param name="usuario">Usuario que modifica.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneFrmDetalle_Elimina(int CodCliente, int frm_pregunta, string usuario)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                connection.Execute(@"UPDATE AFI_BENE_FORM_OPCIONES_W SET BORRADO = 1, MODIFICA_USUARIO = @usuario, MODIFICA_FECHA = GETDATE()
                                     WHERE ID_FRM_PREGUNTA = @frm_pregunta", new { usuario, frm_pregunta });
                connection.Execute(@"UPDATE AFI_BENE_FORM_PREGUNTAS_W SET BORRADO = 1, MODIFICA_USUARIO = @usuario, MODIFICA_FECHA = GETDATE()
                                     WHERE ID_FRM_PREGUNTA = @frm_pregunta", new { usuario, frm_pregunta });

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Actualiza una opción de una pregunta, insertándola si es nueva (id = 0).
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="frm_pregunta">Identificador de la pregunta.</param>
        /// <param name="usuario">Usuario que modifica.</param>
        /// <param name="opcion">JSON de la opción.</param>
        /// <returns>Resultado con el ID de la opción en Description.</returns>
        public ErrorDto AfBeneFrmOpciones_Actualizar(int CodCliente, int frm_pregunta, string usuario, string opcion)
        {
            var frm = JsonConvert.DeserializeObject<OptionabledQuestion>(opcion) ?? new OptionabledQuestion();

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                if (frm.id_opciones == 0)
                {
                    frm.id_opciones = connection.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(ID_OPCIONES), 0) + 1 FROM AFI_BENE_FORM_OPCIONES_W");

                    const string sqlInsert = @"INSERT INTO [dbo].[AFI_BENE_FORM_OPCIONES_W]
                                                (ID_OPCIONES, ID_FRM_PREGUNTA, ITEM, DESCRIPCION, SELECCION, REGISTRO_FECHA, REGISTRO_USUARIO, BORRADO)
                                               VALUES
                                                (@id_opciones, @frm_pregunta, @item, @descripcion, 0, GETDATE(), @usuario, 0)";
                    connection.Execute(sqlInsert, new { frm.id_opciones, frm_pregunta, frm.item, frm.descripcion, usuario });
                }
                else
                {
                    const string sqlUpdate = @"UPDATE AFI_BENE_FORM_OPCIONES_W
                                               SET ITEM = @item, DESCRIPCION = @descripcion, MODIFICA_FECHA = GETDATE(), MODIFICA_USUARIO = @usuario
                                               WHERE ID_FRM_PREGUNTA = @frm_pregunta AND ID_OPCIONES = @id_opciones";
                    connection.Execute(sqlUpdate, new { frm.item, frm.descripcion, usuario, frm_pregunta, frm.id_opciones });
                }

                return new ErrorDto { Code = 0, Description = frm.id_opciones.ToString() };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina (marca como borrada) una opción de una pregunta.
        /// </summary>
        /// <param name="CodCliente">Código de empresa.</param>
        /// <param name="frm_pregunta">Identificador de la pregunta.</param>
        /// <param name="id_opciones">Identificador de la opción.</param>
        /// <param name="usuario">Usuario que modifica.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneFrmOpciones_Eliminar(int CodCliente, int frm_pregunta, int id_opciones, string usuario)
        {
            const string sql = @"UPDATE AFI_BENE_FORM_OPCIONES_W SET BORRADO = 1, MODIFICA_USUARIO = @usuario, MODIFICA_FECHA = GETDATE()
                                 WHERE ID_FRM_PREGUNTA = @frm_pregunta AND ID_OPCIONES = @id_opciones";
            return DbHelper.ExecuteNonQuery(CreatePortalDb(), CodCliente, sql, new { usuario, frm_pregunta, id_opciones });
        }
    }
}
