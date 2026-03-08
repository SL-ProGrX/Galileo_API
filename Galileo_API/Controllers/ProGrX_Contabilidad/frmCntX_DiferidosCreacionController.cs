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
    public class FrmCntXDiferidosCreacionController : ControllerBase
    {
        private readonly FrmCntXDiferidosCreacionBl _bl;

        public FrmCntXDiferidosCreacionController(IConfiguration config) => _bl = new FrmCntXDiferidosCreacionBl(config);

        [HttpGet("CntXDiferidosCreacion_Obtener")]
        public ErrorDto<CntXDiferidoCreacionData?> CntXDiferidosCreacion_Obtener(int codEmpresa, int codConta, int codDifPlantilla, int codDiferido)
        {
            return _bl.CntXDiferidosCreacion_Obtener(codEmpresa, codConta, codDifPlantilla, codDiferido);
        }

        [HttpGet("CntXDiferidosCreacion_Scroll_Obtener")]
        public ErrorDto<CntXDiferidoCreacionData?> CntXDiferidosCreacion_Scroll_Obtener(
            int codEmpresa, int codConta, int scroll, int codDiferido, int codDifPlantillaActual)
        {
            return _bl.CntXDiferidosCreacion_Scroll_Obtener(codEmpresa, codConta, scroll, codDiferido, codDifPlantillaActual);
        }

        [HttpGet("CntXDiferidosCreacion_Historico_Obtener")]
        public ErrorDto<List<CntXDiferidoHistoricoData>> CntXDiferidosCreacion_Historico_Obtener(
            int codEmpresa, int codConta, int codDifPlantilla, int codDiferido)
        {
            return _bl.CntXDiferidosCreacion_Historico_Obtener(codEmpresa, codConta, codDifPlantilla, codDiferido);
        }

        [HttpGet("CntXDiferidosCreacion_PlantillaLista_Obtener")]
        public ErrorDto<List<CntXDiferidosPlantillaData>> CntXDiferidosCreacion_PlantillaLista_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXDiferidosCreacion_PlantillaLista_Obtener(codEmpresa, codConta);
        }

        [HttpGet("CntXDiferidosCreacion_Lista_Obtener")]
        public ErrorDto<List<CntXDiferidoCreacionData>> CntXDiferidosCreacion_Lista_Obtener(int codEmpresa, int codConta)
        {
            return _bl.CntXDiferidosCreacion_Lista_Obtener(codEmpresa, codConta);
        }

        [HttpPost("CntXDiferidosCreacion_Guardar")]
        public ErrorDto CntXDiferidosCreacion_Guardar(int codEmpresa, CntXDiferidosCreacionRequest request)
        {
            return _bl.CntXDiferidosCreacion_Guardar(codEmpresa, request);
        }

        [HttpDelete("CntXDiferidosCreacion_Eliminar")]
        public ErrorDto CntXDiferidosCreacion_Eliminar(
            int codEmpresa, int codConta, string usuario, int codDifPlantilla, int codDiferido)
        {
            return _bl.CntXDiferidosCreacion_Eliminar(codEmpresa, codConta, usuario, codDifPlantilla, codDiferido);
        }
    }
}