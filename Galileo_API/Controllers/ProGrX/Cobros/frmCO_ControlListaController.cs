using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        #endregion
    }
}
