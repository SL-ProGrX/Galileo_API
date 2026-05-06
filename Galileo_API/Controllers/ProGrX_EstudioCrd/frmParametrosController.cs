using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_EstudioCrd;
using Galileo_API.Models.ProGrX_EstudioCrd;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_EstudioCrd
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmParametrosController : ControllerBase
    {
        private readonly FrmParametrosBl _bl;

        public FrmParametrosController(IConfiguration config) =>
            _bl = new FrmParametrosBl(config);

        [HttpGet("Parametros_ObtenerOtros")]
        public ErrorDto<ParametrosOtrosData?> Parametros_ObtenerOtros(int codEmpresa)
        {
            return _bl.Parametros_ObtenerOtros(codEmpresa);
        }

        [HttpPost("Parametros_GuardarOtros")]
        public ErrorDto Parametros_GuardarOtros(
            int codEmpresa, string usuario, ParametrosOtrosGuardarRequest request)
        {
            return _bl.Parametros_GuardarOtros(codEmpresa, usuario, request);
        }

        [HttpGet("Parametros_ObtenerCodigos")]
        public ErrorDto<List<ParametrosCodigoData>> Parametros_ObtenerCodigos(
            int codEmpresa, string garantia, string orden)
        {
            return _bl.Parametros_ObtenerCodigos(codEmpresa, garantia, orden);
        }

        [HttpPost("Parametros_ActualizarCodigo")]
        public ErrorDto Parametros_ActualizarCodigo(
            int codEmpresa, string usuario, ParametrosCodigoActualizarRequest request)
        {
            return _bl.Parametros_ActualizarCodigo(codEmpresa, usuario, request);
        }

        [HttpGet("Parametros_ObtenerMembresias")]
        public ErrorDto<List<ParametrosMembresiaData>> Parametros_ObtenerMembresias(
            int codEmpresa, string garantia)
        {
            return _bl.Parametros_ObtenerMembresias(codEmpresa, garantia);
        }

        [HttpPost("Parametros_GuardarMembresias")]
        public ErrorDto Parametros_GuardarMembresias(
            int codEmpresa, string usuario, ParametrosMembresiasGuardarRequest request)
        {
            return _bl.Parametros_GuardarMembresias(codEmpresa, usuario, request);
        }
    }
}