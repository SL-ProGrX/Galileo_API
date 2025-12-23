using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.BusinessLogic.ProGrX_Activos_Fijos;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.Controllers.ProGrX_Activos_Fijos
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmActivosCambioVidaUtilController : ControllerBase
    {
        private readonly FrmActivosCambioVidaUtilBL _bl;

        public FrmActivosCambioVidaUtilController(IConfiguration config)
        {
            _bl = new FrmActivosCambioVidaUtilBL(config);
        }
        
        [HttpGet("Activos_CambioVU_ActivoLista_Obtener")]
        [Authorize]
        public ErrorDto<ActivoLiteLista> Activos_CambioVU_ActivoLista_Obtener(int CodEmpresa, string filtros)
        {
            return _bl.Activos_CambioVU_ActivoLista_Obtener(CodEmpresa, filtros);
        }

        [HttpGet("Activos_CambioVU_Activo_Obtener")]
        public ErrorDto<ActivoBuscarResponse> Activos_CambioVU_Activo_Obtener(int CodEmpresa, string numPlaca)
        {
            return _bl.Activos_CambioVU_Activo_Obtener(CodEmpresa, numPlaca);
        }

        [HttpGet("Activos_CambioVU_MetodosDepreciacion_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_CambioVU_MetodosDepreciacion_Obtener(int CodEmpresa)
        {
            return _bl.Activos_CambioVU_MetodosDepreciacion_Obtener(CodEmpresa);
        }

        [HttpPost("Activos_CambioVU_Aplicar")]
        public ErrorDto<CambioVidaUtilAplicarResponse> Activos_CambioVU_Aplicar(
            int CodEmpresa,
            string usuario,
            [FromBody] CambioVidaUtilAplicarRequest dto)
        {
            return _bl.Activos_CambioVU_Aplicar(CodEmpresa, usuario, dto);
        }
    }
}
