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
    public class FrmCntXContabilidadesController : ControllerBase
    {
        private readonly FrmCntXContabilidadesBl _bl;

        public FrmCntXContabilidadesController(IConfiguration config) 
            => _bl = new FrmCntXContabilidadesBl(config);

        [HttpGet("CntXContabilidad_Obtener")]
        public ErrorDto<CntXContabilidadData?> CntXContabilidad_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXContabilidad_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXContabilidades_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXContabilidades_Lista_Obtener(int codEmpresa)
        {
            return _bl.CntXContabilidades_Lista_Obtener(codEmpresa);
        }

        [HttpGet("CntXContabilidad_Scroll_Obtener")]
        public ErrorDto<CntXContabilidadData?> CntXContabilidad_Scroll_Obtener(int codEmpresa, int scrollCode, int codConta)
        {
            return _bl.CntXContabilidad_Scroll_Obtener(codEmpresa, scrollCode, codConta);
        }
        
        [HttpGet("CntXContabilidad_ConsolidaBaseList_Obtener")]
        public ErrorDto<List<DropDownConsolidaListaData>> CntXContabilidad_ConsolidaBaseList_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXContabilidad_ConsolidaBaseList_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXContabilidad_ConsolidaUnidadesList_Obtener")]
        public ErrorDto<List<DropDownConsolidaListaData>> CntXContabilidad_ConsolidaUnidadesList_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXContabilidad_ConsolidaUnidadesList_Obtener(codEmpresa, codConta);
        }

        [HttpPost("CntXContabilidades_Guardar")]
        public ErrorDto CntXContabilidades_Guardar(int codEmpresa, string usuario, bool edita, CntXContabilidadData request)
        {
            return _bl.CntXContabilidades_Guardar(codEmpresa, usuario, edita, request);
        }

        [HttpDelete("CntXContabilidades_Eliminar")]
        public ErrorDto CntXContabilidades_Eliminar(int codEmpresa, int codConta, string usuario)
        {
            return _bl.CntXContabilidades_Eliminar(codEmpresa, codConta, usuario);
        }
    }
}