using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros.Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCOAplExcContratosRegistroController : ControllerBase
    {
        private readonly FrmCOAplExcContratosRegistroBL _bl;

        public FrmCOAplExcContratosRegistroController(IConfiguration config)
        {
            _bl = new FrmCOAplExcContratosRegistroBL(config);
        }

        [HttpGet("CO_AplExc_Contratos_Registro_Lista_Obtener")]
        public ErrorDto<List<CoAplExcContratosRegistroListaRow>> CO_AplExc_Contratos_Registro_Lista_Obtener(
            int CodEmpresa,
            string request)
        {
            return _bl.CO_AplExc_Contratos_Registro_Lista_Obtener(CodEmpresa, request);
        }

        [HttpGet("CO_AplExc_Contratos_Registro_Obtener")]
        public ErrorDto<CoAplExcContratosRegistroData> CO_AplExc_Contratos_Registro_Obtener(
            int CodEmpresa,
            string request)
        {
            return _bl.CO_AplExc_Contratos_Registro_Obtener(CodEmpresa, request);
        }

        [HttpGet("CO_AplExc_Contratos_Registro_Creditos_Obtener")]
        public ErrorDto<List<CoAplExcContratosRegistroCreditoRow>> CO_AplExc_Contratos_Registro_Creditos_Obtener(
            int CodEmpresa,
            string request)
        {
            return _bl.CO_AplExc_Contratos_Registro_Creditos_Obtener(CodEmpresa, request);
        }

        [HttpPost("CO_AplExc_Contratos_Registro_Guardar")]
        public ErrorDto<CoAplExcContratosRegistroGuardarResponse> CO_AplExc_Contratos_Registro_Guardar(
            int CodEmpresa,
            [FromBody] CoAplExcContratosRegistroGuardarRequest request)
        {
            return _bl.CO_AplExc_Contratos_Registro_Guardar(CodEmpresa, request);
        }
    }
}
