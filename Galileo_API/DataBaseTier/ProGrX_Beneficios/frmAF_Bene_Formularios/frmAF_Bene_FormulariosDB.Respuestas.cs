using Dapper;
using Galileo.Models;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Linq;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class FrmAfBeneFormulariosDB
    {
        /// <summary>
        /// Obtiene los formularios activos de un beneficio con las respuestas del socio.
        /// </summary>
        /// <param name="Jformulario">JSON con los filtros (socio, beneficio).</param>
        /// <returns>Lista de formularios con respuestas.</returns>
        public ErrorDto<List<Formulario>> AfBeneFormSocios_Obtener(string Jformulario)
        {
            var filtros = JsonConvert.DeserializeObject<FrmFiltros>(Jformulario) ?? new FrmFiltros();

            return DbHelper.WithConn(CreatePortalDb(), filtros.codCliente, connection =>
            {
                const string sql = @"SELECT ID_FORM, COD_FORMULARIO, COD_BENEFICIO, FRM_TITULO, REGISTRO_USUARIO,
                                            REGISTRO_FECHA, MODIFICA_USUARIO, MODIFICA_FECHA, ACTIVO
                                     FROM AFI_BENE_FORM_MAIN_W
                                     WHERE COD_BENEFICIO = @cod_beneficio AND ACTIVO = 1 AND BORRADO = 0";
                var formularios = connection.Query<Formulario>(sql, new { filtros.cod_beneficio }).ToList();

                foreach (var item in formularios)
                {
                    CargarFormularioSocio(connection, item, filtros);
                }

                return formularios;
            });
        }

        /// <summary>
        /// Carga las preguntas y respuestas de un formulario para el socio.
        /// </summary>
        private static void CargarFormularioSocio(SqlConnection connection, Formulario item, FrmFiltros filtros)
        {
            item.formulario = new Form { id = item.id_form };

            const string sql = @"SELECT ID_FRM_PREGUNTA, PREGUNTA_ORDEN, PREGUNTA_TITULO, PREGUNTA_TIPO, CAMPO_HOMOLOGADO,
                                    (SELECT R.RESPUESTA_VALOR FROM AFI_BENE_FORM_RESPUESTAS_W R
                                     WHERE CEDULA = @socio AND ID_BENEFICIO = @id_beneficio AND R.ID_OPCIONES_RSP = 0
                                       AND R.ID_FRM_PREGUNTA = PR.ID_FRM_PREGUNTA) AS RESPUESTA,
                                    REQUERIDO
                                 FROM AFI_BENE_FORM_PREGUNTAS_W PR
                                 WHERE ID_FORM = @id_form AND BORRADO = 0
                                 ORDER BY PREGUNTA_ORDEN ASC";
            item.formulario.questions = connection.Query<FormQuestion>(sql,
                new { filtros.socio, filtros.id_beneficio, id_form = item.id_form }).ToList();

            foreach (var question in item.formulario.questions)
            {
                CargarRespuestaPregunta(connection, question, filtros);
            }
        }

        /// <summary>
        /// Carga las opciones/respuesta de una pregunta según su tipo.
        /// </summary>
        private static void CargarRespuestaPregunta(SqlConnection connection, FormQuestion question, FrmFiltros filtros)
        {
            question.opciones = new List<OptionabledQuestion>();

            switch (question.pregunta_tipo)
            {
                case "select":
                case "multiSelect":
                case "checkbox":
                    CargarOpcionesSeleccion(connection, question, filtros);
                    break;
                case "radio":
                    CargarOpcionesRadio(connection, question, filtros);
                    break;
                case "date":
                    question.respuestaFecha = question.respuesta != null
                        ? DateTime.Parse(question.respuesta.ToString()!)
                        : DateTime.Now;
                    break;
            }
        }

        /// <summary>
        /// Carga las opciones marcadas para preguntas de selección múltiple.
        /// </summary>
        private static void CargarOpcionesSeleccion(SqlConnection connection, FormQuestion question, FrmFiltros filtros)
        {
            var id_opcion = ObtenerIdOpcionRespuesta(connection, question.id_frm_pregunta, filtros.socio, filtros.id_beneficio);
            question.opciones = ObtenerOpcionesConSeleccion(connection, question.id_frm_pregunta, id_opcion);
            question.respuesta = question.opciones.Where(x => x.selected == true).ToList();
        }

        /// <summary>
        /// Carga la opción marcada para preguntas de tipo radio.
        /// </summary>
        private static void CargarOpcionesRadio(SqlConnection connection, FormQuestion question, FrmFiltros filtros)
        {
            var id_opcion = ObtenerIdOpcionRespuesta(connection, question.id_frm_pregunta, filtros.socio, null);
            question.opciones = ObtenerOpcionesConSeleccion(connection, question.id_frm_pregunta, id_opcion);

            if (id_opcion > 0)
            {
                var seleccionada = question.opciones.FirstOrDefault(x => x.selected == true);
                question.respuesta = seleccionada?.item;
            }
        }

        /// <summary>
        /// Obtiene el ID de opción de respuesta registrada para una pregunta.
        /// </summary>
        private static int ObtenerIdOpcionRespuesta(SqlConnection connection, int id_frm_pregunta, string? socio, int? id_beneficio)
        {
            var sql = id_beneficio.HasValue
                ? @"SELECT R.ID_OPCIONES_RSP FROM [dbo].[AFI_BENE_FORM_RESPUESTAS_W] R
                    WHERE CEDULA = @socio AND ID_BENEFICIO = @id_beneficio AND R.ID_OPCIONES_RSP != 0 AND R.ID_FRM_PREGUNTA = @id_frm_pregunta"
                : @"SELECT R.ID_OPCIONES_RSP FROM [dbo].[AFI_BENE_FORM_RESPUESTAS_W] R
                    WHERE CEDULA = @socio AND R.ID_OPCIONES_RSP != 0 AND R.ID_FRM_PREGUNTA = @id_frm_pregunta";

            return connection.QueryFirstOrDefault<int>(sql, new { socio, id_beneficio, id_frm_pregunta });
        }

        /// <summary>
        /// Obtiene las opciones de una pregunta indicando cuáles fueron seleccionadas.
        /// </summary>
        private static List<OptionabledQuestion> ObtenerOpcionesConSeleccion(SqlConnection connection, int id_frm_pregunta, int id_opcion)
        {
            const string sql = @"SELECT ITEM, DESCRIPCION,
                                    ISNULL((SELECT SELECCION FROM AFI_BENE_FORM_OPRESP_W WHERE ID_OPCIONES = @id_opcion AND ITEM = OP.ITEM), 0) AS selected,
                                    ID_FRM_PREGUNTA
                                 FROM AFI_BENE_FORM_OPCIONES_W OP
                                 WHERE ID_FRM_PREGUNTA = @id_frm_pregunta AND BORRADO = 0
                                 ORDER BY ITEM DESC";
            return connection.Query<OptionabledQuestion>(sql, new { id_opcion, id_frm_pregunta }).ToList();
        }

        /// <summary>
        /// Agrega las respuestas de un formulario para un socio; reemplaza las previas si existen.
        /// </summary>
        /// <param name="Jdatos">JSON con los filtros (socio, beneficio, usuario).</param>
        /// <param name="frm">Formulario con las preguntas y respuestas.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto AfBeneFrmRespuesta_Agregar(string Jdatos, Form frm)
        {
            var datos = JsonConvert.DeserializeObject<FrmFiltros>(Jdatos) ?? new FrmFiltros();

            var validacion = ValidarRequeridas(frm);
            if (validacion != null)
            {
                return validacion;
            }

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), datos.codCliente);
            try
            {
                LimpiarRespuestasSiExisten(connection, datos, frm.id);

                var id_respuesta = connection.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(ID_RESPUESTA), 0) + 1 FROM AFI_BENE_FORM_RESPUESTAS_W");

                foreach (var item in (frm.questions ?? new List<FormQuestion>()).Where(q => q.respuesta != null))
                {
                    GuardarRespuestaPregunta(connection, datos, frm.id, id_respuesta, item);
                }

                return new ErrorDto { Code = 0 };
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Valida que las preguntas requeridas tengan respuesta.
        /// </summary>
        private static ErrorDto? ValidarRequeridas(Form frm)
        {
            foreach (var item in (frm.questions ?? new List<FormQuestion>()).Where(item => item.requerido == true && item.respuesta == null))
            {
                return DbHelper.ErrorResponse("La pregunta " + item.pregunta_titulo + " es requerida");
            }

            return null;
        }

        /// <summary>
        /// Limpia las respuestas previas del socio si ya existían para el beneficio.
        /// </summary>
        private void LimpiarRespuestasSiExisten(SqlConnection connection, FrmFiltros datos, int id_frm)
        {
            const string sql = @"SELECT COUNT(*) FROM AFI_BENE_FORM_RESPUESTAS_W
                                 WHERE COD_BENEFICIO = @cod_beneficio AND ID_BENEFICIO = @id_beneficio AND CEDULA = @socio";
            var count = connection.QueryFirstOrDefault<int>(sql, new { datos.cod_beneficio, datos.id_beneficio, datos.socio });

            if (count > 0)
            {
                LimpiaRespuestasSocio(datos, id_frm);
            }
        }

        /// <summary>
        /// Guarda la respuesta de una pregunta según su tipo e inserta el registro de respuesta.
        /// </summary>
        private void GuardarRespuestaPregunta(SqlConnection connection, FrmFiltros datos, int id_frm, int id_respuesta, FormQuestion item)
        {
            var respuesta = string.Empty;
            var id_opciones = 0;

            switch (item.pregunta_tipo)
            {
                case "text":
                case "textarea":
                case "date":
                case "number":
                case "email":
                    respuesta = item.respuesta?.ToString() ?? string.Empty;
                    break;
                case "select":
                case "multiSelect":
                case "checkbox":
                    id_opciones = GuardarOpcionesRespuesta(datos.codCliente, item.id_frm_pregunta, id_respuesta, datos.usuario ?? string.Empty, item.respuesta!).Result;
                    break;
                case "radio":
                    id_opciones = GuardaOpcionSelectRespuesta(datos.codCliente, item.id_frm_pregunta, id_respuesta, datos.usuario ?? string.Empty, item.respuesta!).Result;
                    break;
            }

            const string sql = @"INSERT INTO [dbo].[AFI_BENE_FORM_RESPUESTAS_W]
                                    (ID_RESPUESTA, ID_FRM, ID_FRM_PREGUNTA, COD_BENEFICIO, ID_BENEFICIO, CEDULA, PREGUNTA_TIPO, ID_OPCIONES_RSP, RESPUESTA_VALOR, REGISTRO_FECHA, REGISTRO_USUARIO)
                                 VALUES
                                    (@id_respuesta, @id_frm, @id_frm_pregunta, @cod_beneficio, @id_beneficio, @socio, @pregunta_tipo, @id_opciones, @respuesta, GETDATE(), @usuario)";
            connection.Execute(sql, new
            {
                id_respuesta,
                id_frm,
                item.id_frm_pregunta,
                datos.cod_beneficio,
                datos.id_beneficio,
                datos.socio,
                item.pregunta_tipo,
                id_opciones,
                respuesta,
                datos.usuario
            });

            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
            {
                EmpresaId = datos.codCliente,
                cod_beneficio = datos.cod_beneficio ?? string.Empty,
                consec = 0,
                movimiento = "Ingresa FrmSocio-Web",
                detalle = $"Ingresa ID_FRM {id_frm} PREGUNTA {item.id_frm_pregunta} BENEFICIO:{datos.cod_beneficio} CEDULA:{datos.socio}",
                registro_usuario = datos.usuario ?? string.Empty
            });
        }

        /// <summary>
        /// Guarda respuestas de opciones (lista de OptionabledQuestion) en AFI_BENE_FORM_OPRESP_W.
        /// </summary>
        private ErrorDto<int> GuardarOpcionesRespuesta(int CodCliente, int id_pregunta, int id_respuesta, string usuario, object question)
        {
            var opciones = DeserializarOpciones(question.ToString());

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var id_opciones = connection.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(ID_OPCIONES), 0) + 1 FROM AFI_BENE_FORM_OPRESP_W");

                foreach (var opcion in opciones)
                {
                    InsertarOpcionRespuesta(connection, id_opciones, opcion.item, opcion.descripcion, id_pregunta, id_respuesta, usuario);
                }

                return DbHelper.CreateOkResponse(id_opciones);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Deserializa el objeto de respuesta como lista de opciones (soporta objeto único).
        /// </summary>
        private static List<OptionabledQuestion> DeserializarOpciones(string? json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new List<OptionabledQuestion>();
            }

            try
            {
                return JsonConvert.DeserializeObject<List<OptionabledQuestion>>(json) ?? new List<OptionabledQuestion>();
            }
            catch (JsonException)
            {
                var single = JsonConvert.DeserializeObject<OptionabledQuestion>(json);
                return single != null ? new List<OptionabledQuestion> { single } : new List<OptionabledQuestion>();
            }
        }

        /// <summary>
        /// Guarda respuestas de tipo string (radio) en AFI_BENE_FORM_OPRESP_W.
        /// </summary>
        private ErrorDto<int> GuardaOpcionSelectRespuesta(int CodCliente, int id_pregunta, int id_respuesta, string usuario, object question)
        {
            var valores = DeserializarValores(JsonConvert.SerializeObject(question));

            using var connection = DbHelper.OpenConnection(CreatePortalDb(), CodCliente);
            try
            {
                var id_opciones = 0;
                foreach (var valor in valores)
                {
                    id_opciones = connection.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(ID_OPCIONES), 0) + 1 FROM AFI_BENE_FORM_OPRESP_W");
                    InsertarOpcionRespuesta(connection, id_opciones, valor, valor, id_pregunta, id_respuesta, usuario);
                }

                return DbHelper.CreateOkResponse(id_opciones);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<int>(ex.Message);
            }
        }

        /// <summary>
        /// Deserializa el objeto de respuesta como lista de strings (soporta string único).
        /// </summary>
        private static List<string> DeserializarValores(string json)
        {
            try
            {
                var single = JsonConvert.DeserializeObject<string>(json);
                return string.IsNullOrEmpty(single) ? new List<string>() : new List<string> { single };
            }
            catch (JsonException)
            {
                return JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
            }
        }

        /// <summary>
        /// Inserta un registro de opción de respuesta.
        /// </summary>
        private static void InsertarOpcionRespuesta(SqlConnection connection, int id_opciones, string? item, string? descripcion,
            int id_pregunta, int id_respuesta, string usuario)
        {
            const string sql = @"INSERT INTO [dbo].[AFI_BENE_FORM_OPRESP_W]
                                    (ID_OPCIONES, ITEM, DESCRIPCION, SELECCION, ID_FRM_PREGUNTA, ID_FRM_RESPUESTA, REGISTRO_FECHA, REGISTRO_USUARIO)
                                 VALUES
                                    (@id_opciones, @item, @descripcion, 1, @id_pregunta, @id_respuesta, GETDATE(), @usuario)";
            connection.Execute(sql, new { id_opciones, item, descripcion, id_pregunta, id_respuesta, usuario });
        }

        /// <summary>
        /// Limpia las respuestas y opciones de respuesta de un socio para un formulario.
        /// </summary>
        /// <param name="datos">Filtros del socio y beneficio.</param>
        /// <param name="id_frm">Identificador del formulario.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto<bool> LimpiaRespuestasSocio(FrmFiltros datos, int id_frm)
        {
            using var connection = DbHelper.OpenConnection(CreatePortalDb(), datos.codCliente);
            try
            {
                const string sqlOpciones = @"SELECT ID_OPCIONES_RSP FROM AFI_BENE_FORM_RESPUESTAS_W
                                             WHERE COD_BENEFICIO = @cod_beneficio AND CEDULA = @socio AND ID_FRM = @id_frm";
                var respOp = connection.Query<int>(sqlOpciones, new { datos.cod_beneficio, datos.socio, id_frm }).ToList();

                foreach (var idOpcion in respOp)
                {
                    connection.Execute("DELETE FROM AFI_BENE_FORM_OPRESP_W WHERE ID_OPCIONES = @idOpcion", new { idOpcion });
                }

                const string sqlResp = @"DELETE FROM AFI_BENE_FORM_RESPUESTAS_W
                                         WHERE COD_BENEFICIO = @cod_beneficio AND ID_BENEFICIO = @id_beneficio AND CEDULA = @socio AND ID_FRM = @id_frm";
                connection.Execute(sqlResp, new { datos.cod_beneficio, datos.id_beneficio, datos.socio, id_frm });

                _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDto
                {
                    EmpresaId = datos.codCliente,
                    cod_beneficio = datos.cod_beneficio ?? string.Empty,
                    consec = 0,
                    movimiento = "Edita Frm Socio-Web",
                    detalle = $"Edita Respuestas Frm Socio {datos.socio} de beneficio {datos.cod_beneficio}",
                    registro_usuario = datos.usuario ?? string.Empty
                });

                return DbHelper.CreateOkResponse(true);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<bool>(ex.Message);
            }
        }
    }
}
