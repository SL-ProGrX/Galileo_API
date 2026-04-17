using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCOControlListaController : ControllerBase
    {
        private readonly FrmCOControlListaBL _bl;

        public FrmCOControlListaController(IConfiguration config)
        {
            _bl = new FrmCOControlListaBL(config);
        }

        #region Principal

        [HttpGet("Co_ControlLista_Buscar")]
        public ErrorDto<CoControlListaBuscarResponse> CoControlLista_Buscar(
            int CodEmpresa,
            string filtros)
        {
            return _bl.CoControlLista_Buscar(CodEmpresa, filtros);
        }

        [HttpGet("Co_ControlLista_UsuarioScroll_Obtener")]
        public ErrorDto<CoControlListaUsuarioScrollResponse> CoControlLista_UsuarioScroll_Obtener(
            int CodEmpresa,
            string filtros)
        {
            return _bl.CoControlLista_UsuarioScroll_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Co_ControlLista_Usuarios_Obtener")]
        public ErrorDto<List<CoControlListaUsuarioBusquedaRow>> CoControlLista_Usuarios_Obtener(
            int CodEmpresa,
            string filtros)
        {
            return _bl.CoControlLista_Usuarios_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Co_ControlLista_Garantias_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Garantias_Obtener(int CodEmpresa)
        {
            return _bl.CoControlLista_Garantias_Obtener(CodEmpresa);
        }

        [HttpGet("Co_ControlLista_Antiguedades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Antiguedades_Obtener(int CodEmpresa)
        {
            return _bl.CoControlLista_Antiguedades_Obtener(CodEmpresa);
        }

        [HttpGet("Co_ControlLista_Carteras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Carteras_Obtener(int CodEmpresa)
        {
            return _bl.CoControlLista_Carteras_Obtener(CodEmpresa);
        }

        [HttpGet("Co_ControlLista_Oficinas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Oficinas_Obtener(int CodEmpresa)
        {
            return _bl.CoControlLista_Oficinas_Obtener(CodEmpresa);
        }

        [HttpGet("Co_ControlLista_Instituciones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Instituciones_Obtener(int CodEmpresa)
        {
            return _bl.CoControlLista_Instituciones_Obtener(CodEmpresa);
        }

        [HttpGet("Co_ControlLista_Gestiones_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Gestiones_Obtener(int CodEmpresa)
        {
            return _bl.CoControlLista_Gestiones_Obtener(CodEmpresa);
        }

        [HttpGet("Co_ControlLista_Causas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Causas_Obtener(int CodEmpresa)
        {
            return _bl.CoControlLista_Causas_Obtener(CodEmpresa);
        }

        [HttpGet("Co_ControlLista_Arreglos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CoControlLista_Arreglos_Obtener(int CodEmpresa)
        {
            return _bl.CoControlLista_Arreglos_Obtener(CodEmpresa);
        }

        [HttpGet("Co_ControlLista_Personas_Obtener")]
        public ErrorDto<List<CoControlListaPersonaBusquedaRow>> CoControlLista_Personas_Obtener(
            int CodEmpresa,
            string filtros)
        {
            return _bl.CoControlLista_Personas_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("Co_ControlLista_NotificarMarcados_Procesar")]
        public ErrorDto<int> CoControlLista_NotificarMarcados_Procesar(
                int CodEmpresa,
                CoControlListaNotificarMarcadosRequest request)
        {
            return _bl.CoControlLista_NotificarMarcados_Procesar(CodEmpresa, request);
        }

        #endregion

        #region Operaciones

        [HttpGet("Co_ControlLista_Operaciones_Obtener")]
        public ErrorDto<CoControlListaOperacionesResponse> CoControlLista_Operaciones_Obtener(
            int CodEmpresa,
            string filtros)
        {
            return _bl.CoControlLista_Operaciones_Obtener(CodEmpresa, filtros);
        }

        #endregion

        #region Datos Persona

        [HttpGet("Co_ControlLista_DatosPersonales_Obtener")]
        public ErrorDto<CoControlListaDatosPersonalesResponse> CoControlLista_DatosPersonales_Obtener(
            int CodEmpresa,
            string filtros)
        {
            return _bl.CoControlLista_DatosPersonales_Obtener(CodEmpresa, filtros);
        }

        #endregion

        #region Gestiones
        [HttpGet("Co_ControlLista_Gestiones_Consulta")]
        public ErrorDto<CoControlListaGestionesResponse> Co_ControlLista_Gestiones_Consulta(
            int CodEmpresa,
            string filtros)
        {
            return _bl.Co_ControlLista_Gestiones_Consulta(CodEmpresa, filtros);
        }

        [HttpPost("Co_ControlLista_Notificacion_Procesar")]
        public ErrorDto<bool> CoControlLista_Notificacion_Procesar(
            int CodEmpresa,
            CoControlListaNotificacionRequest request)
        {
            return _bl.CoControlLista_Notificacion_Procesar(CodEmpresa, request);
        }
        #endregion

        #region Fiadores

        [HttpGet("Co_ControlLista_Fiadores_Obtener")]
        public ErrorDto<List<CoControlListaFiadorRow>> CoControlLista_Fiadores_Obtener(
            int CodEmpresa,
            string filtros)
        {
            return _bl.CoControlLista_Fiadores_Obtener(CodEmpresa, filtros);
        }

        #endregion

        #region Traslados

        [HttpGet("Co_ControlLista_UsuariosTraslado_Obtener")]
        public ErrorDto<List<CoControlListaUsuarioBusquedaRow>> CoControlLista_UsuariosTraslado_Obtener(
           int codEmpresa,
           string request)
        {
            return _bl.CoControlLista_UsuariosTraslado_Obtener(codEmpresa, request);
        }

        [HttpPost("Co_ControlLista_AplicarMarcados_Procesar")]
        public ErrorDto<bool> CoControlLista_AplicarMarcados_Procesar(
    int CodEmpresa,
    CoControlListaAplicarMarcadosRequest request)
        {
            return _bl.CoControlLista_AplicarMarcados_Procesar(CodEmpresa, request);
        }

        [HttpPost("Co_ControlLista_TrasladarMarcados_Procesar")]
        public ErrorDto<bool> CoControlLista_TrasladarMarcados_Procesar(
            int CodEmpresa,
            CoControlListaTrasladarMarcadosRequest request)
        {
            return _bl.CoControlLista_TrasladarMarcados_Procesar(CodEmpresa, request);
        }

        #endregion

        #region Gestiones Modal

        [HttpGet("Co_ControlLista_GestionActual_Obtener")]
        public ErrorDto<CoControlListaGestionActualResponse> CoControlLista_GestionActual_Obtener(
                int CodEmpresa,
                string filtros)
        {
            return _bl.CoControlLista_GestionActual_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Co_ControlLista_GestionDetalle_Obtener")]
        public ErrorDto<CoControlListaGestionDetalleResponse> CoControlLista_GestionDetalle_Obtener(
                int CodEmpresa,
                string filtros)
        {
            return _bl.CoControlLista_GestionDetalle_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("Co_ControlLista_Gestion_Procesar")]
        public ErrorDto<bool> CoControlLista_Gestion_Procesar(
            int CodEmpresa,
            CoControlListaGestionProcesarRequest request)
        {
            return _bl.CoControlLista_Gestion_Procesar(CodEmpresa, request);
        }

        #endregion

        #region Cartera
        [HttpGet("Co_ControlLista_ResumenCarteraUsuario_Obtener")]
        public ErrorDto<CoControlListaResumenCarteraUsuarioResponse> CoControlLista_ResumenCarteraUsuario_Obtener(
                int CodEmpresa,
                string filtros)
        {
            return _bl.CoControlLista_ResumenCarteraUsuario_Obtener(CodEmpresa, filtros);
        }

        [HttpPost("Co_ControlLista_AnalisisCartera_Procesar")]
        public ErrorDto<CoControlListaAnalisisCarteraProcesarResponse> CoControlLista_AnalisisCartera_Procesar(
            int CodEmpresa)
        {
            return _bl.CoControlLista_AnalisisCartera_Procesar(CodEmpresa);
        }
        #endregion
    }
}
