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
    public class FrmCntXDiferidosPlantillaController : ControllerBase
    {
        private readonly FrmCntXDiferidosPlantillaBl _bl;

        public FrmCntXDiferidosPlantillaController(IConfiguration config) => _bl = new FrmCntXDiferidosPlantillaBl(config);

        [HttpGet("CntXDiferidosPlantilla_Obtener")]
        public ErrorDto<CntXDiferidosData?> CntXDiferidosPlantilla_Obtener(int codEmpresa, int codConta, int codDiferido)
        {
            return _bl.CntXDiferidosPlantilla_Obtener(codEmpresa, codConta, codDiferido);
        }

        [HttpGet("CntXDiferidosPlantilla_Scroll_Obtener")]
        public ErrorDto<CntXDiferidosData?> CntXDiferidosPlantilla_Scroll_Obtener(int CodEmpresa, int codConta, int scrollCode, int codDiferido)
        {
            return _bl.CntXDiferidosPlantilla_Scroll_Obtener(CodEmpresa, codConta, scrollCode, codDiferido);
        }

        [HttpGet("CntXDiferidosPlantilla_Lista_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXDiferidosPlantilla_Lista_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXDiferidosPlantilla_Lista_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXDiferidosPlantilla_TiposAsientos_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CntXDiferidosPlantilla_TiposAsientos_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXDiferidosPlantilla_TiposAsientos_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXDiferidosPlantilla_TipoAsientoDesc_Obtener")]
        public ErrorDto<string?> CntXDiferidosPlantilla_TipoAsientoDesc_Obtener(int codEmpresa, int codConta, string tipoAsiento)
        {
            return _bl.CntXDiferidosPlantilla_TipoAsientoDesc_Obtener(codEmpresa, codConta, tipoAsiento);
        }

        [HttpGet("CntXDiferidosPlantilla_Detalle_Obtener")]
        public ErrorDto<List<CntXDiferidosDetalleData>> CntXDiferidosPlantilla_Detalle_Obtener(int codEmpresa, int codConta, int codDiferido)
        {
            return _bl.CntXDiferidosPlantilla_Detalle_Obtener(codEmpresa, codConta, codDiferido);
        }

        [HttpPost("CntXDiferidosPlantilla_Guardar")]
        public ErrorDto CntXDiferidosPlantilla_Guardar(int codEmpresa, CntXDiferidosPlantillaRequest request)
        {
            return _bl.CntXDiferidosPlantilla_Guardar(codEmpresa, request);
        }

        [HttpDelete("CntXDiferidosPlantilla_Eliminar")]
        public ErrorDto CntXDiferidosPlantilla_Eliminar(int codEmpresa, int codConta, string usuario, int codDiferido)
        {
            return _bl.CntXDiferidosPlantilla_Eliminar(codEmpresa, codConta, usuario, codDiferido);
        }
    }
}