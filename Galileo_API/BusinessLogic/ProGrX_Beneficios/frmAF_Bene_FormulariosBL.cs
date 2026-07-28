using Galileo.DataBaseTier.ProGrX_Beneficios;
using Galileo.Models.AF;
using Galileo.Models.ERROR;

namespace Galileo_API.BusinessLogic.ProGrX_Beneficios
{
    /// <summary>
    /// Lógica de negocio del Constructor de Formularios de Beneficios (frmAF_Bene_Formularios).
    /// </summary>
    public class FrmAfBeneFormulariosBL
    {
        private readonly FrmAfBeneFormulariosDB _db;

        public FrmAfBeneFormulariosBL(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _db = new FrmAfBeneFormulariosDB(config);
        }

        /// <summary>Lista de formularios de un beneficio.</summary>
        public ErrorDto<List<Formulario>> AfBeneFormulario_Obtener(int CodCliente, string cod_beneficio)
            => _db.AfBeneFormulario_Obtener(CodCliente, cod_beneficio);

        /// <summary>Preguntas de un formulario.</summary>
        public ErrorDto<Form> AfBeneFormularioPregunta_Obtener(int CodCliente, int id_form)
            => _db.AfBeneFormularioPregunta_Obtener(CodCliente, id_form);

        /// <summary>Opciones de una pregunta.</summary>
        public ErrorDto<List<OptionabledQuestion>> AfBeneFormularioOpciones_Obtener(int CodCliente, int id_frm_pregunta)
            => _db.AfBeneFormularioOpciones_Obtener(CodCliente, id_frm_pregunta);

        /// <summary>Formularios activos de un beneficio con respuestas del socio.</summary>
        public ErrorDto<List<Formulario>> AfBeneFormSocios_Obtener(string Jformulario)
            => _db.AfBeneFormSocios_Obtener(Jformulario);

        /// <summary>Limpia las respuestas de un socio para un formulario.</summary>
        public ErrorDto<bool> LimpiaRespuestasSocio(FrmFiltros datos, int id_frm)
            => _db.LimpiaRespuestasSocio(datos, id_frm);

        /// <summary>Reporte pivote de respuestas de un formulario.</summary>
        public ErrorDto<object> AfBeneficiosReporte_Obtener(FrmReporteDatos datos)
            => _db.AfBeneficiosReporte_Obtener(datos);

        /// <summary>Reporte de respuestas de un formulario por socio.</summary>
        public ErrorDto<List<ReporteFormularioDatos>> AfBeneficiosReporteSocio_Obtener(FrmReporteDatos datos)
            => _db.AfBeneficiosReporteSocio_Obtener(datos);

        /// <summary>Agrega un formulario nuevo con preguntas y opciones.</summary>
        public ErrorDto AfBeneFormularios_Agregar(int CodCliente, string formulario)
            => _db.AfBeneFormularios_Agregar(CodCliente, formulario);

        /// <summary>Agrega las respuestas de un formulario por socio.</summary>
        public ErrorDto AfBeneFrmRespuesta_Agregar(string Jdatos, Form frm)
            => _db.AfBeneFrmRespuesta_Agregar(Jdatos, frm);

        /// <summary>Actualiza el encabezado de un formulario.</summary>
        public ErrorDto AfBeneFrmEncabezado_Actualizar(int CodCliente, string formulario)
            => _db.AfBeneFrmEncabezado_Actualizar(CodCliente, formulario);

        /// <summary>Elimina un formulario.</summary>
        public ErrorDto AfBeneFormulario_Eliminar(int CodCliente, int if_frm)
            => _db.AfBeneFormulario_Eliminar(CodCliente, if_frm);

        /// <summary>Actualiza una pregunta del formulario.</summary>
        public ErrorDto AfBeneFrmDetalle_Actualizar(int CodCliente, int id_from, string usuario, string formulario)
            => _db.AfBeneFrmDetalle_Actualizar(CodCliente, id_from, usuario, formulario);

        /// <summary>Elimina una pregunta del formulario.</summary>
        public ErrorDto AfBeneFrmDetalle_Elimina(int CodCliente, int frm_pregunta, string usuario)
            => _db.AfBeneFrmDetalle_Elimina(CodCliente, frm_pregunta, usuario);

        /// <summary>Actualiza una opción de una pregunta.</summary>
        public ErrorDto AfBeneFrmOpciones_Actualizar(int CodCliente, int frm_pregunta, string usuario, string opcion)
            => _db.AfBeneFrmOpciones_Actualizar(CodCliente, frm_pregunta, usuario, opcion);

        /// <summary>Elimina una opción de una pregunta.</summary>
        public ErrorDto AfBeneFrmOpciones_Eliminar(int CodCliente, int frm_pregunta, int id_opciones, string usuario)
            => _db.AfBeneFrmOpciones_Eliminar(CodCliente, frm_pregunta, id_opciones, usuario);
    }
}
