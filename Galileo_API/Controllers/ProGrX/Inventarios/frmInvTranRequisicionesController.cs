using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvTranRequisicionesController : ControllerBase
    {
        private readonly FrmInvTranRequisicionesBL _bl;
        public FrmInvTranRequisicionesController(IConfiguration config)
        {
            _bl = new FrmInvTranRequisicionesBL(config);
        }

        [HttpGet("InvTranRequisicion_Obtener")]
        public ErrorDto<TranRequisicionData> InvTranRequisicion_Obtener(int CodEmpresa, int CodRequisicion)
        {
            return _bl.InvTranRequisicion_Obtener(CodEmpresa, CodRequisicion);
        }

        [HttpGet("InvRequesicionProduc_Obtener")]
        public ErrorDto<List<InvReqProduc>> InvRequesicionProduc_Obtener(int CodEmpresa, int CodRequisicion)
        {
            return _bl.InvRequesicionProduc_Obtener(CodEmpresa, CodRequisicion);
        }

        [HttpGet("InvTranRequisicion_scroll")]
        public ErrorDto<TranRequisicionData> InvTranRequisicion_scroll(int CodEmpresa, int scrollValue, int? CodRequisicion)
        {
            return _bl.InvTranRequisicion_scroll(CodEmpresa, scrollValue, CodRequisicion);
        }

        [HttpPost("InvTranRequisicion_Insertar")]
        public ErrorDto InvTranRequisicion_Insertar(int CodEmpresa, TranRequisicionData request)
        {
            return _bl.InvTranRequisicion_Insertar(CodEmpresa, request);
        }

        [HttpPost("InvTranRequisicion_Actualizar")]
        public ErrorDto InvTranRequisicion_Actualizar(int CodEmpresa, TranRequisicionData request)
        {
            return _bl.InvTranRequisicion_Actualizar(CodEmpresa, request);
        }

        [HttpPost("InvTranRequesicion_Eliminar")]
        public ErrorDto InvTranRequesicion_Eliminar(int CodEmpresa, int CodRequisicion)
        {
            return _bl.InvTranRequesicion_Eliminar(CodEmpresa, CodRequisicion);
        }

        [HttpPost("InvRequesicionProduc_Insertar")]
        public ErrorDto InvRequesicionProduc_Insertar(int CodEmpresa, int CodRequisicion, List<InvReqProduc> producLineas)
        {
            return _bl.InvRequesicionProduc_Insertar(CodEmpresa, CodRequisicion, producLineas);
        }

        [HttpGet("InvTranPlantilla_Obtener")]
        public ErrorDto<List<TranRequisicionData>> InvTranPlantilla_Obtener(int CodEmpresa, int? CodRequisicion, string? GeneraUser, string? GeneraFecha)
        {
            return _bl.InvTranPlantilla_Obtener(CodEmpresa, CodRequisicion, GeneraUser, GeneraFecha);
        }

        [HttpGet("InvTranRequisiciones_Lista")]
        public ErrorDto<List<TranRequisicionData>> InvTranRequisiciones_Lista(int CodEmpresa, string usuario, string columna, string estado)
        {
            return _bl.InvTranRequisiciones_Lista(CodEmpresa, usuario, columna, estado);
        }

        [HttpPost("InvRequisicionProduc_Eliminar")]
        public ErrorDto InvRequisicionProduc_Eliminar(int CodEmpresa, int CodRequisicion, int Linea)
        {
            return _bl.InvRequisicionProduc_Eliminar(CodEmpresa, CodRequisicion, Linea);
        }

        [HttpGet("UENS_Obtener")]
        public ErrorDto<List<CatalogosLista>> UENS_Obtener(int CodEmpresa)
        {
            return _bl.UENS_Obtener(CodEmpresa);
        }

        [HttpGet("UsuarioRecibeLista_Obtener")]
        public ErrorDto<List<InvRequsUsuarioRecibe>> UsuarioRecibeLista_Obtener(int CodEmpresa, string cod_unidad)
        {
            return _bl.UsuarioRecibeLista_Obtener(CodEmpresa, cod_unidad);
        }

        [HttpGet("UsuariosActivoLista_Obtener")]
        public ErrorDto<List<InvRequsUsuarioRecibe>> UsuariosActivoLista_Obtener(int CodEmpresa)
        {
            return _bl.UsuariosActivoLista_Obtener(CodEmpresa);
        }

        [HttpGet("ProductosRequesicionesActivo_Obtener")]
        public ErrorDto<InvRequesicionesActivosLista> ProductosRequesicionesActivo_Obtener(int CodEmpresa, string invReqFiltros)
        {
            return _bl.ProductosRequesicionesActivo_Obtener(CodEmpresa, invReqFiltros);
        }

        [HttpPost("InvRequisicion_Autorizar")]
        public ErrorDto InvRequisicion_Autorizar(int CodEmpresa, int CodRequisicion, string Usuario, string Estado)
        {
            return _bl.InvRequisicion_Autorizar(CodEmpresa, CodRequisicion, Usuario, Estado);
        }

        [HttpPost("InvRequisicion_Procesar")]
        public ErrorDto InvRequisicion_Procesar(int CodEmpresa, int CodRequisicion, string Usuario, string Estado)
        {
            return _bl.InvRequisicion_Procesar(CodEmpresa, CodRequisicion, Usuario, Estado);
        }

        [HttpPost("ValidaAutorizacion")]
        public ErrorDto ValidaAutorizacion(int CodEmpresa, string usuario, string cod_unidad, string cod_proceso)
        {
            return _bl.ValidaAutorizacion(CodEmpresa, usuario, cod_unidad, cod_proceso);
        }

        [HttpGet("ObtenerUsuario")]
        public ErrorDto<List<string>> ObtenerUsuario(int CodEmpresa)
        {
            return _bl.ObtenerUsuario(CodEmpresa);
        }
    }
}