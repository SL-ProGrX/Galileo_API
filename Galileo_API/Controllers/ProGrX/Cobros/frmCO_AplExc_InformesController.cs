using Galileo.Models.ERROR;
using Galileo_API.BusinessLogic.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galileo_API.Controllers.ProGrX.Cobros
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrmCOAplExcInformesController : ControllerBase
    {
        private readonly FrmCOAplExcInformesBL BL;

        public FrmCOAplExcInformesController(IConfiguration config)
        {
            BL = new FrmCOAplExcInformesBL(config);
        }

        [Authorize]
        [HttpGet("CO_AplExc_Informes_Catalogo_Obtener")]
        public ErrorDto<List<CoAplExcInformeItemDto>> CO_AplExc_Informes_Catalogo_Obtener(int CodEmpresa)
        {
            return BL.CO_AplExc_Informes_Catalogo_Obtener(CodEmpresa);
        }

        [Authorize]
        [HttpGet("CO_AplExc_Informes_Aplicaciones_F4_Obtener")]
        public ErrorDto<List<CoAplExcAplicacionF4Dto>> CO_AplExc_Informes_Aplicaciones_F4_Obtener(int CodEmpresa, string? texto)
        {
            return BL.CO_AplExc_Informes_Aplicaciones_F4_Obtener(CodEmpresa, texto);
        }

        [Authorize]
        [HttpGet("CO_AplExc_Informes_Personas_F4_Obtener")]
        public ErrorDto<List<CoAplExcPersonaF4Dto>> CO_AplExc_Informes_Personas_F4_Obtener(int CodEmpresa, string? texto)
        {
            return BL.CO_AplExc_Informes_Personas_F4_Obtener(CodEmpresa, texto);
        }
    }
}