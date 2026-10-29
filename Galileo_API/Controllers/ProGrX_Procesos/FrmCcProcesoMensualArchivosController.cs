
using Galileo.Models;
using Galileo.Models.ERROR; 
using Galileo_API.BusinessLogic.ProGrX_Procesos.frmCC_ProcesoMensualBL;
using Galileo_API.Extensions;
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

        [ValidateAntiForgeryToken]
        [HttpPost("CcProcesoMensual_GeneraDeducciones_Ejecutar")]
        public ErrorDto<CcProcesoMensualGeneraDeduccionesResponse> CcProcesoMensual_GeneraDeducciones_Ejecutar(
            int codEmpresa,
            [FromBody] CcProcesoMensualGeneraDeduccionesRequest request)
        {
            // El usuario ejecutor es el de la sesión (en VB6 es glogon.Usuario), no el enviado por el cliente.
            request.Usuario = User.ObtenerUsuario();

            return _archivosBl.CcProcesoMensual_GeneraDeducciones_Ejecutar(
                codEmpresa,
                request);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("GenerarArchivo")]
        public ErrorDto<CcProcesoMensualArchivoGeneradoModel> GenerarArchivo_Ejecutar(
            int codEmpresa,
            [FromBody] CcProcesoMensualGeneraArchivoRequest request)
        {
            // Usuario y empresa son los de la sesión (en VB6 glogon.Usuario y gPortal.Empresa_Id); la empresa ya fue autorizada por EmpresaAccessFilter.
            request.Usuario = User.ObtenerUsuario();
            request.EmpresaId = codEmpresa;

            return _archivosBl.GenerarArchivo_Ejecutar(
                codEmpresa,
                request);
        }
    }
}

