using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Creditos;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Creditos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrAbonosComprobanteController : ControllerBase
    {
        private readonly FrmCrAbonosComprobanteBl _BL;

        public FrmCrAbonosComprobanteController(IConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            _BL = new FrmCrAbonosComprobanteBl(config);
        }

        [HttpGet("CrAbonosComprobante_Operacion_Obtener")]
        public ErrorDto<CrAbonosComprobanteOperacionData> CrAbonosComprobante_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            return _BL.CrAbonosComprobante_Operacion_Obtener(codEmpresa, operacion);
        }

        [HttpGet("CrAbonosComprobante_Operaciones_Lista_Obtener")]
        public ErrorDto<List<CrAbonosComprobanteOperacionListaItem>>
            CrAbonosComprobante_Operaciones_Lista_Obtener(int codEmpresa)
        {
            return _BL.CrAbonosComprobante_Operaciones_Lista_Obtener(codEmpresa);
        }

        [HttpGet("CrAbonosComprobante_TiposDocumento_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>>
            CrAbonosComprobante_TiposDocumento_Obtener(int codEmpresa)
        {
            return _BL.CrAbonosComprobante_TiposDocumento_Obtener(codEmpresa);
        }

        [HttpPost("CrAbonosComprobante_Aplicar")]
        public ErrorDto<CrAbonosComprobanteAplicarResultadoData> CrAbonosComprobante_Aplicar(
            int codEmpresa,
            CrAbonosComprobanteAplicarRequest request)
        {
            return _BL.CrAbonosComprobante_Aplicar(codEmpresa, request);
        }
    }
}
