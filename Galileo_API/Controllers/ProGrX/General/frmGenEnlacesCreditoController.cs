using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;

namespace Galileo.Controllers
{
    [Route("api/frmGenEnlacesCredito")]
    [ApiController]
    public class FrmGenEnlacesCreditoController : ControllerBase
    {

        readonly FrmGenEnlacesCreditoBl _bl;
        public FrmGenEnlacesCreditoController(IConfiguration config)
        {
            _bl = new FrmGenEnlacesCreditoBl(config);
        }

        [HttpGet("EnlacesCreditoConsultar")]
        //[Authorize]
        public ErrorDto<EnlaceCreditoLista> EnlacesCreditoConsultar(int codEmpresa, int? pagina, int? paginacion, string? filtro)
        {
            return _bl.EnlacesCreditoConsultar(codEmpresa, pagina, paginacion, filtro);
        }

        [HttpGet("CodigoCredito_ObtenerTodos")]
        //[Authorize]
        public ErrorDto<List<CodigoCreditoDto>> CodigoCredito_ObtenerTodos(int codEmpresa, string cod_institucion)
        {
            return _bl.CodigoCredito_ObtenerTodos(codEmpresa, cod_institucion);
        }


        [HttpPost("EnlaceCredito_Actualizar")]
        // [Authorize]
        public ErrorDto EnlaceCredito_Actualizar(EnlaceCreditoDto request)
        {
            return _bl.EnlaceCredito_Actualizar(request);
        }
    }
}
