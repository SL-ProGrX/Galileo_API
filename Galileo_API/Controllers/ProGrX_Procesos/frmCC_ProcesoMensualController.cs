
using Galileo.Models;
using Galileo.Models.ERROR; 
using Galileo_API.BusinessLogic.ProGrX_Procesos.frmCC_ProcesoMensualBL;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualCargaArchivos;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualEstadoModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCcProcesoMensualController : ControllerBase
    {
        private readonly CcProcesoMensualBL  _bl;
        private readonly CcProcesoMensualArchivosBL _archivosBl;

        public FrmCcProcesoMensualController(IConfiguration config, CcProcesoMensualArchivosBL archivosBl)
        {
            _bl = new CcProcesoMensualBL(config);
            _archivosBl = archivosBl;
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

        [Authorize]
        [HttpPost("CcProcesoMensual_CargarDeducciones")]
        public ErrorDto<CcProcesoMensualCargaDeduccionesResponse> CcProcesoMensual_CargarDeducciones( [FromBody] CcProcesoMensualCargaDeduccionesRequest request)
        {
            return _bl.CcProcesoMensual_CargarDeducciones(request);
        }

        [Authorize]
        [HttpGet("CcProcesoMensual_EstadoActualProceso_Obtener")]
        public ErrorDto<CcProcesoMensualEstadoResponse> CcProcesoMensual_EstadoActualProceso_Obtener(int codEmpresa, int gInstitucion)
        {
            return _bl.CcProcesoMensual_EstadoActualProceso_Obtener(codEmpresa, gInstitucion);
        }

        [Authorize]
        [HttpGet("CcProcesoMensual_DatosInstitucion_Obtener")]
        public ErrorDto<CcProcesoMensualCargaConfigDbModel> CcProcesoMensual_DatosInstitucion_Obtener(int codEmpresa, int codInstitucion)
        {
            return _bl.CcProcesoMensual_DatosInstitucion_Obtener(codEmpresa, codInstitucion);
        }

        [Authorize]
        [HttpPost("CcProcesoMensual_GeneraDeducciones_Ejecutar")]
        public ErrorDto<CcProcesoMensualGeneraDeduccionesResponse> CcProcesoMensual_GeneraDeducciones_Ejecutar(int codEmpresa, [FromBody] CcProcesoMensualGeneraDeduccionesRequest request)
        {
            return _archivosBl.CcProcesoMensual_GeneraDeducciones_Ejecutar(  codEmpresa,  request);
             
        }

        [Authorize]
        [HttpPost("GenerarArchivo")]
        public ErrorDto<CcProcesoMensualArchivoGeneradoModel> GenerarArchivo(int codEmpresa, CcProcesoMensualGeneraArchivoRequest request)
        {
            return _archivosBl.GenerarArchivo(  codEmpresa,  request);
        }

    }
}
