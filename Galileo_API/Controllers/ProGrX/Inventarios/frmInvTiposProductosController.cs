using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.AspNetCore.Authorization;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class FrmInvTiposProductosController : ControllerBase
    {
        private readonly FrmInvTiposProductosBL _bl;

        public FrmInvTiposProductosController(IConfiguration config)
        {
            _bl = new FrmInvTiposProductosBL(config);

        }

        [HttpGet("TipoProducto_Obtener")]
        public ErrorDto<TipoProductoDataLista> TipoProducto_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro, int cod_contabilidad)
        {
            return _bl.TipoProducto_Obtener(CodCliente, pagina, paginacion, filtro, cod_contabilidad);
        }

        [HttpGet("TipoProducto_ObtenerTodos")]
        public List<TipoProductoDto> TipoProducto_ObtenerTodos(int CodEmpresa, int cod_contabilidad)
        {
            return _bl.TipoProducto_ObtenerTodos(CodEmpresa, cod_contabilidad);
        }

        [HttpPost("TipoProducto_Insertar")]
        public ErrorDto TipoProducto_Insertar(int CodEmpresa, TipoProductoDto request)
        {
            return _bl.TipoProducto_Insertar(CodEmpresa, request);
        }

        [HttpPost("TipoProducto_Actualizar")]
        public ErrorDto TipoProducto_Actualizar(int CodEmpresa, TipoProductoDto request)
        {
            return _bl.TipoProducto_Actualizar(CodEmpresa, request);
        }

        [HttpPost("TipoProducto_Eliminar")]
        public ErrorDto TipoProducto_Eliminar(int CodEmpresa, string producto)
        {
            return _bl.TipoProducto_Eliminar(CodEmpresa, producto);
        }

        [HttpGet("TipoProductoSub_Obtener")]
        public ErrorDto<TipoProductoSubDataLista> TipoProductoSub_Obtener(int CodCliente, int ProdClas, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.TipoProductoSub_Obtener(CodCliente, ProdClas, pagina, paginacion, filtro);
        }

        [HttpGet("TipoProductoSub_ObtenerTodos")]
        public ErrorDto<List<TipoProductoSubGradaData>> TipoProductoSub_ObtenerTodos(int CodEmpresa, string Cod_Prodclas)
        {
            return _bl.TipoProductoSub_ObtenerTodos(CodEmpresa, Cod_Prodclas);
        }

        [HttpPost("TipoProductoSub_Insertar")]
        public ErrorDto TipoProductoSub_Insertar(int CodEmpresa, TipoProductoSubDto request)
        {
            return _bl.TipoProductoSub_Insertar(CodEmpresa, request);
        }

        [HttpPost("TipoProductoSub_Actualizar")]
        public ErrorDto TipoProductoSub_Actualizar(int CodEmpresa, TipoProductoSubDto request)
        {
            return _bl.TipoProductoSub_Actualizar(CodEmpresa, request);
        }

        [HttpGet("Cabys_ObtenerTodos")]
        public ErrorDto<List<InvCabys>> Cabys_Obtener(int CodEmpresa)
        {
            return _bl.Cabys_ObtenerTodos(CodEmpresa);
        }

        [HttpGet("Cabys_Obtener")]
        public ErrorDto<List<InvCabys>> Cabys_Obtener(int CodEmpresa, string filtro)
        {
            return _bl.Cabys_Obtener(CodEmpresa, filtro);
        }
    }
}