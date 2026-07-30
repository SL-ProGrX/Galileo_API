using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Contabilidad;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Contabilidad
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCntXDivisasDCController : ControllerBase
    {
        private readonly FrmCntXDivisasDCBl _bl;

        public FrmCntXDivisasDCController(IConfiguration config)
        {
            _bl = new FrmCntXDivisasDCBl(config);
        }

        [Authorize]
        [HttpGet("ObtenerDivisas")]
        public ErrorDto<List<DivisaDto>> ObtenerDivisas(int codEmpresa, int codContabilidad)
        {
            return _bl.ObtenerDivisas(codEmpresa, codContabilidad);
        }

        [Authorize]
        [HttpGet("ObtenerTiposCambio")]
        public ErrorDto<List<TipoCambioDto>> ObtenerTiposCambio(int codEmpresa, int codContabilidad, int periodoAnio,int periodoMes,string codDivisa)
        {
            return _bl.ObtenerTiposCambio(codEmpresa,codContabilidad,periodoAnio,periodoMes,codDivisa
            );
        }

        [Authorize]
        [HttpPost("Procesar")]
        public ErrorDto Procesar(int codEmpresa, int codContabilidad, int periodoAnio, int periodoMes,string usuario,
            ProcesarDiferencialRequestDto request)
        {
            return _bl.Procesar(codEmpresa,codContabilidad,periodoAnio,periodoMes,request,usuario
            );
        }
    }
}
