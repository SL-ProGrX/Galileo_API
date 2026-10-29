using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_RRHH;
using Galileo_API.BusinessLogic.ProGrX_RRHH;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_RRHH
{
    /// <summary>
    /// Expone los servicios del catálogo
    /// de entidades relacionadas de Recursos Humanos.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FrmRHCatEntidadesController : ControllerBase
    {
        private readonly FrmRHCatEntidadesBL _bl;

        /// <summary>
        /// Inicializa el controlador del catálogo
        /// de entidades relacionadas.
        /// </summary>
        /// <param name="config"></param>
        public FrmRHCatEntidadesController(IConfiguration config)
        {
            _bl = new FrmRHCatEntidadesBL(config);
        }

        #region Entidades

        /// <summary>
        /// Obtiene la información general de una entidad relacionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_CatEntidades_Obtener")]
        public ErrorDto<RhCatEntidadData> RH_CatEntidades_Obtener(
            int CodEmpresa,
            [FromQuery] RhCatEntidadObtenerRequest? request)
        {
            return _bl.RH_CatEntidades_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Navega entre las entidades relacionadas registradas.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_CatEntidades_Scroll_Obtener")]
        public ErrorDto<RhCatEntidadData> RH_CatEntidades_Scroll_Obtener(
            int CodEmpresa,
            [FromQuery] RhCatEntidadScrollRequest? request)
        {
            return _bl.RH_CatEntidades_Scroll_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Obtiene las entidades disponibles para el buscador F4.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_CatEntidades_Busqueda_Obtener")]
        public ErrorDto<List<RhCatEntidadBusquedaData>>
            RH_CatEntidades_Busqueda_Obtener(
                int CodEmpresa,
                [FromQuery] RhCatEntidadBusquedaRequest? request)
        {
            return _bl.RH_CatEntidades_Busqueda_Obtener(
                CodEmpresa,
                request);
        }

        /// <summary>
        /// Registra o modifica una entidad relacionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RH_CatEntidades_Guardar")]
        [ValidateAntiForgeryToken]
        public ErrorDto RH_CatEntidades_Guardar(
            int CodEmpresa,
            string? usuario,
            [FromBody] RhCatEntidadGuardarRequest? request)
        {
            return _bl.RH_CatEntidades_Guardar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion

        #region Bancos y cuentas

        /// <summary>
        /// Obtiene los bancos disponibles para el usuario
        /// según la configuración de tesorería.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_CatEntidades_Bancos_Obtener")]
        public ErrorDto<List<RhCatEntidadBancoData>>
            RH_CatEntidades_Bancos_Obtener(
                int CodEmpresa,
                string? usuario)
        {
            return _bl.RH_CatEntidades_Bancos_Obtener(
                CodEmpresa,
                usuario);
        }

        /// <summary>
        /// Obtiene las cuentas bancarias asociadas
        /// a la identificación de una entidad.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_CatEntidades_Cuentas_Obtener")]
        public ErrorDto<RhCatEntidadCuentasLista>
            RH_CatEntidades_Cuentas_Obtener(
                int CodEmpresa,
                string? identificacion)
        {
            return _bl.RH_CatEntidades_Cuentas_Obtener(
                CodEmpresa,
                identificacion);
        }

        #endregion

        #region Conceptos

        /// <summary>
        /// Obtiene los conceptos activos disponibles
        /// y asignados a una entidad relacionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codEr"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet("RH_CatEntidades_Conceptos_Obtener")]
        public ErrorDto<RhCatEntidadConceptosLista>
            RH_CatEntidades_Conceptos_Obtener(
                int CodEmpresa,
                string? codEr)
        {
            return _bl.RH_CatEntidades_Conceptos_Obtener(
                CodEmpresa,
                codEr);
        }

        /// <summary>
        /// Asigna o desasigna un concepto de nómina
        /// a una entidad relacionada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RH_CatEntidades_Concepto_Actualizar")]
        [ValidateAntiForgeryToken]
        public ErrorDto RH_CatEntidades_Concepto_Actualizar(
            int CodEmpresa,
            string? usuario,
            [FromBody] RhCatEntidadConceptoActualizarRequest? request)
        {
            return _bl.RH_CatEntidades_Concepto_Actualizar(
                CodEmpresa,
                usuario,
                request);
        }

        #endregion
    }
}