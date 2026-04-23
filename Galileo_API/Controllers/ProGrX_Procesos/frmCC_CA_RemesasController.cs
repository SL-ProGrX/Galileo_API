using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Procesos;
using Galileo_API.Models.ProGrX_Procesos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Procesos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCcCaRemesasController : ControllerBase
    {
        private readonly FrmCcCaRemesasBL _bl;

        public FrmCcCaRemesasController(IConfiguration config)
        {
            _bl = new FrmCcCaRemesasBL(config);
        }

        [HttpGet("CcCaRemesas_Catalogos_Obtener")]
        public ErrorDto<CcCaRemesasCatalogosResponse> CcCaRemesas_Catalogos_Obtener(int CodEmpresa)
        {
            return _bl.CcCaRemesas_Catalogos_Obtener(CodEmpresa);
        }

        [HttpPost("CcCaRemesas_Envio_Consulta")]
        public ErrorDto<List<CcCaRemesasEnvioConsultaData>> CcCaRemesas_Envio_Consulta(
            int CodEmpresa,
            [FromBody] CcCaRemesasEnvioConsultaRequest request)
        {
            return _bl.CcCaRemesas_Envio_Consulta(CodEmpresa, request);
        }

        [HttpGet("CcCaRemesas_Recibe_Pendientes_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> CcCaRemesas_Recibe_Pendientes_Obtener(int CodEmpresa)
        {
            return _bl.CcCaRemesas_Recibe_Pendientes_Obtener(CodEmpresa);
        }

        [HttpGet("CcCaRemesas_Recibe_Detalle_Obtener")]
        public ErrorDto<List<CcCaRemesasRecibeDetalleData>> CcCaRemesas_Recibe_Detalle_Obtener(int CodEmpresa, long remesa)
        {
            return _bl.CcCaRemesas_Recibe_Detalle_Obtener(CodEmpresa, remesa);
        }
    }
}
