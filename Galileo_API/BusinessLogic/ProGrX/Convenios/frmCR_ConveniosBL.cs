using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Convenios;
using Galileo_API.DataBaseTier.ProGrX.Convenios;

namespace Galileo_API.BusinessLogic.ProGrX.Convenios
{
    /// <summary>
    /// Coordina las operaciones de negocio del registro de convenios.
    /// </summary>
    public class FrmCRConveniosBL
    {
        private readonly FrmCRConveniosDB _db;

        /// <summary>
        /// Inicializa la capa de negocio del registro de convenios.
        /// </summary>
        public FrmCRConveniosBL(IConfiguration config)
        {
            _db = new FrmCRConveniosDB(config);
        }

        #region Base y General

        /// <summary>
        /// Obtiene los datos iniciales del registro de convenios.
        /// </summary>
        public ErrorDto<CrConveniosInicialData> CR_Convenios_Inicial_Obtener(int CodEmpresa)
        {
            return _db.CR_Convenios_Inicial_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Obtiene la lista F4 de convenios.
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            return _db.CR_Convenios_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene la información general de un convenio.
        /// </summary>
        public ErrorDto<CrConvenioDto> CR_Convenio_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            return _db.CR_Convenio_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre convenios registrados.
        /// </summary>
        public ErrorDto<CrConvenioDto> CR_Convenios_Scroll_Obtener(int CodEmpresa, CrConveniosScrollRequest? request, string? usuario)
        {
            return _db.CR_Convenios_Scroll_Obtener(CodEmpresa, request, usuario);
        }

        /// <summary>
        /// Obtiene los tipos de convenio activos.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_Convenios_Tipos_Dropdown_Obtener(int CodEmpresa)
        {
            return _db.CR_Convenios_Tipos_Dropdown_Obtener(CodEmpresa);
        }

        /// <summary>
        /// Obtiene los clientes disponibles para asociar al convenio.
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Clientes_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            return _db.CR_Convenios_Clientes_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene los proveedores disponibles para asociar al convenio.
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Proveedores_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            return _db.CR_Convenios_Proveedores_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Registra o modifica la información general del convenio.
        /// </summary>
        public ErrorDto<CrConvenioDto> CR_Convenios_Guardar(int CodEmpresa, string? usuario, CrConveniosGuardarRequest? request)
        {
            return _db.CR_Convenios_Guardar(CodEmpresa, usuario, request);
        }

        /// <summary>
        /// Elimina un convenio registrado.
        /// </summary>
        public ErrorDto CR_Convenios_Borrar(int CodEmpresa, string? usuario, CrConveniosConvenioRequest? request)
        {
            return _db.CR_Convenios_Borrar(CodEmpresa, usuario, request);
        }

        #endregion

        #region Recaudación y Devoluciones

        /// <summary>
        /// Obtiene las retenciones de recaudación del convenio.
        /// </summary>
        public ErrorDto<List<CrConveniosRecaudacionData>> CR_Convenios_Recaudacion_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            return _db.CR_Convenios_Recaudacion_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene las retenciones disponibles para recaudación.
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Recaudacion_Retenciones_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            return _db.CR_Convenios_Recaudacion_Retenciones_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre las retenciones disponibles para recaudación.
        /// </summary>
        public ErrorDto<CrConveniosF4Data> CR_Convenios_Recaudacion_Scroll_Obtener(int CodEmpresa, CrConveniosRecaudacionScrollRequest? request)
        {
            return _db.CR_Convenios_Recaudacion_Scroll_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia o desasocia una retención de recaudación.
        /// </summary>
        public ErrorDto CR_Convenios_Recaudacion_Actualizar(int CodEmpresa, string? usuario, CrConveniosRecaudacionActualizarRequest? request)
        {
            return _db.CR_Convenios_Recaudacion_Actualizar(CodEmpresa, usuario, request);
        }

        /// <summary>
        /// Obtiene los conceptos de devolución del convenio.
        /// </summary>
        public ErrorDto<List<CrConveniosDevolucionesData>> CR_Convenios_Devoluciones_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            return _db.CR_Convenios_Devoluciones_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene los conceptos disponibles para devolución.
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Devoluciones_Conceptos_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            return _db.CR_Convenios_Devoluciones_Conceptos_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre los conceptos disponibles para devolución.
        /// </summary>
        public ErrorDto<CrConveniosF4Data> CR_Convenios_Devoluciones_Scroll_Obtener(int CodEmpresa, CrConveniosDevolucionesScrollRequest? request)
        {
            return _db.CR_Convenios_Devoluciones_Scroll_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia o desasocia un concepto de devolución.
        /// </summary>
        public ErrorDto CR_Convenios_Devoluciones_Actualizar(int CodEmpresa, string? usuario, CrConveniosDevolucionesActualizarRequest? request)
        {
            return _db.CR_Convenios_Devoluciones_Actualizar(CodEmpresa, usuario, request);
        }

        #endregion

        #region Créditos y Cargos

        /// <summary>
        /// Obtiene las líneas o destinos de crédito asociados al convenio.
        /// </summary>
        public ErrorDto<List<CrConveniosCreditoData>> CR_Convenios_Creditos_Lista_Obtener(int CodEmpresa, CrConveniosCreditosListaRequest? request)
        {
            return _db.CR_Convenios_Creditos_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene líneas o destinos disponibles para crédito.
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Creditos_F4_Obtener(int CodEmpresa, CrConveniosCreditosBusquedaRequest? request)
        {
            return _db.CR_Convenios_Creditos_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre líneas o destinos de crédito.
        /// </summary>
        public ErrorDto<CrConveniosF4Data> CR_Convenios_Creditos_Scroll_Obtener(int CodEmpresa, CrConveniosCreditosScrollRequest? request)
        {
            return _db.CR_Convenios_Creditos_Scroll_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia o desasocia una línea o destino de crédito.
        /// </summary>
        public ErrorDto CR_Convenios_Creditos_Actualizar(int CodEmpresa, string? usuario, CrConveniosCreditosActualizarRequest? request)
        {
            return _db.CR_Convenios_Creditos_Actualizar(CodEmpresa, usuario, request);
        }

        /// <summary>
        /// Obtiene los cargos adicionales del convenio.
        /// </summary>
        public ErrorDto<List<CrConveniosCargoData>> CR_Convenios_Cargos_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            return _db.CR_Convenios_Cargos_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene los cargos adicionales disponibles.
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Cargos_F4_Obtener(int CodEmpresa, CrConveniosCodigoBusquedaRequest? request)
        {
            return _db.CR_Convenios_Cargos_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre los cargos adicionales disponibles.
        /// </summary>
        public ErrorDto<CrConveniosF4Data> CR_Convenios_Cargos_Scroll_Obtener(int CodEmpresa, CrConveniosCargosScrollRequest? request)
        {
            return _db.CR_Convenios_Cargos_Scroll_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia o desasocia un cargo adicional.
        /// </summary>
        public ErrorDto CR_Convenios_Cargos_Actualizar(int CodEmpresa, string? usuario, CrConveniosCargosActualizarRequest? request)
        {
            return _db.CR_Convenios_Cargos_Actualizar(CodEmpresa, usuario, request);
        }

        #endregion

        #region Rebajas de CxP y Ahorros

        /// <summary>
        /// Obtiene los cargos de CxP aplicables al convenio.
        /// </summary>
        public ErrorDto<List<CrConveniosRebajasCxpData>> CR_Convenios_RebajasCxP_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            return _db.CR_Convenios_RebajasCxP_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia o desasocia un cargo de CxP.
        /// </summary>
        public ErrorDto CR_Convenios_RebajasCxP_Actualizar(int CodEmpresa, string? usuario, CrConveniosRebajasCxpActualizarRequest? request)
        {
            return _db.CR_Convenios_RebajasCxP_Actualizar(CodEmpresa, usuario, request);
        }

        /// <summary>
        /// Obtiene los planes de ahorro asociados al convenio.
        /// </summary>
        public ErrorDto<List<CrConveniosAhorrosData>> CR_Convenios_Ahorros_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            return _db.CR_Convenios_Ahorros_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene los planes de ahorro disponibles.
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Ahorros_Planes_F4_Obtener(int CodEmpresa, CrConveniosAhorrosPlanesBusquedaRequest? request)
        {
            return _db.CR_Convenios_Ahorros_Planes_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Navega entre los planes de ahorro disponibles.
        /// </summary>
        public ErrorDto<CrConveniosAhorroPlanData> CR_Convenios_Ahorros_Planes_Scroll_Obtener(int CodEmpresa, CrConveniosAhorrosScrollRequest? request)
        {
            return _db.CR_Convenios_Ahorros_Planes_Scroll_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene contratos de ahorro disponibles para el convenio.
        /// </summary>
        public ErrorDto<TablasListaGenericaModel> CR_Convenios_Ahorros_Contratos_F4_Obtener(int CodEmpresa, CrConveniosAhorrosContratosBusquedaRequest? request)
        {
            return _db.CR_Convenios_Ahorros_Contratos_F4_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Asocia, modifica o desasocia un plan de ahorro.
        /// </summary>
        public ErrorDto CR_Convenios_Ahorros_Actualizar(int CodEmpresa, string? usuario, CrConveniosAhorrosActualizarRequest? request)
        {
            return _db.CR_Convenios_Ahorros_Actualizar(CodEmpresa, usuario, request);
        }

        #endregion

        #region Órdenes y Reservas

        /// <summary>
        /// Obtiene las órdenes de pago procesadas del convenio.
        /// </summary>
        public ErrorDto<List<CrConveniosOrdenData>> CR_Convenios_Ordenes_Lista_Obtener(int CodEmpresa, CrConveniosOrdenesListaRequest? request)
        {
            return _db.CR_Convenios_Ordenes_Lista_Obtener(CodEmpresa, request);
        }

        /// <summary>
        /// Obtiene el historial de reservas del convenio.
        /// </summary>
        public ErrorDto<List<CrConveniosReservaData>> CR_Convenios_Reservas_Lista_Obtener(int CodEmpresa, CrConveniosConvenioRequest? request)
        {
            return _db.CR_Convenios_Reservas_Lista_Obtener(CodEmpresa, request);
        }

        #endregion
    }
}