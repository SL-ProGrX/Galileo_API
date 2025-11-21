using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MComprasController : ControllerBase
    {
        private readonly IConfiguration _config;

        public MComprasController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("sbCprCboCargosPer")]
        // [Authorize]
        public List<CargoPeriodicoDto> sbCprCboCargosPer(int CodEmpresa)
        {
            return new MComprasBl(_config).sbCprCboCargosPer(CodEmpresa);
        }

        [HttpGet("fxCprCambiaFecha")]
        // [Authorize]
        public bool fxCprCambiaFecha(int CodEmpresa, string vUsuario)
        {
            return new MComprasBl(_config).fxCprCambiaFecha(CodEmpresa, vUsuario);
        }

        [HttpGet("sbCprOrdenesDespacho")]
        // [Authorize]
        public ErrorDto sbCprOrdenesDespacho(int CodEmpresa, string vOrden)
        {
            return new MComprasBl(_config).sbCprOrdenesDespacho(CodEmpresa, vOrden);
        }

        [HttpGet("sbCprCboTiposOrden")]
        // [Authorize]
        public List<TipoOrdenDto> sbCprCboTiposOrden(int CodEmpresa)
        {
            return new MComprasBl(_config).sbCprCboTiposOrden(CodEmpresa);
        }

        [HttpGet("UnidadesObtener")]
        public ErrorDto<UnidadesDtoList> UnidadesObtener(int CodEmpresa, string? filtros)
        {
            return new MComprasBl(_config).UnidadesObtener(CodEmpresa, filtros);
        }

        [HttpGet("CentroCostosObtener")]
        public ErrorDto<CentroCostoDtoList> CentroCostosObtener(int CodEmpresa, string? filtros)
        {
            return new MComprasBl(_config).CentroCostosObtener(CodEmpresa, filtros);
        }


        [HttpGet("CatalogoCompras_Obtener")]
        public ErrorDto<List<CatalogoDto>> CatalogoCompras_Obtener(int CodEmpresa, string tipo)
        {
            return new MComprasBl(_config).CatalogoCompras_Obtener(CodEmpresa, tipo);
        }
    }
}
