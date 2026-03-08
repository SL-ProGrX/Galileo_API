using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCntXUtilEliminaAsientosController : ControllerBase
    {
        private readonly FrmCntXUtilEliminaAsientosBl _bl;

        public FrmCntXUtilEliminaAsientosController(IConfiguration config)
        {
            _bl = new FrmCntXUtilEliminaAsientosBl(config);
        }

        [HttpGet]
        [Route("Cntx_TiposAsientos_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsientosBuscar(int codEmpresa, int cod_contabilidad)
        {
            return _bl.Cntx_TiposAsientos_Buscar(codEmpresa, cod_contabilidad);
        }

        [HttpGet]
        [Route("Cntx_Util_Asientos_Calcular")]
        public ErrorDto<int> CalcularTotal(int codEmpresa, int cod_contabilidad, string tipo_asiento, DateTime desde, DateTime hasta, int anio, int mes)
        {
            return _bl.Cntx_Util_Asientos_Calcular(codEmpresa, cod_contabilidad, tipo_asiento, desde,
                hasta, anio, mes);
        }

        [HttpPost]
        [Route("Cntx_Util_Asientos_Eliminar")]
        public ErrorDto<bool> Eliminar([FromBody] CntxEliminarAsientosRequestDto request)
        {
            return _bl.Cntx_Util_Asientos_Eliminar(request);
        }

        [HttpGet]
        [Route("Cntx_PeriodoActual_Obtener")]
        public ErrorDto<CntxPeriodoActualDto> PeriodoActual(int codEmpresa, int cod_contabilidad)
        {
            return _bl.Cntx_PeriodoActual_Obtener(codEmpresa, cod_contabilidad);
        }
    }
}