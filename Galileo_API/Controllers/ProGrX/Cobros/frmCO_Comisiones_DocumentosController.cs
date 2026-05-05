using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Galileo.BusinessLogic.ProGrX.Cobros;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cobros;

namespace Galileo.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]

    public class FrmCoComisionesDocumentosController : ControllerBase
    {
        private readonly FrmCoComisionesDocumentosBL _bl;

        public FrmCoComisionesDocumentosController(IConfiguration config)
        {
            _bl = new FrmCoComisionesDocumentosBL(config);
        }

        [Authorize]
        [HttpGet("CO_ComisionesDocumento_Obtener")]
        public ErrorDto<List<CoComisionesDocumentosData>> CO_ComisionesDocumento_Obtener(int CodEmpresa)
        {
            return _bl.CO_ComisionesDocumento_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpPost("CO_ComisionesDocumento_Insertar")]
        public ErrorDto CO_ComisionesDocumento_Insertar(int CodEmpresa, string usuario, string tipo_documento)
        {
            return _bl.CO_ComisionesDocumento_Insertar(CodEmpresa, usuario, tipo_documento);
        }

        [Authorize]
        [HttpDelete("CO_ComisionesDocumento_Delete")]
        public ErrorDto CO_ComisionesDocumento_Delete(int CodEmpresa, string usuario, string tipo_documento)
        {
            return _bl.CO_ComisionesDocumento_Delete(CodEmpresa, usuario, tipo_documento);
        }

    }
}
