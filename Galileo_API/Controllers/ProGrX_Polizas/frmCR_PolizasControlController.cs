using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX_Polizas;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX_Polizas
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FrmCrPolizasControlController : ControllerBase
    {
        private readonly FrmCrPolizasControlBL _bl;

        public FrmCrPolizasControlController(IConfiguration config)
        {
            _bl = new FrmCrPolizasControlBL(config);
        }

        [HttpGet("Cr_PolizasControl_Obtener")]
        public ErrorDto<PolizaLookupResponseDto> Cr_PolizasControl_Obtener(int CodEmpresa, string CodPoliza)
        {
            return _bl.Cr_PolizasControl_Obtener(CodEmpresa, CodPoliza);
        }

        [HttpGet("ObtenerPolizaScroll")]
        public ErrorDto<PolizaLookupResponseDto?> Cr_PolizasControl_Scroll(
                int codEmpresa,
                string codPolizaActual,
                int direccion)
        {
            return _bl.Cr_PolizasControl_Scroll(codEmpresa, codPolizaActual, direccion);
        }

        [HttpGet("Cr_PolizasControl_Cierres_Lista")]
        public ErrorDto<List<CrPolizasControlCierreRowDto>> Cr_PolizasControl_Cierres_Lista(
         int CodEmpresa,
         string cod_poliza,
         string tipos)
        {
            return _bl.Cr_PolizasControl_Cierres_Lista(CodEmpresa, cod_poliza, tipos);
        }

        [HttpPost("Cr_PolizasControl_Nuevo")]
        public ErrorDto Cr_PolizasControl_Nuevo(int CodEmpresa, CrPolizasControlNuevoRequestDto request)
        {
            return _bl.Cr_PolizasControl_Nuevo(CodEmpresa, request);
        }

        [HttpPost("Cr_PolizasControl_Actualizar")]
        public ErrorDto Cr_PolizasControl_Actualizar(int CodEmpresa)
        {
            return _bl.Cr_PolizasControl_Actualizar(CodEmpresa);
        }

        [HttpPost("Cr_PolizasControl_Cierre_Eliminar")]
        public ErrorDto Cr_PolizasControl_Cierre_Eliminar(
            int CodEmpresa,
            string cod_poliza,
            int cod_corte,
            string Tipo,
            string usuario)
        {
            return _bl.Cr_PolizasControl_Cierre_Eliminar(CodEmpresa, cod_poliza, cod_corte, Tipo, usuario);
        }
    }
}
