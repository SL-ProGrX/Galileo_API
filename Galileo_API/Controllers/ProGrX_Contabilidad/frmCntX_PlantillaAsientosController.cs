using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCntXPlantillaAsientosController : ControllerBase
    {
        private readonly FrmCntXPlantillaAsientosBl _bl;

        public FrmCntXPlantillaAsientosController(IConfiguration config)
        {
            _bl = new FrmCntXPlantillaAsientosBl(config);
        }

        [Authorize]
        [HttpGet("Consultar")]
        public ErrorDto<CntxPlantillaResponseDto> Consultar(int codEmpresa, int codPlantilla)
        {
            return _bl.Consultar(codEmpresa, codPlantilla);
        }

        [Authorize]
        [HttpPost("Insertar")]
        public ErrorDto<int> Insertar(int codEmpresa, CntxPlantillaSaveDto modelo)
        {
            return _bl.Insertar(codEmpresa, modelo);
        }

        [Authorize]
        [HttpPut("Actualizar")]
        public ErrorDto<int?> Actualizar(int codEmpresa, CntxPlantillaSaveDto modelo)
        {
            return _bl.Actualizar(codEmpresa, modelo);
        }

        [Authorize]
        [HttpDelete("Borrar")]
        public ErrorDto<int> Borrar(int codEmpresa, int codPlantilla)
        {
            return _bl.Borrar(codEmpresa, codPlantilla);
        }

        [Authorize]
        [HttpGet("Scroll")]
        public ErrorDto<int?> Scroll(int codEmpresa, int? codigoActual, int direccion)
        {
            return _bl.Scroll(codEmpresa, codigoActual, direccion);
        }

        [Authorize]
        [HttpGet("BuscarPlantillas")]
        public ErrorDto<List<CntxPlantillaDto>> BuscarPlantillas(int codEmpresa)
        {
            return _bl.BuscarPlantillas(codEmpresa);
        }

        [HttpGet]
        [Route("Cntx_TiposAsientos_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsientosBuscar(int codEmpresa, int cod_contabilidad)
        {
            return _bl.Cntx_TiposAsientos_Buscar(codEmpresa, cod_contabilidad);
        }
    }
}