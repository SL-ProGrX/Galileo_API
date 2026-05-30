
using Galileo.Models;
using Galileo.Models.ERROR; 
using Galileo_API.BusinessLogic.ProGrX_Procesos.frmCC_ProcesoMensualBL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualEstadoModels;

namespace Galileo_API.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCC_ProcesoMensualController : ControllerBase
    {
        private readonly CcProcesoMensualBL  _bl;

        public FrmCC_ProcesoMensualController(IConfiguration config)
        {
            _bl = new CcProcesoMensualBL(config);
        }

        [Authorize]
        [HttpGet("CcProcesoMensual_Inicial_Obtener")]
        public ErrorDto<CcProcesoMensualInicialResponse> CcProcesoMensual_Inicial_Obtener(int codEmpresa, int gInstitucion, string usuario)
        {
            return _bl.CcProcesoMensual_Inicial_Obtener(codEmpresa, gInstitucion, usuario);
        }

        [Authorize]
        [HttpGet("CcProcesoMensual_Bitacora_Obtener")]
        public ErrorDto<List<CcProcesoMensualBitacoraDbModel>> CcProcesoMensual_Bitacora_Obtener(int codEmpresa, int gInstitucion, int proceso)
        {
            return _bl.CcProcesoMensual_Bitacora_Obtener(codEmpresa, gInstitucion, proceso);
        }

        [Authorize]
        [HttpGet("CcProcesoMensual_ValidaPaso")]
        public ErrorDto<CcProcesoMensualValidaPasoResponse> CcProcesoMensual_ValidaPaso(int codEmpresa, int codInstitucion, decimal fechaProceso, string transaccion = "08")
        {
            return _bl.CcProcesoMensual_ValidaPaso(codEmpresa, codInstitucion, fechaProceso, transaccion);
        }

    }
}
