using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
   
    public class FrmCntXConAsientosController : ControllerBase
    {
        private readonly FrmCntXConAsientosBl _bl;

        public FrmCntXConAsientosController(IConfiguration config)
        {
            _bl = new FrmCntXConAsientosBl(config);
        }

        /// <summary>
        /// Lista consolidaciones
        /// </summary>
        [Authorize]
        [HttpGet("Consolidaciones_Listar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Consolidaciones_Listar(
            int codEmpresa,
            int codContabilidad)
        {
            return _bl.Consolidaciones_Listar(codEmpresa, codContabilidad);
        }

        /// <summary>
        /// Buscar asientos por consolidación
        /// </summary>
        [Authorize]
        [HttpGet("Asientos_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>> Asientos_Buscar(int codEmpresa, int codContabilidad, int? codConsolida)
        {
            return _bl.Asientos_Buscar(codEmpresa, codContabilidad, codConsolida);
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

        [Authorize]
        [HttpGet("AsientoDetalle_Obtener")]
        public ErrorDto<List<CntxConAsientoDetalleDto>> AsientoDetalle_Obtener(int codEmpresa,int codContabilidad,int? codConsolida,string? codAsiento)
        {
            return _bl.AsientoDetalle_Obtener(codEmpresa,codContabilidad,codConsolida,codAsiento
            );
        }

        [Authorize]
        [HttpPost("GuardarAsiento")]
        public ErrorDto<bool> GuardarAsiento([FromBody] CntxConAsientoGuardarDto request)
        {
            return _bl.GuardarAsiento(request);
        }

        [Authorize]
        [HttpDelete("EliminarAsiento")]
        public ErrorDto<bool> EliminarAsiento(int codEmpresa,int codContabilidad,int codConsolida,string codAsiento,string usuario)
        {
            return _bl.EliminarAsiento(codEmpresa, codContabilidad, codConsolida, codAsiento, usuario);
        }
    }
}