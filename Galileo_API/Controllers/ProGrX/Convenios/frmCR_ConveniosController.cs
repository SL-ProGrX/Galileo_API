using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Convenios;
using Galileo_API.BusinessLogic.ProGrX.Convenios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Convenios
{
    /// <summary>
    /// Expone los servicios del registro de convenios.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCRConveniosController : ControllerBase
    {
        private readonly FrmCRConveniosBL _bl;

        /// <summary>
        /// Inicializa el controlador del registro de convenios.
        /// </summary>
        public FrmCRConveniosController(IConfiguration config)
        {
            _bl = new FrmCRConveniosBL(config);
        }

        #region Base y General

        /// <summary>
        /// Obtiene los datos iniciales de la pantalla.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Inicial_Obtener")]
        public ErrorDto<CrConveniosInicialData> CR_Convenios_Inicial_Obtener(int CodEmpresa)
        {
            return _bl.CR_Convenios_Inicial_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Obtiene la búsqueda F4 de convenios.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_F4_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_F4_Obtener(int CodEmpresa, [FromQuery] CrConveniosCodigoBusquedaRequest? request)
        {
            return _bl.CR_Convenios_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la información general de un convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenio_Obtener")]
        public ErrorDto<CrConvenioDto> CR_Convenio_Obtener(int CodEmpresa, [FromQuery] CrConveniosConvenioRequest? request)
        {
            return _bl.CR_Convenio_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre convenios registrados.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Scroll_Obtener")]
        public ErrorDto<CrConvenioDto> CR_Convenios_Scroll_Obtener(int CodEmpresa, [FromQuery] CrConveniosScrollRequest? request, string? usuario)
        {
            return _bl.CR_Convenios_Scroll_Obtener(CodEmpresa, request, usuario);
        }

        /// <summary>
        /// Obtiene los tipos de convenio activos.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Tipos_Dropdown_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Convenios_Tipos_Dropdown_Obtener(int CodEmpresa)
        {
            return _bl.CR_Convenios_Tipos_Dropdown_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Obtiene los clientes disponibles para asociar al convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Clientes_F4_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Clientes_F4_Obtener(int CodEmpresa, [FromQuery] CrConveniosCodigoBusquedaRequest? request)
        {
            return _bl.CR_Convenios_Clientes_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene los proveedores disponibles para asociar al convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Proveedores_F4_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Proveedores_F4_Obtener(int CodEmpresa, [FromQuery] CrConveniosCodigoBusquedaRequest? request)
        {
            return _bl.CR_Convenios_Proveedores_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Registra o modifica la información general de un convenio.
        /// </summary>
        [Authorize]
        [HttpPost("CR_Convenios_Guardar")]
        public ErrorDto<CrConvenioDto> CR_Convenios_Guardar(int CodEmpresa, string? usuario, [FromBody] CrConveniosGuardarRequest? request)
        {
            return _bl.CR_Convenios_Guardar(CodEmpresa, usuario, request);
        }

        /// <summary>
        /// Elimina un convenio registrado.
        /// </summary>
        [Authorize]
        [HttpDelete("CR_Convenios_Borrar")]
        public ErrorDto CR_Convenios_Borrar(int CodEmpresa, string? usuario, [FromQuery] CrConveniosConvenioRequest? request)
        {
            return _bl.CR_Convenios_Borrar(CodEmpresa, usuario, request);
        }

        #endregion

        #region Recaudación y Devoluciones

        /// <summary>
        /// Obtiene las retenciones de recaudación del convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Recaudacion_Lista_Obtener")]
        public ErrorDto<List<CrConveniosRecaudacionData>> CR_Convenios_Recaudacion_Lista_Obtener(int CodEmpresa, [FromQuery] CrConveniosConvenioRequest? request)
        {
            return _bl.CR_Convenios_Recaudacion_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene las retenciones disponibles para recaudación.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Recaudacion_Retenciones_F4_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Recaudacion_Retenciones_F4_Obtener(int CodEmpresa, [FromQuery] CrConveniosCodigoBusquedaRequest? request)
        {
            return _bl.CR_Convenios_Recaudacion_Retenciones_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre las retenciones disponibles para recaudación.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Recaudacion_Scroll_Obtener")]
        public ErrorDto<CrConveniosF4Data> CR_Convenios_Recaudacion_Scroll_Obtener(int CodEmpresa, [FromQuery] CrConveniosRecaudacionScrollRequest? request)
        {
            return _bl.CR_Convenios_Recaudacion_Scroll_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia o desasocia una retención de recaudación.
        /// </summary>
        [Authorize]
        [HttpPost("CR_Convenios_Recaudacion_Actualizar")]
        public ErrorDto CR_Convenios_Recaudacion_Actualizar(int CodEmpresa, string? usuario, [FromBody] CrConveniosRecaudacionActualizarRequest? request)
        {
            return _bl.CR_Convenios_Recaudacion_Actualizar(CodEmpresa, usuario, request);
        }

        /// <summary>
        /// Obtiene los conceptos de devolución del convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Devoluciones_Lista_Obtener")]
        public ErrorDto<List<CrConveniosDevolucionesData>> CR_Convenios_Devoluciones_Lista_Obtener(int CodEmpresa, [FromQuery] CrConveniosConvenioRequest? request)
        {
            return _bl.CR_Convenios_Devoluciones_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene los conceptos disponibles para devolución.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Devoluciones_Conceptos_F4_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Devoluciones_Conceptos_F4_Obtener(int CodEmpresa, [FromQuery] CrConveniosCodigoBusquedaRequest? request)
        {
            return _bl.CR_Convenios_Devoluciones_Conceptos_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre los conceptos disponibles para devolución.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Devoluciones_Scroll_Obtener")]
        public ErrorDto<CrConveniosF4Data> CR_Convenios_Devoluciones_Scroll_Obtener(int CodEmpresa, [FromQuery] CrConveniosDevolucionesScrollRequest? request)
        {
            return _bl.CR_Convenios_Devoluciones_Scroll_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia o desasocia un concepto de devolución.
        /// </summary>
        [Authorize]
        [HttpPost("CR_Convenios_Devoluciones_Actualizar")]
        public ErrorDto CR_Convenios_Devoluciones_Actualizar(int CodEmpresa, string? usuario, [FromBody] CrConveniosDevolucionesActualizarRequest? request)
        {
            return _bl.CR_Convenios_Devoluciones_Actualizar(CodEmpresa, usuario, request);
        }

        #endregion

        #region Créditos y Cargos

        /// <summary>
        /// Obtiene las líneas o destinos de crédito del convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Creditos_Lista_Obtener")]
        public ErrorDto<List<CrConveniosCreditoData>> CR_Convenios_Creditos_Lista_Obtener(int CodEmpresa, [FromQuery] CrConveniosCreditosListaRequest? request)
        {
            return _bl.CR_Convenios_Creditos_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene líneas o destinos disponibles para crédito.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Creditos_F4_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Creditos_F4_Obtener(int CodEmpresa, [FromQuery] CrConveniosCreditosBusquedaRequest? request)
        {
            return _bl.CR_Convenios_Creditos_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre líneas o destinos de crédito.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Creditos_Scroll_Obtener")]
        public ErrorDto<CrConveniosF4Data> CR_Convenios_Creditos_Scroll_Obtener(int CodEmpresa, [FromQuery] CrConveniosCreditosScrollRequest? request)
        {
            return _bl.CR_Convenios_Creditos_Scroll_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia o desasocia una línea o destino de crédito.
        /// </summary>
        [Authorize]
        [HttpPost("CR_Convenios_Creditos_Actualizar")]
        public ErrorDto CR_Convenios_Creditos_Actualizar(int CodEmpresa, string? usuario, [FromBody] CrConveniosCreditosActualizarRequest? request)
        {
            return _bl.CR_Convenios_Creditos_Actualizar(CodEmpresa, usuario, request);
        }

        /// <summary>
        /// Obtiene los cargos adicionales del convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Cargos_Lista_Obtener")]
        public ErrorDto<List<CrConveniosCargoData>> CR_Convenios_Cargos_Lista_Obtener(int CodEmpresa, [FromQuery] CrConveniosConvenioRequest? request)
        {
            return _bl.CR_Convenios_Cargos_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene los cargos adicionales disponibles.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Cargos_F4_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Cargos_F4_Obtener(int CodEmpresa, [FromQuery] CrConveniosCodigoBusquedaRequest? request)
        {
            return _bl.CR_Convenios_Cargos_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre los cargos adicionales disponibles.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Cargos_Scroll_Obtener")]
        public ErrorDto<CrConveniosF4Data> CR_Convenios_Cargos_Scroll_Obtener(int CodEmpresa, [FromQuery] CrConveniosCargosScrollRequest? request)
        {
            return _bl.CR_Convenios_Cargos_Scroll_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia o desasocia un cargo adicional.
        /// </summary>
        [Authorize]
        [HttpPost("CR_Convenios_Cargos_Actualizar")]
        public ErrorDto CR_Convenios_Cargos_Actualizar(int CodEmpresa, string? usuario, [FromBody] CrConveniosCargosActualizarRequest? request)
        {
            return _bl.CR_Convenios_Cargos_Actualizar(CodEmpresa, usuario, request);
        }

        #endregion

        #region Rebajas de CxP y Ahorros

        /// <summary>
        /// Obtiene los cargos de CxP aplicables al convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_RebajasCxP_Lista_Obtener")]
        public ErrorDto<List<CrConveniosRebajasCxpData>> CR_Convenios_RebajasCxP_Lista_Obtener(int CodEmpresa, [FromQuery] CrConveniosConvenioRequest? request)
        {
            return _bl.CR_Convenios_RebajasCxP_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia o desasocia un cargo de CxP.
        /// </summary>
        [Authorize]
        [HttpPost("CR_Convenios_RebajasCxP_Actualizar")]
        public ErrorDto CR_Convenios_RebajasCxP_Actualizar(int CodEmpresa, string? usuario, [FromBody] CrConveniosRebajasCxpActualizarRequest? request)
        {
            return _bl.CR_Convenios_RebajasCxP_Actualizar(CodEmpresa, usuario, request);
        }

        /// <summary>
        /// Obtiene los planes de ahorro asociados al convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Ahorros_Lista_Obtener")]
        public ErrorDto<List<CrConveniosAhorrosData>> CR_Convenios_Ahorros_Lista_Obtener(int CodEmpresa, [FromQuery] CrConveniosConvenioRequest? request)
        {
            return _bl.CR_Convenios_Ahorros_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene los planes de ahorro disponibles.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Ahorros_Planes_F4_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Ahorros_Planes_F4_Obtener(int CodEmpresa, [FromQuery] CrConveniosAhorrosPlanesBusquedaRequest? request)
        {
            return _bl.CR_Convenios_Ahorros_Planes_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre los planes de ahorro disponibles.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Ahorros_Planes_Scroll_Obtener")]
        public ErrorDto<CrConveniosAhorroPlanData> CR_Convenios_Ahorros_Planes_Scroll_Obtener(int CodEmpresa, [FromQuery] CrConveniosAhorrosScrollRequest? request)
        {
            return _bl.CR_Convenios_Ahorros_Planes_Scroll_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene contratos de ahorro disponibles para el convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Ahorros_Contratos_F4_Obtener")]
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Ahorros_Contratos_F4_Obtener(int CodEmpresa, [FromQuery] CrConveniosAhorrosContratosBusquedaRequest? request)
        {
            return _bl.CR_Convenios_Ahorros_Contratos_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia, modifica o desasocia un plan de ahorro.
        /// </summary>
        [Authorize]
        [HttpPost("CR_Convenios_Ahorros_Actualizar")]
        public ErrorDto CR_Convenios_Ahorros_Actualizar(int CodEmpresa, string? usuario, [FromBody] CrConveniosAhorrosActualizarRequest? request)
        {
            return _bl.CR_Convenios_Ahorros_Actualizar(CodEmpresa, usuario, request);
        }

        #endregion

        #region Órdenes y Reservas

        /// <summary>
        /// Obtiene las órdenes de pago procesadas del convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Ordenes_Lista_Obtener")]
        public ErrorDto<List<CrConveniosOrdenData>> CR_Convenios_Ordenes_Lista_Obtener(int CodEmpresa, [FromQuery] CrConveniosOrdenesListaRequest? request)
        {
            return _bl.CR_Convenios_Ordenes_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene el historial de reservas del convenio.
        /// </summary>
        [Authorize]
        [HttpGet("CR_Convenios_Reservas_Lista_Obtener")]
        public ErrorDto<List<CrConveniosReservaData>> CR_Convenios_Reservas_Lista_Obtener(int CodEmpresa, [FromQuery] CrConveniosConvenioRequest? request)
        {
            return _bl.CR_Convenios_Reservas_Lista_Obtener(CodEmpresa, request);
        }

        #endregion
    }
}