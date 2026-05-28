using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Fondos;
using Galileo_API.Models.ProGrX.Fondos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Fondos
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmFndBitacoraController : ControllerBase
    {
        private readonly FrmFndBitacoraBl _bl;

        public FrmFndBitacoraController(IConfiguration config)
        {
            _bl = new FrmFndBitacoraBl(config);
        }

        [HttpGet("Fnd_Operadoras_Obtener")]
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Operadoras_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Operadoras_Obtener(CodEmpresa);
        }

        [HttpGet("Fnd_Movimientos_Obtener")]
        public ErrorDto<List<FrmFndBitacoraMovimientoDto>> Fnd_Movimientos_Obtener(int CodEmpresa)
        {
            return _bl.Fnd_Movimientos_Obtener(CodEmpresa);
        }

        [HttpPost("Fnd_Bitacora_Cambios_Obtener")]
        public ErrorDto<List<FrmFndBitacoraCambiosDto>> Fnd_Bitacora_Cambios_Obtener(
            int CodEmpresa,
            [FromBody] FrmFndBitacoraCambiosRequest request)
        {
            return _bl.Fnd_Bitacora_Cambios_Obtener(CodEmpresa, request);
        }

        [HttpPost("Fnd_Bitacora_Cambio_Revisar")]
        public ErrorDto<bool> Fnd_Bitacora_Cambio_Revisar(
            int CodEmpresa,
            [FromBody] FrmFndBitacoraCambioRevisarRequest request)
        {
            return _bl.Fnd_Bitacora_Cambio_Revisar(CodEmpresa, request);
        }

        [HttpPost("Sif_RegistraTags")]
        public ErrorDto<bool> Sif_RegistraTags(
            int CodEmpresa,
            [FromBody] FrmFndBitacoraSifRegistraTagsRequest request)
        {
            return _bl.Sif_RegistraTags(CodEmpresa, request);
        }
    }
}
