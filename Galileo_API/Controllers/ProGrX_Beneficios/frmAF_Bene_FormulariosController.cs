using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models.AF;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Beneficios;

namespace Galileo_API.Controllers.ProGrX_Beneficios
{
    /// <summary>
    /// Endpoints del Constructor de Formularios de Beneficios (frmAF_Bene_Formularios).
    /// </summary>
    [Route("api/frmAF_Bene_Formularios")]
    [ApiController]
    public class FrmAfBeneFormulariosController : ControllerBase
    {
        private readonly FrmAfBeneFormulariosBL _bl;

        public FrmAfBeneFormulariosController(IConfiguration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            _bl = new FrmAfBeneFormulariosBL(config);
        }

        /// <summary>Lista de formularios de un beneficio.</summary>
        [Authorize]
        [HttpGet("AfBeneFormulario_Obtener")]
        public ErrorDto<List<Formulario>> AfBeneFormulario_Obtener(int CodCliente, string cod_beneficio)
            => _bl.AfBeneFormulario_Obtener(CodCliente, cod_beneficio);

        /// <summary>Preguntas de un formulario.</summary>
        [Authorize]
        [HttpGet("AfBeneFormularioPregunta_Obtener")]
        public ErrorDto<Form> AfBeneFormularioPregunta_Obtener(int CodCliente, int id_form)
            => _bl.AfBeneFormularioPregunta_Obtener(CodCliente, id_form);

        /// <summary>Opciones de una pregunta.</summary>
        [Authorize]
        [HttpGet("AfBeneFormularioOpciones_Obtener")]
        public ErrorDto<List<OptionabledQuestion>> AfBeneFormularioOpciones_Obtener(int CodCliente, int id_frm_pregunta)
            => _bl.AfBeneFormularioOpciones_Obtener(CodCliente, id_frm_pregunta);

        /// <summary>Formularios activos de un beneficio con respuestas del socio.</summary>
        [Authorize]
        [HttpGet("AfBeneFormSocios_Obtener")]
        public ErrorDto<List<Formulario>> AfBeneFormSocios_Obtener(string formulario)
            => _bl.AfBeneFormSocios_Obtener(formulario);

        /// <summary>Reporte pivote de respuestas de un formulario.</summary>
        [Authorize]
        [HttpPost("AfBeneficiosReporte_Obtener")]
        public ErrorDto<object> AfBeneficiosReporte_Obtener([FromBody] FrmReporteDatos datos)
            => _bl.AfBeneficiosReporte_Obtener(datos);

        /// <summary>Reporte de respuestas de un formulario por socio.</summary>
        [Authorize]
        [HttpPost("AfBeneficiosReporteSocio_Obtener")]
        public ErrorDto<List<ReporteFormularioDatos>> AfBeneficiosReporteSocio_Obtener([FromBody] FrmReporteDatos datos)
            => _bl.AfBeneficiosReporteSocio_Obtener(datos);

        /// <summary>Agrega un formulario nuevo con preguntas y opciones.</summary>
        [Authorize]
        [HttpPost("AfBeneFormularios_Agregar")]
        public ErrorDto AfBeneFormularios_Agregar(int CodCliente, [FromBody] string formulario)
            => _bl.AfBeneFormularios_Agregar(CodCliente, formulario);

        /// <summary>Agrega las respuestas de un formulario por socio.</summary>
        [Authorize]
        [HttpPost("AfBeneFrmRespuesta_Agregar")]
        public ErrorDto AfBeneFrmRespuesta_Agregar(string datos, [FromBody] Form frm)
            => _bl.AfBeneFrmRespuesta_Agregar(datos, frm);

        /// <summary>Limpia las respuestas de un socio para un formulario.</summary>
        [Authorize]
        [HttpPost("LimpiaRespuestasSocio")]
        public ErrorDto<bool> LimpiaRespuestasSocio(int id_frm, [FromBody] FrmFiltros datos)
            => _bl.LimpiaRespuestasSocio(datos, id_frm);

        /// <summary>Actualiza el encabezado de un formulario.</summary>
        [Authorize]
        [HttpPut("AfBeneFrmEncabezado_Actualizar")]
        public ErrorDto AfBeneFrmEncabezado_Actualizar(int CodCliente, [FromBody] string formulario)
            => _bl.AfBeneFrmEncabezado_Actualizar(CodCliente, formulario);

        /// <summary>Actualiza una pregunta del formulario.</summary>
        [Authorize]
        [HttpPut("AfBeneFrmDetalle_Actualizar")]
        public ErrorDto AfBeneFrmDetalle_Actualizar(int CodCliente, int id_from, string usuario, [FromBody] string formulario)
            => _bl.AfBeneFrmDetalle_Actualizar(CodCliente, id_from, usuario, formulario);

        /// <summary>Actualiza una opción de una pregunta.</summary>
        [Authorize]
        [HttpPut("AfBeneFrmOpciones_Actualizar")]
        public ErrorDto AfBeneFrmOpciones_Actualizar(int CodCliente, int frm_pregunta, string usuario, [FromBody] string opcion)
            => _bl.AfBeneFrmOpciones_Actualizar(CodCliente, frm_pregunta, usuario, opcion);

        /// <summary>Elimina un formulario.</summary>
        [Authorize]
        [HttpDelete("AfBeneFormulario_Eliminar")]
        public ErrorDto AfBeneFormulario_Eliminar(int CodCliente, int if_frm)
            => _bl.AfBeneFormulario_Eliminar(CodCliente, if_frm);

        /// <summary>Elimina una pregunta del formulario.</summary>
        [Authorize]
        [HttpDelete("AfBeneFrmDetalle_Elimina")]
        public ErrorDto AfBeneFrmDetalle_Elimina(int CodCliente, int frm_pregunta, string usuario)
            => _bl.AfBeneFrmDetalle_Elimina(CodCliente, frm_pregunta, usuario);

        /// <summary>Elimina una opción de una pregunta.</summary>
        [Authorize]
        [HttpDelete("AfBeneFrmOpciones_Eliminar")]
        public ErrorDto AfBeneFrmOpciones_Eliminar(int CodCliente, int frm_pregunta, int id_opciones, string usuario)
            => _bl.AfBeneFrmOpciones_Eliminar(CodCliente, frm_pregunta, id_opciones, usuario);
    }
}
