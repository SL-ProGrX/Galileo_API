using Galileo.Models.AF;
using System.Text.Json;

namespace Galileo.DataBaseTier.ProGrX_Beneficios
{
    public partial class ZohoDB
    {
        /// <summary>
        /// Actualiza el mensaje de error en la tabla de tickets.
        /// </summary>
        private void ActualizaError(int CodEmpresa, string ticket, string error, string usuario)
        {
            const string sql = @"UPDATE AFI_BENE_OTORGA_INT SET MSJ_INTERFACE = @error,
                                     ESTADO = 'E', VISTO_POR = @usuario, I_VISTO = 1, VISTO_FECHA = getdate()
                                 WHERE ID_ZOHO = @ticket";

            DbHelper.ExecuteNonQuery(CreatePortalDb(), CodEmpresa, sql, new { error, usuario, ticket });
        }

        /// <summary>
        /// Incluye las respuestas de los formularios homologados del beneficio en la base de datos.
        /// </summary>
        private void IncluirRespuestasFormularios(FrmFiltros filtros, Dictionary<string, JsonElement> datos)
        {
            var frmRespuestas = new FrmAfBeneFormulariosDB(_config);
            var jDatos = Newtonsoft.Json.JsonConvert.SerializeObject(filtros);
            var formularios = frmRespuestas.AfBeneFormSocios_Obtener(jDatos).Result ?? [];

            foreach (var item in formularios)
            {
                var form = new Form
                {
                    id = item.id_form,
                    questions = item.formulario.questions
                };

                foreach (var question in item.formulario.questions ?? [])
                {
                    question.respuesta = FormularioRespuesta_Obtener(question, datos);
                }

                frmRespuestas.AfBeneFrmRespuesta_Agregar(jDatos, form);
            }
        }

        /// <summary>
        /// Resuelve la respuesta homologada o el valor alternativo de una pregunta requerida.
        /// </summary>
        private static object? FormularioRespuesta_Obtener(
            FormQuestion question,
            Dictionary<string, JsonElement> datos)
        {
            if (!string.IsNullOrEmpty(question.campo_homologado) &&
                datos.TryGetValue(question.campo_homologado, out var element))
            {
                var value = element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                    ? null
                    : element.ToString();
                return RegresaRespuesta(question, value);
            }

            if (question.requerido != true)
            {
                return null;
            }

            var defaultValue = question.opciones?.FirstOrDefault() ?? (object)"NA";
            return RegresaRespuesta(question, defaultValue);
        }

        /// <summary>
        /// Convierte la respuesta homologada al formato esperado por el tipo de pregunta del formulario.
        /// </summary>
        private static object? RegresaRespuesta(FormQuestion question, object? value)
        {
            return question.pregunta_tipo switch
            {
                "radio" => question.opciones?.FirstOrDefault()?.item,
                "text" or "textarea" or "date" or "number" or "email" => value?.ToString() ?? string.Empty,
                "select" or "multiSelect" or "checkbox" => RespuestasMultiples_Serializar(question, value),
                _ => null
            };
        }

        /// <summary>
        /// Serializa las opciones homologadas seleccionadas para preguntas de selección.
        /// </summary>
        private static string RespuestasMultiples_Serializar(FormQuestion question, object? value)
        {
            var respuestas = (value?.ToString() ?? string.Empty)
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var seleccionadas = (question.opciones ?? [])
                .Where(opcion => respuestas.Any(respuesta =>
                    (opcion.descripcion ?? string.Empty).Contains(
                        respuesta,
                        StringComparison.OrdinalIgnoreCase)))
                .Select(opcion => new OptionabledQuestion
                {
                    id_opciones = opcion.id_opciones,
                    item = opcion.item,
                    descripcion = opcion.descripcion,
                    selected = true
                })
                .ToList();

            return Newtonsoft.Json.JsonConvert.SerializeObject(seleccionadas);
        }
    }
}
