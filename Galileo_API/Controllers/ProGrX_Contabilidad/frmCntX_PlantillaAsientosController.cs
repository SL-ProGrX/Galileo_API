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
        public ErrorDto<CntxPlantillaResponseDto> Consultar(int codEmpresa, int codContabilidad, int codPlantilla)
        {
            return _bl.Consultar(codEmpresa, codContabilidad, codPlantilla);
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
        public ErrorDto<int> Borrar(int codEmpresa, int codContabilidad, int codPlantilla)
        {
            return _bl.Borrar(codEmpresa, codContabilidad, codPlantilla);
        }

        [Authorize]
        [HttpGet("Scroll")]
        public ErrorDto<int?> Scroll(int codEmpresa, int codContabilidad, int? codigoActual, int direccion)
        {
            return _bl.Scroll(codEmpresa, codContabilidad, codigoActual, direccion);
        }

        [Authorize]
        [HttpGet("BuscarPlantillas")]
        public ErrorDto<List<CntxPlantillaDto>> BuscarPlantillas(int codEmpresa, int codContabilidad)
        {
            return _bl.BuscarPlantillas(codEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpGet]
        [Route("Cntx_TiposAsientos_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsientosBuscar(int codEmpresa, int cod_contabilidad)
        {
            return _bl.Cntx_TiposAsientos_Buscar(codEmpresa, cod_contabilidad);
        }

        [Authorize]
        [HttpGet]
        [Route("Unidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return _bl.Unidades_Obtener(codEmpresa, cod_contabilidad);
        }

        [Authorize]
        [HttpGet]
        [Route("Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Obtener(int codEmpresa, int cod_contabilidad)
        {
            return _bl.Divisas_Obtener(codEmpresa, cod_contabilidad);
        }

        [Authorize]
        [HttpGet]
        [Route("CentroCosto_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CentroCosto_Obtener(int codEmpresa, int cod_contabilidad, string codUnidad)
        {
            return _bl.CentroCosto_Obtener(codEmpresa, cod_contabilidad, codUnidad);
        }
    }
}
