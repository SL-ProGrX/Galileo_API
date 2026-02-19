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
    public class FrmCntXPlantillaRateController : ControllerBase
    {
        private readonly FrmCntXPlantillaRateBl _bl;

        public FrmCntXPlantillaRateController(IConfiguration config)
        {
            _bl = new FrmCntXPlantillaRateBl(config);
        }

        [HttpGet("CntxPlantillaRate_Scroll_Obtener")]
        public ErrorDto<CntxPlantillaRateDto> CntxPlantillaRate_Scroll_Obtener(int codEmpresa, int scrollCode, int? codPlantilla)
        {
            return _bl.CntxPlantillaRate_Scroll_Obtener(codEmpresa, scrollCode, codPlantilla);
        }

        [HttpGet("CntxPlantillaRate_Consulta_Obtener")]
        public ErrorDto<CntxPlantillaRateDto> CntxPlantillaRate_Consulta_Obtener(int codEmpresa, int codPlantilla)
        {
            return _bl.CntxPlantillaRate_Consulta_Obtener(codEmpresa, codPlantilla);
        }

        [HttpPost("CntxPlantillaRate_Guardar")]
        public ErrorDto CntxPlantillaRate_Guardar(int codEmpresa, bool existe, CntxPlantillaRateDto request)
        {
            return _bl.CntxPlantillaRate_Guardar(codEmpresa, existe, request);
        }

        [HttpDelete("CntxPlantillaRate_Eliminar")]
        public ErrorDto CntxPlantillaRate_Eliminar(int codEmpresa, string usuario, int codPlantilla)
        {
            return _bl.CntxPlantillaRate_Eliminar(codEmpresa, usuario, codPlantilla);
        }

        [HttpGet("TiposAsiento_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> TiposAsiento_Obtener(int codEmpresa)
        {
            return _bl.TiposAsiento_Obtener(codEmpresa);
        }

        [HttpGet("Plantillas_Buscar")]
        public ErrorDto<List<DropDownListaGenericaModel>>Plantillas_Buscar(int codEmpresa)
        {
            return _bl.Plantillas_Buscar(codEmpresa);
        }

        [HttpGet("Unidades_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Unidades_Obtener(int codEmpresa)
        {
            return _bl.Unidades_Obtener(codEmpresa);
        }

        [HttpGet("Divisas_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Divisas_Obtener(int codEmpresa)
        {
            return _bl.Divisas_Obtener(codEmpresa);
        }

        [HttpGet("CentroCosto_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CentroCosto_Obtener(int codEmpresa,string codUnidad)
        {
            return _bl.CentroCosto_Obtener(codEmpresa, codUnidad);
        }



    }
}
