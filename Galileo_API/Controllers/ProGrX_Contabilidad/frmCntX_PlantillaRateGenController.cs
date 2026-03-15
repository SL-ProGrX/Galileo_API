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
    [Authorize]
    public class FrmCntXPlantillaRateGenController : ControllerBase
    {
        private readonly FrmCntXPlantillaRateGenBl _bl;

        public FrmCntXPlantillaRateGenController(IConfiguration config) => 
            _bl = new FrmCntXPlantillaRateGenBl(config);

        [HttpGet("CntXPlantillaRate_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXPlantillaRate_Lista_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXPlantillaRate_Lista_Obtener(codEmpresa, codConta);
        }
        
        [HttpGet("CntXPlantillaRate_Detalle_Obtener")]
        public ErrorDto<List<CntXPlantillaRateDetalleData>> CntXPlantillaRate_Detalle_Obtener(int codEmpresa, int codConta, int codPlantilla)
        {
            return _bl.CntXPlantillaRate_Detalle_Obtener(codEmpresa, codConta, codPlantilla);
        }

        [HttpPost("CntXPlantillaRate_Generar")]
        public ErrorDto CntXPlantillaRate_Generar(int codEmpresa, CntXPlantillaRateGenerarRequest request)
        {
            return _bl.CntXPlantillaRate_Generar(codEmpresa, request);
        }
    }
}