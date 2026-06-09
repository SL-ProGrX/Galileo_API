
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

    public class FrmCcProcesoMensualArchivosController : ControllerBase
    {
        private readonly CcProcesoMensualArchivosBL _archivosBl;

        public FrmCcProcesoMensualArchivosController(
            CcProcesoMensualArchivosBL archivosBl)
        {
            _archivosBl = archivosBl;
        }

        [HttpPost("CcProcesoMensual_GeneraDeducciones_Ejecutar")]
        public ErrorDto<CcProcesoMensualGeneraDeduccionesResponse> CcProcesoMensual_GeneraDeducciones_Ejecutar(
            int codEmpresa,
            [FromBody] CcProcesoMensualGeneraDeduccionesRequest request)
        {
            return _archivosBl.CcProcesoMensual_GeneraDeducciones_Ejecutar(
                codEmpresa,
                request);
        }

        [HttpPost("GenerarArchivo")]
        public ErrorDto<CcProcesoMensualArchivoGeneradoModel> GenerarArchivo_Ejecutar(
            int codEmpresa,
            [FromBody] CcProcesoMensualGeneraArchivoRequest request)
        {
            return _archivosBl.GenerarArchivo_Ejecutar(
                codEmpresa,
                request);
        }
    }
}

