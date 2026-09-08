using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Conciliacion;
using Galileo_API.Models.ProGrX_Conciliacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Conciliacion
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmVerificaAsientosDocumentoController : ControllerBase
    {
        private readonly FrmVerificaAsientosDocumentoBL _bl;

        public FrmVerificaAsientosDocumentoController(IConfiguration config)
        {
            _bl = new FrmVerificaAsientosDocumentoBL(config);
        }

        [Authorize]
        [HttpGet("ASE_VerificaAsientosDocumento_Inicial_Obtener")]
        public ErrorDto<AseVerificaAsientosDocumentoInicialData> ASE_VerificaAsientosDocumento_Inicial_Obtener(int CodEmpresa)
        {
            return _bl.ASE_VerificaAsientosDocumento_Inicial_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("ASE_VerificaAsientosDocumento_Lista_Obtener")]
        public ErrorDto<AseVerificaAsientosDocumentoListaResult> ASE_VerificaAsientosDocumento_Lista_Obtener(int CodEmpresa, [FromBody] AseVerificaAsientosDocumentoListaRequest? request)
        {
            return _bl.ASE_VerificaAsientosDocumento_Lista_Obtener(CodEmpresa, request);
        }

        [Authorize]
        [HttpPost("ASE_VerificaAsientosDocumento_Lista_Export")]
        public ErrorDto<AseVerificaAsientosDocumentoListaResult> ASE_VerificaAsientosDocumento_Lista_Export(int CodEmpresa, [FromBody] AseVerificaAsientosDocumentoListaRequest? request)
        {
            return _bl.ASE_VerificaAsientosDocumento_Lista_Export(CodEmpresa, request);
        }
    }
}