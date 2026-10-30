
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_RRHH;
using Galileo_API.BusinessLogic.ProGrX_RRHH;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_RRHH
{
    /// <summary>
    /// Expone los servicios del catálogo
    /// de conceptos de Recursos Humanos.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FrmRHCatConceptosController : ControllerBase
    {
        private readonly FrmRHCatConceptosBL _bl;

        /// <summary>
        /// Inicializa el controlador del catálogo
        /// de conceptos de Recursos Humanos.
        /// </summary>
        /// <param name="config"></param>
        public FrmRHCatConceptosController(IConfiguration config)
        {
            _bl = new FrmRHCatConceptosBL(config);
        }

        #region Conceptos

        /// <summary>
        /// Obtiene la información inicial del catálogo de conceptos,
        /// incluyendo las opciones de los selectores.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_Cat_Conceptos_Inicial_Obtener")]
        public ErrorDto<RhCatConceptosInicialData>
            RH_Cat_Conceptos_Inicial_Obtener(
                int CodEmpresa)
        {
            return _bl.RH_Cat_Conceptos_Inicial_Obtener(
                CodEmpresa);
        }

        /// <summary>
        /// Obtiene la información general de un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContabilidad"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_Cat_Conceptos_Obtener")]
        public ErrorDto<RhCatConceptoData>
            RH_Cat_Conceptos_Obtener(
                int CodEmpresa,
                int CodContabilidad,
                [FromQuery] RhCatConceptoObtenerRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Obtener(
                CodEmpresa,
                CodContabilidad,
                request);
        }

        /// <summary>
        /// Obtiene los conceptos disponibles para el buscador F4.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_Cat_Conceptos_F4_Obtener")]
        public ErrorDto<List<RhCatConceptoBusquedaData>>
            RH_Cat_Conceptos_F4_Obtener(
                int CodEmpresa,
                [FromQuery] RhCatConceptoBusquedaRequest? request)
        {
            return _bl.RH_Cat_Conceptos_F4_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Navega entre los conceptos registrados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContabilidad"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_Cat_Conceptos_Scroll_Obtener")]
        public ErrorDto<RhCatConceptoData>
            RH_Cat_Conceptos_Scroll_Obtener(
                int CodEmpresa,
                int CodContabilidad,
                [FromQuery] RhCatConceptoScrollRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Scroll_Obtener(
                CodEmpresa,
                CodContabilidad,
                request);
        }

        /// <summary>
        /// Genera el siguiente código para un concepto nuevo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_Cat_Conceptos_Codigo_Nuevo")]
        public ErrorDto<RhCatConceptoCodigoNuevoData>
            RH_Cat_Conceptos_Codigo_Nuevo(
                int CodEmpresa)
        {
            return _bl.RH_Cat_Conceptos_Codigo_Nuevo(
                CodEmpresa);
        }

        /// <summary>
        /// Genera la siguiente prioridad para el tipo
        /// de concepto indicado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_Cat_Conceptos_Prioridad_Nueva")]
        public ErrorDto<RhCatConceptoPrioridadNuevaData>
            RH_Cat_Conceptos_Prioridad_Nueva(
                int CodEmpresa,
                [FromQuery] RhCatConceptoPrioridadNuevaRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Prioridad_Nueva(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Registra o modifica un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CodContabilidad"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RH_Cat_Conceptos_Guardar")]
        public ErrorDto RH_Cat_Conceptos_Guardar(
            int CodEmpresa,
            int CodContabilidad,
            string? usuario,
            [FromBody] RhCatConceptoGuardarRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Guardar(
                CodEmpresa,
                CodContabilidad,
                usuario,
                request);
        }

        /// <summary>
        /// Valida si un código de concepto está disponible
        /// para registrar un concepto nuevo.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_Cat_Conceptos_Codigo_Validar")]
        public ErrorDto RH_Cat_Conceptos_Codigo_Validar(
            int CodEmpresa,
            [FromQuery] RhCatConceptoCodigoValidarRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Codigo_Validar(
                CodEmpresa,
                request);
        }

        #endregion

        #region Tablas

        /// <summary>
        /// Obtiene los rangos de aplicación de un concepto.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_Cat_Conceptos_Tabla_Lista_Obtener")]
        public ErrorDto<RhCatConceptoTablasLista>
            RH_Cat_Conceptos_Tabla_Lista_Obtener(
                int CodEmpresa,
                [FromQuery] RhCatConceptoTablaListaRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Tabla_Lista_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Registra o modifica un rango de la tabla de un concepto.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RH_Cat_Conceptos_Tabla_Guardar")]
        public ErrorDto RH_Cat_Conceptos_Tabla_Guardar(
            int CodEmpresa,
            string? usuario,
            [FromBody] RhCatConceptoTablaGuardarRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Tabla_Guardar(
                CodEmpresa,
                usuario,
                request);
        }

        /// <summary>
        /// Elimina un rango de la tabla de un concepto.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RH_Cat_Conceptos_Tabla_Eliminar")]
        public ErrorDto RH_Cat_Conceptos_Tabla_Eliminar(
            int CodEmpresa,
            string? usuario,
            [FromBody] RhCatConceptoTablaEliminarRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Tabla_Eliminar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion

        #region Detalle

        /// <summary>
        /// Obtiene los detalles asociados a un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_Cat_Conceptos_Detalle_Lista_Obtener")]
        public ErrorDto<RhCatConceptoDetallesLista>
            RH_Cat_Conceptos_Detalle_Lista_Obtener(
                int CodEmpresa,
                [FromQuery] RhCatConceptoDetalleListaRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Detalle_Lista_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Registra o modifica un detalle de un concepto.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RH_Cat_Conceptos_Detalle_Guardar")]
        public ErrorDto RH_Cat_Conceptos_Detalle_Guardar(
            int CodEmpresa,
            string? usuario,
            [FromBody] RhCatConceptoDetalleGuardarRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Detalle_Guardar(
                CodEmpresa,
                usuario,
                request);
        }

        /// <summary>
        /// Elimina un detalle de un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RH_Cat_Conceptos_Detalle_Eliminar")]
        public ErrorDto RH_Cat_Conceptos_Detalle_Eliminar(
            int CodEmpresa,
            string? usuario,
            [FromBody] RhCatConceptoDetalleEliminarRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Detalle_Eliminar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion

        #region Exclusiones

        /// <summary>
        /// Obtiene los conceptos excluidos de un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_Cat_Conceptos_Exclusion_Lista_Obtener")]
        public ErrorDto<RhCatConceptoExclusionesLista>
            RH_Cat_Conceptos_Exclusion_Lista_Obtener(
                int CodEmpresa,
                [FromQuery] RhCatConceptoExclusionListaRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Exclusion_Lista_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Registra la exclusión de un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RH_Cat_Conceptos_Exclusion_Guardar")]
        public ErrorDto RH_Cat_Conceptos_Exclusion_Guardar(
            int CodEmpresa,
            string? usuario,
            [FromBody] RhCatConceptoExclusionGuardarRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Exclusion_Guardar(
                CodEmpresa,
                usuario,
                request);
        }

        /// <summary>
        /// Elimina la exclusión de un concepto de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RH_Cat_Conceptos_Exclusion_Eliminar")]
        public ErrorDto RH_Cat_Conceptos_Exclusion_Eliminar(
            int CodEmpresa,
            string? usuario,
            [FromBody] RhCatConceptoExclusionEliminarRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Exclusion_Eliminar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion

        #region Actualizar Personas

        /// <summary>
        /// Actualiza el concepto de nómina en todas
        /// las personas vinculadas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RH_Cat_Conceptos_Personas_Actualizar")]
        public ErrorDto RH_Cat_Conceptos_Personas_Actualizar(
            int CodEmpresa,
            string? usuario,
            [FromBody] RhCatConceptoPersonasActualizarRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Personas_Actualizar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion

        /// <summary>
        /// Obtiene las entidades relacionadas disponibles
        /// para el buscador F4 de conceptos de nómina.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_Cat_Conceptos_Entidades_F4_Obtener")]
        public ErrorDto<List<RhCatConceptoEntidadBusquedaData>>
            RH_Cat_Conceptos_Entidades_F4_Obtener(
                int CodEmpresa,
                [FromQuery] RhCatConceptoEntidadBusquedaRequest? request)
        {
            return _bl.RH_Cat_Conceptos_Entidades_F4_Obtener(
                CodEmpresa,
                request);
        }

    }
}
