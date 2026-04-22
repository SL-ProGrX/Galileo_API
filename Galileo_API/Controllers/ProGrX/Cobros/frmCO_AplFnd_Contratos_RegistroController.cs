using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCOAplFndContratosRegistroController : ControllerBase
    {
        private readonly FrmCOAplFndContratosRegistroBL _bl;

        public FrmCOAplFndContratosRegistroController(IConfiguration config)
        {
            _bl = new FrmCOAplFndContratosRegistroBL(config);
        }

        [HttpGet("CO_AplFnd_Contratos_Registro_Lista_Obtener")]
        public ErrorDto<List<CoAplFndContratosRegistroListaRow>> CO_AplFnd_Contratos_Registro_Lista_Obtener(
            int CodEmpresa,
            string request)
        {
            return _bl.CO_AplFnd_Contratos_Registro_Lista_Obtener(CodEmpresa, request);
        }

        [HttpGet("CO_AplFnd_Contratos_Registro_Obtener")]
        public ErrorDto<CoAplFndContratosRegistroData> CO_AplFnd_Contratos_Registro_Obtener(
            int CodEmpresa,
            string request)
        {
            return _bl.CO_AplFnd_Contratos_Registro_Obtener(CodEmpresa, request);
        }

        [HttpGet("CO_AplFnd_Contratos_Registro_Creditos_Obtener")]
        public ErrorDto<List<CoAplFndContratosRegistroCreditoRow>> CO_AplFnd_Contratos_Registro_Creditos_Obtener(
            int CodEmpresa,
            string request)
        {
            return _bl.CO_AplFnd_Contratos_Registro_Creditos_Obtener(CodEmpresa, request);
        }

        [HttpPost("CO_AplFnd_Contratos_Registro_Guardar")]
        public ErrorDto<CoAplFndContratosRegistroGuardarResponse> CO_AplFnd_Contratos_Registro_Guardar(
            int CodEmpresa,
            [FromBody] CoAplFndContratosRegistroGuardarRequest request)
        {
            return _bl.CO_AplFnd_Contratos_Registro_Guardar(CodEmpresa, request);
        }

        [HttpGet("CO_AplFnd_Contratos_Registro_Personas_F4_Obtener")]
        public ErrorDto<List<CoAplFndContratosRegistroPersonaF4Row>> CO_AplFnd_Contratos_Registro_Personas_F4_Obtener(
            int CodEmpresa,
            string request)
        {
            return _bl.CO_AplFnd_Contratos_Registro_Personas_F4_Obtener(CodEmpresa, request);
        }

        [HttpPost("CO_AplFnd_Contratos_Registro_Carga_Lote")]
        public ErrorDto<CoAplFndContratosRegistroCargaLoteResponse> CO_AplFnd_Contratos_Registro_Carga_Lote(
    int CodEmpresa,
    [FromBody] CoAplFndContratosRegistroCargaLoteRequest request)
        {
            return _bl.CO_AplFnd_Contratos_Registro_Carga_Lote(CodEmpresa, request);
        }
    }
}
